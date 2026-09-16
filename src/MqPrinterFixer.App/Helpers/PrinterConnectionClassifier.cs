using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Phân loại port name thành PrinterConnectionType. Pure logic, testable.
/// </summary>
public static class PrinterConnectionClassifier
{
    public static PrinterConnectionType Classify(string? portName, string? printerName)
    {
        var port = portName?.Trim() ?? string.Empty;
        var name = printerName?.Trim() ?? string.Empty;

        // 1. UNC path (\\HOST\Share) in port → Network Shared
        if (port.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return PrinterConnectionType.NetworkShared;
        }

        // 2. Virtual ports (Microsoft Print to PDF, XPS, Fax...)
        if (IsVirtualPort(port) || IsVirtualPrinterName(name))
        {
            return PrinterConnectionType.Virtual;
        }

        // 3. USB ports: USB001, USB002...
        if (port.StartsWith("USB", StringComparison.OrdinalIgnoreCase))
        {
            return PrinterConnectionType.Usb;
        }

        // 4. WSD ports: WSD-xxxxx
        if (port.StartsWith("WSD", StringComparison.OrdinalIgnoreCase))
        {
            return PrinterConnectionType.Wsd;
        }

        // 5. TCP/IP: port name starting with "IP_" hoặc bare IP address
        if (port.StartsWith("IP_", StringComparison.OrdinalIgnoreCase))
        {
            return PrinterConnectionType.TcpIp;
        }

        if (IsIpAddressLike(port))
        {
            return PrinterConnectionType.TcpIp;
        }

        return PrinterConnectionType.Unknown;
    }

    /// <summary>
    /// Nhận diện port ảo (không kết nối tới hardware thật).
    /// Windows dùng các tên chuẩn:
    ///   NUL:                     — Microsoft Print to PDF, XPS
    ///   PORTPROMPT:              — Microsoft Print to PDF
    ///   SHRFAX:                  — Fax
    ///   FILE:                    — Print to file
    /// </summary>
    private static bool IsVirtualPort(string port)
    {
        if (string.IsNullOrEmpty(port)) return false;

        var p = port.ToLowerInvariant();

        if (p.StartsWith("nul:"))        return true;
        if (p.StartsWith("portprompt"))  return true;
        if (p.StartsWith("shrfax"))      return true;
        if (p.StartsWith("fax:"))        return true;
        if (p.StartsWith("file:"))       return true;

        return false;
    }

    private static bool IsVirtualPrinterName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        var n = name.ToLowerInvariant();

        if (n.Contains("microsoft print to pdf")) return true;
        if (n.Contains("microsoft xps"))          return true;
        if (n.Contains("onenote"))                return true;
        if (n.StartsWith("fax"))                  return true;

        return false;
    }

    /// <summary>
    /// Nhận diện IP dạng "192.168.1.100" hoặc "10.0.0.5". Không parse đầy đủ,
    /// chỉ cần đủ phân biệt với các tên port khác.
    /// </summary>
    private static bool IsIpAddressLike(string port)
    {
        if (string.IsNullOrEmpty(port)) return false;

        // Bỏ hậu tố tùy chọn, ví dụ "192.168.1.100:9100"
        var colon = port.IndexOf(':');
        var candidate = colon > 0 ? port.Substring(0, colon) : port;

        var parts = candidate.Split('.');
        if (parts.Length != 4) return false;

        foreach (var part in parts)
        {
            if (part.Length == 0 || part.Length > 3) return false;
            if (!int.TryParse(part, out var value) || value < 0 || value > 255) return false;
        }

        return true;
    }
}