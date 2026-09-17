using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Interfaces;

public interface IPrinterRoleDetectionService
{
    /// <summary>
    /// Dò role hiện tại. Mode mặc định là Auto.
    /// Không thay đổi bất kỳ cấu hình Windows printer nào.
    /// </summary>
    Task<PrinterRoleDetectionResult> DetectAsync(
        RoleDetectionMode mode = RoleDetectionMode.Auto,
        CancellationToken cancellationToken = default);
}