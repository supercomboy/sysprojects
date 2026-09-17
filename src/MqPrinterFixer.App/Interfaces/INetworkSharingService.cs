using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface INetworkSharingService
{
    /// <summary>Đọc state sharing hiện tại.</summary>
    Task<SharingState> GetSharingStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Build preview cho một action. Không thay đổi hệ thống.</summary>
    Task<NetworkChangePreview> PreviewChangeAsync(
        NetworkChangeAction action,
        CancellationToken cancellationToken = default);

    /// <summary>Apply action. Chạy script, sau đó Verify.</summary>
    Task<NetworkChangeResult> ApplyChangeAsync(
        NetworkChangePreview preview,
        CancellationToken cancellationToken = default);
}