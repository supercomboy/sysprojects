using System.Text.Json;
using System.Text.Json.Serialization;
using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class NetworkService : INetworkService
{
    private readonly IPowerShellService _powerShell;

    // Script chạy 1 lần lấy cả profile + IPv4 + MAC.
    // Output: JSON (object nếu 1 profile, array nếu nhiều).
    private const string QueryScript = @"
$ErrorActionPreference = 'SilentlyContinue'
$profiles = @(Get-NetConnectionProfile)
if ($profiles.Count -eq 0) { Write-Output '[]'; exit 0 }

$result = @()
foreach ($p in $profiles) {
    $ifAlias = $p.InterfaceAlias
    $ip = (Get-NetIPAddress -InterfaceAlias $ifAlias -AddressFamily IPv4 -ErrorAction SilentlyContinue |
           Where-Object { $_.PrefixOrigin -ne 'WellKnown' } |
           Select-Object -First 1).IPAddress
    $mac = (Get-NetAdapter -InterfaceAlias $ifAlias -ErrorAction SilentlyContinue).MacAddress
    $result += [PSCustomObject]@{
        InterfaceAlias  = $ifAlias
        Name            = $p.Name
        NetworkCategory = $p.NetworkCategory.ToString()
        IPv4Address     = $ip
        MacAddress      = $mac
    }
}
$result | ConvertTo-Json -Compress -Depth 3
";

    public NetworkService(IPowerShellService powerShell)
    {
        _powerShell = powerShell ?? throw new ArgumentNullException(nameof(powerShell));
    }

    public async Task<NetworkInfo> GetActiveNetworkAsync(CancellationToken cancellationToken = default)
    {
        var result = await _powerShell.RunAsync(QueryScript, cancellationToken: cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            return NetworkInfo.Empty;
        }

        try
        {
            var json = result.StandardOutput.Trim();
            if (json.Length == 0 || json == "[]")
            {
                return NetworkInfo.Empty;
            }

            ProfileDto? chosen;
            if (json.StartsWith('['))
            {
                var list = JsonSerializer.Deserialize<List<ProfileDto>>(json);
                chosen = ChooseBestProfile(list);
            }
            else
            {
                chosen = JsonSerializer.Deserialize<ProfileDto>(json);
            }

            return chosen is null ? NetworkInfo.Empty : ToNetworkInfo(chosen);
        }
        catch (JsonException)
        {
            return NetworkInfo.Empty;
        }
    }

    private static ProfileDto? ChooseBestProfile(List<ProfileDto>? profiles)
    {
        if (profiles is null || profiles.Count == 0)
        {
            return null;
        }
        if (profiles.Count == 1)
        {
            return profiles[0];
        }

        // Ưu tiên: non-domain + có IPv4 > có IPv4 > phần tử đầu.
        return profiles.FirstOrDefault(p =>
                   !string.IsNullOrWhiteSpace(p.IPv4Address) &&
                   !string.Equals(p.NetworkCategory, "DomainAuthenticated", StringComparison.OrdinalIgnoreCase))
               ?? profiles.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.IPv4Address))
               ?? profiles[0];
    }

    private static NetworkInfo ToNetworkInfo(ProfileDto dto)
    {
        return new NetworkInfo(
            IsConnected: true,
            AdapterName: string.IsNullOrWhiteSpace(dto.InterfaceAlias) ? "—" : dto.InterfaceAlias,
            NetworkName: string.IsNullOrWhiteSpace(dto.Name) ? "—" : dto.Name,
            Category: NetworkCategoryMapper.Parse(dto.NetworkCategory),
            IPv4Address: string.IsNullOrWhiteSpace(dto.IPv4Address) ? "—" : dto.IPv4Address,
            MacAddress: string.IsNullOrWhiteSpace(dto.MacAddress) ? "—" : dto.MacAddress);
    }

    /// <summary>
    /// DTO map với JSON PowerShell.
    /// </summary>
    private sealed class ProfileDto
    {
        [JsonPropertyName("InterfaceAlias")]
        public string? InterfaceAlias { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("NetworkCategory")]
        public string? NetworkCategory { get; set; }

        [JsonPropertyName("IPv4Address")]
        public string? IPv4Address { get; set; }

        [JsonPropertyName("MacAddress")]
        public string? MacAddress { get; set; }
    }
}