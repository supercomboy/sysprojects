using System.Text.Json;
using System.Text.Json.Serialization;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Parse JSON từ PowerShell GetFirewallCheckScript → FirewallStatus.
/// Pure function, testable không cần PowerShell thật.
/// </summary>
public static class FirewallStatusParser
{
    public static FirewallStatus Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return FirewallStatus.Unknown;
        }

        List<Dto>? dtos;

        try
        {
            var trimmed = json.Trim();

            // ConvertTo-Json trả về object đơn nếu mảng chỉ có 1 phần tử
            if (trimmed.StartsWith('['))
            {
                dtos = JsonSerializer.Deserialize<List<Dto>>(trimmed);
            }
            else
            {
                var single = JsonSerializer.Deserialize<Dto>(trimmed);
                dtos = single is null ? null : new List<Dto> { single };
            }
        }
        catch (JsonException)
        {
            return FirewallStatus.Unknown;
        }

        if (dtos is null || dtos.Count == 0)
        {
            return FirewallStatus.Unknown;
        }

        var checks = new List<FirewallRuleCheck>(dtos.Count);

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.GroupName))
            {
                continue;
            }

            var status = FirewallStatusAggregator.ComputeRuleStatus(
                dto.TotalRules, dto.EnabledRules);

            checks.Add(new FirewallRuleCheck(
                GroupName: dto.GroupName!,
                Status: status,
                TotalRules: dto.TotalRules,
                EnabledRules: dto.EnabledRules,
                DisplayName: FriendlyName(dto.GroupName!)));
        }

        if (checks.Count == 0)
        {
            return FirewallStatus.Unknown;
        }

        var overall = FirewallStatusAggregator.ComputeOverallStatus(checks);

        return new FirewallStatus(overall, checks);
    }

    private static string FriendlyName(string groupName) => groupName switch
    {
        "File and Printer Sharing" => "File & Printer Sharing",
        "Network Discovery"        => "Network Discovery",
        _                          => groupName
    };

    private sealed class Dto
    {
        [JsonPropertyName("GroupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("TotalRules")]
        public int TotalRules { get; set; }

        [JsonPropertyName("EnabledRules")]
        public int EnabledRules { get; set; }
    }
}