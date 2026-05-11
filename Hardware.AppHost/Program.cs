using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Hardware_API>("api")
    // .WithReference(postgres, "DefaultConnection")
    .WithReference(cache, "Redis")
    // .WaitFor(postgres)
    .WaitFor(cache)
    .WithExternalHttpEndpoints();

builder.Build().Run();
