using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

/// <summary>
/// Chạy PowerShell command và trả về kết quả có cấu trúc.
/// KHÔNG dùng cho mọi thứ - chỉ khi Windows admin operation thực sự phù hợp.
/// </summary>
public interface IPowerShellService
{
    /// <summary>
    /// Chạy một đoạn script PowerShell ngắn (không có interactive).
    /// </summary>
    Task<PowerShellResult> RunAsync(
        string script,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
}