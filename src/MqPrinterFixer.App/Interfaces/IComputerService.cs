using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface IComputerService
{
    /// <summary>
    /// Đổi tên máy. Yêu cầu process đang elevated.
    /// Sau khi thành công, <see cref="RenameComputerResult.RestartRequired"/> = true.
    /// </summary>
    Task<RenameComputerResult> RenameComputerAsync(
        string newName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Khởi động lại máy ngay lập tức (force, không hỏi app khác).
    /// Yêu cầu elevation.
    /// </summary>
    Task<PowerShellResult> RestartComputerNowAsync(
        CancellationToken cancellationToken = default);
}