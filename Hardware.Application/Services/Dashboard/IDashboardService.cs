using Hardware.Application.DTOs.Dashboard;

namespace Hardware.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
