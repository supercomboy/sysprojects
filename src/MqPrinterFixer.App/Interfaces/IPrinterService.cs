using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface IPrinterService
{
    /// <summary>Liệt kê tất cả máy in đã cài cho user hiện tại.</summary>
    Task<IReadOnlyList<PrinterInfo>> GetAllPrintersAsync(CancellationToken cancellationToken = default);

    /// <summary>Trả về máy in mặc định, hoặc null nếu không có.</summary>
    Task<PrinterInfo?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default);
}