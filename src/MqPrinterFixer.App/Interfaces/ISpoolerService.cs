using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface ISpoolerService
{
    /// <summary>Lấy trạng thái hiện tại của Print Spooler.</summary>
    Task<SpoolerInfo> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Khởi động Print Spooler. Yêu cầu Admin.</summary>
    Task<NetworkChangeResult> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Khởi động lại Print Spooler. Yêu cầu Admin.</summary>
    Task<NetworkChangeResult> RestartAsync(CancellationToken cancellationToken = default);
}