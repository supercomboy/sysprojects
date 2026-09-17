using System.Text.Json;
using System.Text.Json.Serialization;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Parse JSON output từ script PowerShell GetSharingState → SharingState.
/// Pure function, testable không cần chạy PowerShell.
/// </summary>
public static class SharingStateParser
{
    public static SharingState Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return SharingState.Unknown;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<Dto>(json);
            if (dto is null)
            {
                return SharingState.Unknown;
            }

            return new SharingState(
                NetworkCategory: NetworkCategoryMapper.Parse(dto.NetworkCategory),
                InterfaceAlias: string.IsNullOrWhiteSpace(dto.InterfaceAlias)
                    ? "—" : dto.InterfaceAlias,
                NetworkDiscoveryEnabled: dto.NetworkDiscoveryEnabled,
                FileAndPrinterSharingEnabled: dto.FileAndPrinterSharingEnabled,
                PasswordProtectedSharingEnabled: dto.PasswordProtectedSharingEnabled);
        }
        catch (JsonException)
        {
            return SharingState.Unknown;
        }
    }

    private sealed class Dto
    {
        [JsonPropertyName("NetworkCategory")]
        public string? NetworkCategory { get; set; }

        [JsonPropertyName("InterfaceAlias")]
        public string? InterfaceAlias { get; set; }

        [JsonPropertyName("NetworkDiscoveryEnabled")]
        public bool NetworkDiscoveryEnabled { get; set; }

        [JsonPropertyName("FileAndPrinterSharingEnabled")]
        public bool FileAndPrinterSharingEnabled { get; set; }

        [JsonPropertyName("PasswordProtectedSharingEnabled")]
        public bool PasswordProtectedSharingEnabled { get; set; }
    }
}