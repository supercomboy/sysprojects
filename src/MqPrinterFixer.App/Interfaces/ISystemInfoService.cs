using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

/// <summary>
/// Đọc thông tin hệ thống read-only. Không yêu cầu elevation.
/// </summary>
public interface ISystemInfoService
{
    Task<ComputerInfo> GetComputerInfoAsync(CancellationToken cancellationToken = default);
}