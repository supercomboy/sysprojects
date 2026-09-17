using MqPrinterFixer.App.Helpers;
using MqPrinterFixer.App.Models;
using Xunit;

namespace MqPrinterFixer.Tests;

public class NetworkChangePreviewBuilderTests
{
    private static SharingState State(
        NetworkCategory cat = NetworkCategory.Private,
        bool discovery = true,
        bool fps = true,
        bool pps = true)
        => new(cat, "Ethernet", discovery, fps, pps);

    // ============================================================
    //  Change Network to Private
    // ============================================================

    [Fact]
    public void ChangeToPrivate_WhenPublicAndAdmin_CanApply()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.ChangeNetworkToPrivate,
            State(cat: NetworkCategory.Public),
            isAdministrator: true);

        Assert.True(preview.CanApply);
        Assert.Equal("Public", preview.CurrentState);
        Assert.Equal("Private", preview.ProposedState);
    }

    [Fact]
    public void ChangeToPrivate_WhenAlreadyPrivate_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.ChangeNetworkToPrivate,
            State(cat: NetworkCategory.Private),
            isAdministrator: true);

        Assert.False(preview.CanApply);
        Assert.Contains("Private", preview.BlockReason);
    }

    [Fact]
    public void ChangeToPrivate_WhenDomainAuthenticated_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.ChangeNetworkToPrivate,
            State(cat: NetworkCategory.DomainAuthenticated),
            isAdministrator: true);

        Assert.False(preview.CanApply);
        Assert.Contains("DomainAuthenticated", preview.BlockReason);
    }

    [Fact]
    public void ChangeToPrivate_WhenPublicAndNonAdmin_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.ChangeNetworkToPrivate,
            State(cat: NetworkCategory.Public),
            isAdministrator: false);

        Assert.False(preview.CanApply);
        Assert.Contains("Administrator", preview.BlockReason);
    }

    // ============================================================
    //  Enable Network Discovery
    // ============================================================

    [Fact]
    public void EnableDiscovery_WhenPrivateAndAdmin_CanApply()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.EnableNetworkDiscovery,
            State(cat: NetworkCategory.Private, discovery: false),
            isAdministrator: true);

        Assert.True(preview.CanApply);
        Assert.Equal("Disabled", preview.CurrentState);
        Assert.Equal("Enabled", preview.ProposedState);
    }

    [Fact]
    public void EnableDiscovery_WhenAlreadyEnabled_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.EnableNetworkDiscovery,
            State(cat: NetworkCategory.Private, discovery: true),
            isAdministrator: true);

        Assert.False(preview.CanApply);
    }

    [Fact]
    public void EnableDiscovery_WhenPublic_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.EnableNetworkDiscovery,
            State(cat: NetworkCategory.Public, discovery: false),
            isAdministrator: true);

        Assert.False(preview.CanApply);
        Assert.Contains("Public", preview.BlockReason);
    }

    // ============================================================
    //  Enable File & Printer Sharing
    // ============================================================

    [Fact]
    public void EnableFps_WhenPrivateAndAdmin_CanApply()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.EnableFileAndPrinterSharing,
            State(cat: NetworkCategory.Private, fps: false),
            isAdministrator: true);

        Assert.True(preview.CanApply);
    }

    [Fact]
    public void EnableFps_WhenAlreadyEnabled_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.EnableFileAndPrinterSharing,
            State(fps: true),
            isAdministrator: true);

        Assert.False(preview.CanApply);
    }

    // ============================================================
    //  Turn Off Password Protected Sharing
    // ============================================================

    [Fact]
    public void TurnOffPps_WhenOnAndAdmin_CanApplyWithConfirmation()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.TurnOffPasswordProtectedSharing,
            State(pps: true),
            isAdministrator: true);

        Assert.True(preview.CanApply);
        Assert.True(preview.RequiresConfirmation);
        Assert.Contains("BẢO MẬT", preview.Reason);
    }

    [Fact]
    public void TurnOffPps_WhenAlreadyOff_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.TurnOffPasswordProtectedSharing,
            State(pps: false),
            isAdministrator: true);

        Assert.False(preview.CanApply);
    }

    [Fact]
    public void TurnOffPps_WhenNonAdmin_Blocked()
    {
        var preview = NetworkChangePreviewBuilder.Build(
            NetworkChangeAction.TurnOffPasswordProtectedSharing,
            State(pps: true),
            isAdministrator: false);

        Assert.False(preview.CanApply);
        Assert.Contains("Administrator", preview.BlockReason);
    }
}