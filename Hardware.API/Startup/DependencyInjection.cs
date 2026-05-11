using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Hardware.API.Filters;
using Hardware.Application.Mappings;
using Hardware.Application.Services.Authentication;
using Hardware.Application.Services.Dashboard;
using Hardware.Application.Services.Identity;
using Hardware.Application.Services.Inventory;
using Hardware.Application.Services.Purchasing;
using Hardware.Application.Services.Sales;
using Hardware.Infrastructure.Identity;
using Hardware.Infrastructure.Services.Dashboard;
using Hardware.Infrastructure.Services.Inventory;
using Hardware.Infrastructure.Services.Purchasing;
using Hardware.Infrastructure.Services.Sales;
using Hardware.Domain.Entities.Identity;
using Hardware.Domain.Interfaces.Repositories;
using Hardware.Domain.Interfaces.Services;
using Hardware.Infrastructure.BackgroundJobs;
using Hardware.Infrastructure.Caching;
using Hardware.Infrastructure.Data;
using Hardware.Infrastructure.Data.Interceptors;
using Hardware.Infrastructure.Identity;
using Hardware.Infrastructure.Repositories;
using Hardware.Shared.Configuration;
using Hardware.Shared.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace Hardware.API.Startup;

public static class DependencyInjection
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();

        services.RegisterOptions(configuration);
        services.RegisterDbContext(configuration);
        services.RegisterIdentity();
        services.RegisterAuthentication(configuration);
        services.RegisterAuthorization();
        services.RegisterRedis(configuration);
        services.RegisterHangfire(configuration);
        services.RegisterSignalR(configuration);
        services.RegisterAutoMapper();
        services.RegisterValidators();
        services.RegisterApplicationServices();
        services.RegisterInfrastructureServices();
        services.RegisterCors(configuration);
        services.RegisterRateLimiting(configuration);
        services.RegisterOutputCache(configuration);
        services.RegisterResponseCompression();
        services.RegisterSwagger();
        services.RegisterControllers();
        services.RegisterHealthChecks(configuration);

        return services;
    }

    private static void RegisterOptions(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddOptions<JwtSettings>().Bind(cfg.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<AppSettings>().Bind(cfg.GetSection(AppSettings.SectionName)).ValidateOnStart();
        services.AddOptions<RedisSettings>().Bind(cfg.GetSection(RedisSettings.SectionName)).ValidateOnStart();
        services.AddOptions<HangfireSettings>().Bind(cfg.GetSection(HangfireSettings.SectionName)).ValidateOnStart();
        services.AddOptions<CorsSettings>().Bind(cfg.GetSection(CorsSettings.SectionName)).ValidateOnStart();
        services.AddOptions<RateLimitingSettings>().Bind(cfg.GetSection(RateLimitingSettings.SectionName))
            .ValidateOnStart();
        services.AddOptions<EmailSettings>().Bind(cfg.GetSection(EmailSettings.SectionName)).ValidateOnStart();
        // SeedData validated lazily inside the seeder (only when EnableSeeding=true).
        services.AddOptions<SeedDataSettings>().Bind(cfg.GetSection(SeedDataSettings.SectionName));
    }

    private static void RegisterDbContext(this IServiceCollection services, IConfiguration cfg)
    {
        // Interceptors registered as singleton because AddDbContextPool's factory `sp`
        // is the root provider and cannot resolve scoped services. Both interceptors
        // are stateless and safe as singletons (they read per-request data via
        // singleton IHttpContextAccessor).
        services.AddSingleton<SlowQueryInterceptor>();
        services.AddSingleton<AuditInterceptor>();

        var connectionString = cfg.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "ConnectionStrings:DefaultConnection is required.");

        services.AddDbContextPool<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                npgsql.CommandTimeout(30);
                npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                npgsql.MinBatchSize(4);
                npgsql.MaxBatchSize(42);
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<SlowQueryInterceptor>());
        });
    }

    private static void RegisterIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void RegisterAuthentication(this IServiceCollection services, IConfiguration cfg)
    {
        var jwt = cfg.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Jwt configuration section is required.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecurityKey)),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        if (!path.StartsWithSegments("/hubs") && !path.StartsWithSegments("/hangfire"))
                            return Task.CompletedTask;

                        var token = context.Request.Query["access_token"].FirstOrDefault();

                        if (string.IsNullOrEmpty(token) && path.StartsWithSegments("/hangfire"))
                            token = context.Request.Cookies["hw_hangfire_jwt"];

                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async ctx =>
                    {
                        var store = ctx.HttpContext.RequestServices.GetService<ISessionRevocationStore>();
                        if (store is null) return;
                        var sid = ctx.Principal?.FindFirst(CustomClaimTypes.SessionId)?.Value;
                        if (!string.IsNullOrEmpty(sid) && await store.IsRevokedAsync(sid))
                            ctx.Fail("Session revoked");
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers["Token-Expired"] = "true";
                        return Task.CompletedTask;
                    }
                };
            });
    }

    private static void RegisterAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin",       p => p.RequireRole(RoleConstants.AdminRoles));
            options.AddPolicy("RequireManager",     p => p.RequireRole(RoleConstants.ManagerRoles));
            options.AddPolicy("RequireCashier",     p => p.RequireRole(RoleConstants.CashierRoles));
            options.AddPolicy("RequireStoreKeeper", p => p.RequireRole(RoleConstants.StoreKeeperRoles));
        });
    }

    private static void RegisterRedis(this IServiceCollection services, IConfiguration cfg)
    {
        var redisConnection = cfg.GetConnectionString("Redis");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
            try
            {
                if (string.IsNullOrWhiteSpace(redisConnection))
                {
                    logger.LogWarning("Redis connection string is missing; cache pattern operations disabled");
                    return null!;
                }

                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false;
                options.AllowAdmin = true;
                return ConnectionMultiplexer.Connect(options);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to connect to Redis; running without distributed cache backplane");
                return null!;
            }
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "Hardware:";
        });

        services.AddMemoryCache();
        services.AddScoped<ICacheService, RedisCacheService>();
    }

    private static void RegisterHangfire(this IServiceCollection services, IConfiguration cfg)
    {
        var redisConnection = cfg.GetConnectionString("Redis");
        var hangfireSettings = cfg.GetSection(HangfireSettings.SectionName).Get<HangfireSettings>() ??
                               new HangfireSettings();

        services.AddHangfire(c =>
        {
            c.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            if (!string.IsNullOrWhiteSpace(redisConnection))
                c.UseRedisStorage(redisConnection, new RedisStorageOptions
                {
                    Prefix = hangfireSettings.KeyPrefix
                });
        });

        if (!string.IsNullOrWhiteSpace(redisConnection))
            services.AddHangfireServer(options =>
            {
                options.Queues = hangfireSettings.Queues;
                options.WorkerCount = Environment.ProcessorCount * hangfireSettings.WorkerCountMultiplier;
                options.ServerName = $"Hardware-{Environment.MachineName}";
            });
    }

    private static void RegisterSignalR(this IServiceCollection services, IConfiguration cfg)
    {
        var redisConnection = cfg.GetConnectionString("Redis");
        var builder = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 1024 * 1024;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        }).AddMessagePackProtocol();

        if (!string.IsNullOrWhiteSpace(redisConnection))
            builder.AddStackExchangeRedis(redisConnection,
                options => { options.Configuration.ChannelPrefix = RedisChannel.Literal("Hardware:signalr"); });
    }

    private static void RegisterAutoMapper(this IServiceCollection services) =>
        services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

    private static void RegisterValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AutoMapperProfile>();
        services.AddFluentValidationAutoValidation(o => { o.DisableDataAnnotationsValidation = true; });
        services.AddFluentValidationClientsideAdapters();
    }

    private static void RegisterApplicationServices(this IServiceCollection services)
    {
        // Auth service implementation lives in Infrastructure (needs DbContext + UserManager).
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        // Dashboard
        services.AddScoped<IDashboardService, DashboardService>();

        // Inventory
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IStockService, StockService>();

        // Sales
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Purchasing
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
    }

    private static void RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Singleton because AuditInterceptor (singleton) depends on it; safe — only reads
        // from singleton IHttpContextAccessor, no instance state.
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ISessionRevocationStore, RedisSessionRevocationStore>();
        services.AddScoped<RefreshTokenCleanupJob>();
    }

    private static void RegisterCors(this IServiceCollection services, IConfiguration cfg)
    {
        var corsSettings = cfg.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        services.AddCors(options =>
        {
            options.AddPolicy(corsSettings.PolicyName, policy =>
            {
                if (corsSettings.AllowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
                else
                {
                    policy.WithOrigins(corsSettings.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    if (corsSettings.AllowCredentials) policy.AllowCredentials();
                }
            });
        });
    }

    private static void RegisterRateLimiting(this IServiceCollection services, IConfiguration cfg)
    {
        var rateLimit = cfg.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>() ??
                        new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("default", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimit.DefaultPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("auth_endpoints", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimit.AuthPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("admin", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User.Identity?.Name ??
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimit.AdminPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });
    }

    private static void RegisterOutputCache(this IServiceCollection services, IConfiguration cfg)
    {
        var redisConnection = cfg.GetConnectionString("Redis");

        var outputBuilder = services.AddOutputCache(options =>
        {
            options.AddBasePolicy(b => b.Cache().Expire(TimeSpan.FromSeconds(60)));
            options.AddPolicy("short", b => b.Cache().Expire(TimeSpan.FromSeconds(30)));
            options.AddPolicy("long", b => b.Cache().Expire(TimeSpan.FromMinutes(10)));
        });

        if (!string.IsNullOrWhiteSpace(redisConnection))
            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "Hardware:outputcache:";
            });
    }

    private static void RegisterResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
    }

    private static void RegisterSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Hardware API",
                Version = "v1",
                Description = "Hardware API. Supports JWT Bearer authentication.",
                Contact = new OpenApiContact
                {
                    Name = "Dhanu",
                    Email = "support@example.com"
                }
            });

            // JWT Bearer security scheme — adds the green Authorize button to Swagger UI.
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste the JWT access token returned by POST /api/v1/auth/login. " +
                              "Swagger UI will prefix it with 'Bearer ' automatically.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [jwtSecurityScheme] = Array.Empty<string>()
            });
        });
    }

    private static void RegisterControllers(this IServiceCollection services)
    {
        services.AddScoped<ValidationFilter>();
        services.AddControllers(options => { options.Filters.Add<ValidationFilter>(); });

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
    }

    private static void RegisterHealthChecks(this IServiceCollection services, IConfiguration cfg)
    {
        var checks = services.AddHealthChecks();
        var defaultConn = cfg.GetConnectionString("DefaultConnection");
        var redisConn = cfg.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(defaultConn))
            checks.AddNpgSql(defaultConn, name: "postgres", tags: ["ready"]);

        if (!string.IsNullOrWhiteSpace(redisConn))
            checks.AddRedis(redisConn, "redis", tags: ["ready"]);
    }
}
