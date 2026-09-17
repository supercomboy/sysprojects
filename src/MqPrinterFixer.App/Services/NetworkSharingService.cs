using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Interfaces;
using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Services;

public sealed class NetworkSharingService : INetworkSharingService
{
    private readonly IPowerShellService _powerShell;
    private readonly ISystemInfoService _systemInfo;

    // -----------------------------------------------------------------
    //   Script: GetSharingState
    // -----------------------------------------------------------------
    private const string GetStateScript = """
        $ErrorActionPreference = 'SilentlyContinue'

        $profile = Get-NetConnectionProfile | Select-Object -First 1
        $category = if ($profile) { $profile.NetworkCategory.ToString() } else { 'Unknown' }
        $ifAlias = if ($profile) { $profile.InterfaceAlias } else { '' }

        $ndRules = @(Get-NetFirewallRule -DisplayGroup 'Network Discovery' -ErrorAction SilentlyContinue)
        $ndEnabled = $false
        if ($ndRules.Count -gt 0) {
            $ndEnabledCount = @($ndRules | Where-Object { $_.Enabled -eq 'True' }).Count
            $ndEnabled = ($ndEnabledCount -gt 0 -and $ndEnabledCount -eq $ndRules.Count)
        }

        $fpsRules = @(Get-NetFirewallRule -DisplayGroup 'File and Printer Sharing' -ErrorAction SilentlyContinue)
        $fpsEnabled = $false
        if ($fpsRules.Count -gt 0) {
            $fpsEnabledCount = @($fpsRules | Where-Object { $_.Enabled -eq 'True' }).Count
            $fpsEnabled = ($fpsEnabledCount -gt 0 -and $fpsEnabledCount -eq $fpsRules.Count)
        }

        $lsa = Get-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa' -Name 'everyoneincludesanonymous' -ErrorAction SilentlyContinue
        $everyoneAnon = if ($lsa -and $null -ne $lsa.everyoneincludesanonymous) { [int]$lsa.everyoneincludesanonymous } else { 0 }
        $ppsEnabled = ($everyoneAnon -eq 0)

        [PSCustomObject]@{
            NetworkCategory = $category
            InterfaceAlias = $ifAlias
            NetworkDiscoveryEnabled = $ndEnabled
            FileAndPrinterSharingEnabled = $fpsEnabled
            PasswordProtectedSharingEnabled = $ppsEnabled
        } | ConvertTo-Json -Compress -Depth 3
        """;

    public NetworkSharingService(
        IPowerShellService powerShell,
        ISystemInfoService systemInfo)
    {
        _powerShell = powerShell ?? throw new ArgumentNullException(nameof(powerShell));
        _systemInfo = systemInfo ?? throw new ArgumentNullException(nameof(systemInfo));
    }

    public async Task<SharingState> GetSharingStateAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _powerShell.RunAsync(GetStateScript, cancellationToken: cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            return SharingState.Unknown;
        }

