using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface INetworkService
{
    /// <summary>
    /// Lấy thông tin mạng đang hoạt động (adapter, IPv4, category).
    /// Read-only. Trả <see cref="NetworkInfo.Empty"/> nếu không có kết nối.
    /// </summary>
    Task<NetworkInfo> GetActiveNetworkAsync(CancellationToken cancellationToken = default);

    // ----- Các action thay đổi sẽ thêm ở Phase 13 -----
    // Task<NetworkCategoryChangeResult> ChangePublicToPrivateAsync(...);
    // Task<bool> TestHostReachabilityAsync(string host, ...);
}