using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class PrinterRoleDetectionService : IPrinterRoleDetectionService
{
    private readonly IPrinterService _printerService;

    public PrinterRoleDetectionService(IPrinterService printerService)
    {
        _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
    }

    public async Task<PrinterRoleDetectionResult> DetectAsync(
        RoleDetectionMode mode = RoleDetectionMode.Auto,
        CancellationToken cancellationToken = default)
    {
        var printers = await _printerService.GetAllPrintersAsync(cancellationToken);
        var local = Environment.MachineName;

        var detected = PrinterRoleMapper.ComputeRole(printers, local);
        var effective = PrinterRoleMapper.ApplyOverride(detected, mode);

        int localShared = 0;
        int remoteShared = 0;
        foreach (var p in printers)
        {
            if (PrinterRoleMapper.IsRemoteShared(p, local)) remoteShared++;
            else if (PrinterRoleMapper.IsLocalShared(p))    localShared++;
        }

        return new PrinterRoleDetectionResult(
            DetectedRole: detected,
            EffectiveRole: effective,
            Mode: mode,
            TotalPrinterCount: printers.Count,
            LocalSharedPrinterCount: localShared,
            RemoteSharedPrinterCount: remoteShared);
    }
}