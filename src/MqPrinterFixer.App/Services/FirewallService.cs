using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class FirewallService : IFirewallService
{
    private readonly IPowerShellService _powerShell;
    private readonly ISystemInfoService _systemInfo;

    // Các group cần check + enable cho printer sharing.
    // DisplayGroup name dùng tiếng Anh — nếu Windows non-English sẽ không match.
    // Phase 27 sẽ chuyển sang filter theo Name prefix.
    private static readonly string[] RequiredGroups =
    {
        "File and Printer Sharing",
        "Network Discovery"
    };

    // Script không có biến động → dùng raw string literal không `$`.
    private const string CheckScript = """
        $ErrorActionPreference = 'SilentlyContinue'
        $groups = @('File and Printer Sharing', 'Network Discovery')
        $results = @()
        foreach ($g in $groups) {
            $rules = @(Get-NetFirewallRule -DisplayGroup $g -ErrorAction SilentlyContinue)
            $total = $rules.Count
            $enabled = 0
            if ($total -gt 0) {
                $enabled = @($rules | Where-Object { $_.Enabled -eq 'True' }).Count
            }
            $results += [PSCustomObject]@{
                GroupName    = $g
                TotalRules   = $total
                EnabledRules = $enabled
            }
        }
        $results | ConvertTo-Json -Compress -Depth 3
        """;

    // Script enable rules cho Private profile. KHÔNG disable.
    private const string EnableScript = """
        $ErrorActionPreference = 'Stop'
        try {
            Set-NetFirewallRule -DisplayGroup 'File and Printer Sharing' -Profile Private -Enabled True -ErrorAction Stop
            Set-NetFirewallRule -DisplayGroup 'Network Discovery' -Profile Private -Enabled True -ErrorAction Stop
            Write-Output 'DONE'
        } catch {
            [Console]::Error.WriteLine($_.Exception.Message)
            exit 1
        }
        """;

    public FirewallService(
        IPowerShellService powerShell,
        ISystemInfoService systemInfo)
    {
        _powerShell = powerShell ?? throw new ArgumentNullException(nameof(powerShell));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    }

    public async Task<FirewallStatus> CheckAsync(CancellationToken cancellationToken = default)
    {
        var result = await _powerShell.RunAsync(CheckScript, cancellationToken: cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            return FirewallStatus.Unknown;
        }

        return FirewallStatusParser.Parse(result.StandardOutput);
    }

    public async Task<NetworkChangeResult> EnableRequiredRulesAsync(
        CancellationToken cancellationToken = default)
    {
        const NetworkChangeAction action = NetworkChangeAction.EnableFileAndPrinterSharing;

        var info = await _systemInfo.GetComputerInfoAsync(cancellationToken);
        if (!info.IsAdministrator)
        {
            return NetworkChangeResult.Blocked(action,
                "Cần quyền Administrator. Đóng app và mở lại bằng 'Run as administrator'.");
        }

        var before = await CheckAsync(cancellationToken);
        var beforeSummary = before.Summary;

        var applyResult = await _powerShell.RunAsync(
            EnableScript, cancellationToken: cancellationToken);

        if (!applyResult.Success)
        {
            return new NetworkChangeResult(
                Success: false,
                Action: action,
                BeforeState: beforeSummary,
                AfterState: beforeSummary,
                Verified: false,
                ErrorMessage: string.IsNullOrWhiteSpace(applyResult.StandardError)
                    ? "Firewall enable thất bại."
                    : applyResult.StandardError);
        }

        var after = await CheckAsync(cancellationToken);
        var verified = FirewallStatusAggregator.ComputeOverallStatus(after.Checks)
                       == FirewallRuleStatus.Enabled;

        return new NetworkChangeResult(
            Success: true,
            Action: action,
            BeforeState: beforeSummary,
            AfterState: after.Summary,
            Verified: verified,
            ErrorMessage: verified
                ? string.Empty
                : "Đã bật rules nhưng verify không xác nhận đủ. Kiểm tra Windows Firewall manually.");
    }
}