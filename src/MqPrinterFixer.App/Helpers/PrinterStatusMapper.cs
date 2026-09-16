using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Map WMI Win32_Printer state → PrinterStatus chuẩn hóa.
/// Tham khảo: Win32_Printer.PrinterStatus (1-7) và DetectedErrorState (0-11).
/// </summary>
public static class PrinterStatusMapper
{
    // PrinterStatus codes
    private const int PsOther            = 1;
    private const int PsUnknown          = 2;
    private const int PsIdle             = 3;
    private const int PsPrinting         = 4;
    private const int PsWarmup           = 5;
    private const int PsStoppedPrinting  = 6;
    private const int PsOffline          = 7;

    // DetectedErrorState codes
    private const int DesUnknown    = 0;
    private const int DesOther      = 1;
    private const int DesNoError    = 2;

    public static PrinterStatus Map(bool workOffline, int? printerStatus, int? detectedErrorState)
    {
        if (workOffline)
        {
            return PrinterStatus.Offline;
        }

        // Nếu có lỗi rõ ràng từ DetectedErrorState → Error
        if (detectedErrorState.HasValue &&
            detectedErrorState.Value != DesUnknown &&
            detectedErrorState.Value != DesNoError)
        {
            // DesOther (1) là "Other" – không rõ ràng, coi như Unknown.
            if (detectedErrorState.Value == DesOther)
            {
                return PrinterStatus.Unknown;
            }
            return PrinterStatus.Error;
        }

        // Dựa vào PrinterStatus chính
        return printerStatus switch
        {
            PsIdle            => PrinterStatus.Ready,
            PsPrinting        => PrinterStatus.Ready,
            PsWarmup          => PrinterStatus.Ready,
            PsStoppedPrinting => PrinterStatus.Paused,
            PsOffline         => PrinterStatus.Offline,
            PsOther           => PrinterStatus.Unknown,
            PsUnknown         => PrinterStatus.Unknown,
            null              => PrinterStatus.Unknown,
            _                 => PrinterStatus.Unknown
        };
    }

    /// <summary>
    /// Nhãn hiển thị (UI) cho PrinterStatus.
    /// </summary>
    public static string ToDisplayName(PrinterStatus status) => status switch
    {
        PrinterStatus.Ready   => "Ready",
        PrinterStatus.Offline => "Offline",
        PrinterStatus.Paused  => "Paused",
        PrinterStatus.Error   => "Error",
        _                     => "Unknown"
    };

    /// <summary>
    /// Nhãn hiển thị (UI) cho PrinterConnectionType.
    /// </summary>
    public static string ToDisplayName(PrinterConnectionType type) => type switch
    {
        PrinterConnectionType.Usb           => "USB",
        PrinterConnectionType.TcpIp         => "TCP/IP",
        PrinterConnectionType.Wsd           => "WSD",
        PrinterConnectionType.NetworkShared => "Network Shared",
        PrinterConnectionType.Virtual       => "Virtual",
        _                                   => "Unknown"
    };
}