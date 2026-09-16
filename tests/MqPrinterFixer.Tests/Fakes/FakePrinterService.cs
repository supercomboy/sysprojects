using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.Tests.Fakes;

internal sealed class FakePrinterService : IPrinterService
{
    public List<PrinterInfo> PrintersToReturn { get; } = new();

    public Task<IReadOnlyList<PrinterInfo>> GetAllPrintersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PrinterInfo>>(PrintersToReturn);

    public Task<PrinterInfo?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(PrintersToReturn.FirstOrDefault(p => p.IsDefault));

    public static PrinterInfo Build(
        string name,
        bool isDefault = false,
        PrinterStatus status = PrinterStatus.Ready,
        PrinterConnectionType type = PrinterConnectionType.Usb,
        string port = "USB001",
        bool shared = false,
        string shareName = "")
        => new(
            Name: name,
            Status: status,
            IsDefault: isDefault,
            ConnectionType: type,
            PortName: port,
            DriverName: "FakeDriver",
            IsShared: shared,
            ShareName: shareName,
            SourceHost: string.Empty,
            PrinterPath: string.Empty,
            Location: string.Empty,
            Comment: string.Empty);
}