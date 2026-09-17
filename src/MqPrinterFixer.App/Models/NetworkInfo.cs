namespace MqPrinterFixer.App.Models;

/// <summary>
/// Thông tin mạng đang hoạt động (read-only snapshot).
/// </summary>
public sealed record NetworkInfo(
    bool IsConnected,
    string AdapterName,
    string NetworkName,
    NetworkCategory Category,
    string IPv4Address,
    string MacAddress)
{
    public static readonly NetworkInfo Empty = new(
        IsConnected: false,
        AdapterName: "—",
        NetworkName: "—",
        Category: NetworkCategory.Unknown,
        IPv4Address: "—",
        MacAddress: "—");

    public bool HasIPv4 => !string.IsNullOrWhiteSpace(IPv4Address) && IPv4Address != "—";
    public bool HasAdapter => !string.IsNullOrWhiteSpace(AdapterName) && AdapterName != "—";
}