        return SharingStateParser.Parse(result.StandardOutput);
    }

    public async Task<NetworkChangePreview> PreviewChangeAsync(
        NetworkChangeAction action,
        CancellationToken cancellationToken = default)
    {
        var state = await GetSharingStateAsync(cancellationToken);
        var info = await _systemInfo.GetComputerInfoAsync(cancellationToken);
        return NetworkChangePreviewBuilder.Build(action, state, info.IsAdministrator);
    }

    public async Task<NetworkChangeResult> ApplyChangeAsync(
        NetworkChangePreview preview,
        CancellationToken cancellationToken = default)
    {
        if (!preview.CanApply)
        {
            return NetworkChangeResult.Blocked(preview.Action, preview.BlockReason);
        }

        var beforeState = await GetSharingStateAsync(cancellationToken);

        var applyResult = preview.Action switch
        {
            NetworkChangeAction.ChangeNetworkToPrivate
                => await ApplyChangeToPrivateAsync(beforeState, cancellationToken),

            NetworkChangeAction.EnableNetworkDiscovery
                => await RunScriptAsync(EnableNetworkDiscoveryScript, cancellationToken),

            NetworkChangeAction.EnableFileAndPrinterSharing
                => await RunScriptAsync(EnableFileAndPrinterSharingScript, cancellationToken),

            NetworkChangeAction.TurnOffPasswordProtectedSharing
                => await RunScriptAsync(TurnOffPasswordProtectedSharingScript, cancellationToken),

            _ => Models.PowerShellResult.Fail(-1, "Unknown action")
        };

        if (!applyResult.Success)
        {
            return new NetworkChangeResult(
                Success: false,
                Action: preview.Action,
                BeforeState: preview.CurrentState,
                AfterState: preview.CurrentState,
                Verified: false,
                ErrorMessage: string.IsNullOrWhiteSpace(applyResult.StandardError)
                    ? "Apply thất bại."
                    : applyResult.StandardError);
        }

        // Verify sau khi apply
        var afterState = await GetSharingStateAsync(cancellationToken);
        var verified = VerifyChange(preview.Action, afterState);

        return new NetworkChangeResult(
            Success: true,
            Action: preview.Action,
            BeforeState: preview.CurrentState,
            AfterState: DescribeNewState(preview.Action, afterState),
            Verified: verified,
            ErrorMessage: verified
                ? string.Empty
                : "Đã apply nhưng verify không xác nhận. Mở lại Windows Settings để kiểm tra.");
    }

    // -----------------------------------------------------------------
    //   Apply scripts
    // -----------------------------------------------------------------

       private async Task<PowerShellResult> ApplyChangeToPrivateAsync(
        SharingState state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state.InterfaceAlias) || state.InterfaceAlias == "—")
        {
            return PowerShellResult.Fail(-1, "Không xác định được InterfaceAlias.");
        }

        // Dùng verbatim string + ghép chuỗi để tránh mọi vấn đề
        // interpolation với PowerShell braces.
        var escaped = state.InterfaceAlias.Replace("'", "''");

        var script =
            "$ErrorActionPreference = 'Stop'\n" +
            "try {\n" +
            "    Set-NetConnectionProfile -InterfaceAlias '" + escaped +
                "' -NetworkCategory Private -ErrorAction Stop\n" +
            "    Write-Output 'DONE'\n" +
            "} catch {\n" +
            "    [Console]::Error.WriteLine($_.Exception.Message)\n" +
            "    exit 1\n" +
            "}";

        return await RunScriptAsync(script, cancellationToken);
    }

    private const string EnableNetworkDiscoveryScript = """
        $ErrorActionPreference = 'Stop'
        try {
            Set-NetFirewallRule -DisplayGroup 'Network Discovery' -Profile Private -Enabled True -ErrorAction Stop
            Write-Output 'DONE'
        } catch {
            [Console]::Error.WriteLine($_.Exception.Message)
            exit 1
        }
        """;

    private const string EnableFileAndPrinterSharingScript = """
        $ErrorActionPreference = 'Stop'
        try {
            Set-NetFirewallRule -DisplayGroup 'File and Printer Sharing' -Profile Private -Enabled True -ErrorAction Stop
            Write-Output 'DONE'
        } catch {
            [Console]::Error.WriteLine($_.Exception.Message)
            exit 1
        }
        """;

    private const string TurnOffPasswordProtectedSharingScript = """
        $ErrorActionPreference = 'Stop'
        try {
            Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa' -Name 'everyoneincludesanonymous' -Value 1 -Type DWord -ErrorAction Stop
            Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa' -Name 'forceguest' -Value 0 -Type DWord -ErrorAction Stop
            Write-Output 'DONE'
        } catch {
            [Console]::Error.WriteLine($_.Exception.Message)
            exit 1
        }
        """;

    private Task<PowerShellResult> RunScriptAsync(string script, CancellationToken cancellationToken)
        => _powerShell.RunAsync(script, cancellationToken: cancellationToken);

    // -----------------------------------------------------------------
    //   Verify
    // -----------------------------------------------------------------

    private static bool VerifyChange(NetworkChangeAction action, SharingState after) =>
        action switch
        {
            NetworkChangeAction.ChangeNetworkToPrivate
                => after.NetworkCategory == NetworkCategory.Private,

            NetworkChangeAction.EnableNetworkDiscovery
                => after.NetworkDiscoveryEnabled,

            NetworkChangeAction.EnableFileAndPrinterSharing
                => after.FileAndPrinterSharingEnabled,

            NetworkChangeAction.TurnOffPasswordProtectedSharing
                => !after.PasswordProtectedSharingEnabled,

            _ => false
        };

    private static string DescribeNewState(NetworkChangeAction action, SharingState state) =>
        action switch
        {
            NetworkChangeAction.ChangeNetworkToPrivate
                => NetworkCategoryMapper.ToDisplayName(state.NetworkCategory),

            NetworkChangeAction.EnableNetworkDiscovery
                => state.NetworkDiscoveryEnabled ? "Enabled" : "Disabled",

            NetworkChangeAction.EnableFileAndPrinterSharing
                => state.FileAndPrinterSharingEnabled ? "Enabled" : "Disabled",

            NetworkChangeAction.TurnOffPasswordProtectedSharing
                => state.PasswordProtectedSharingEnabled ? "ON" : "OFF",

            _ => "Unknown"
        };
}