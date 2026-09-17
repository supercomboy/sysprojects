using MqPrinterFixer.App.Models;

namespace MqPrinterFixer.App.Helpers;

/// <summary>
/// Xây dựng preview từ state hiện tại + action + quyền admin.
/// Pure function, testable 100%.
/// </summary>
public static class NetworkChangePreviewBuilder
{
    public static NetworkChangePreview Build(
        NetworkChangeAction action,
        SharingState state,
        bool isAdministrator)
    {
        return action switch
        {
            NetworkChangeAction.ChangeNetworkToPrivate
                => BuildChangeToPrivate(state, isAdministrator),
            NetworkChangeAction.EnableNetworkDiscovery
                => BuildEnableDiscovery(state, isAdministrator),
            NetworkChangeAction.EnableFileAndPrinterSharing
                => BuildEnableFilePrinterSharing(state, isAdministrator),
            NetworkChangeAction.TurnOffPasswordProtectedSharing
                => BuildTurnOffPasswordProtectedSharing(state, isAdministrator),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
    }

    // -----------------------------------------------------------------
    //   ACTION 1: Change Network to Private
    // -----------------------------------------------------------------
    private static NetworkChangePreview BuildChangeToPrivate(
        SharingState state, bool isAdmin)
    {
        var categoryName = NetworkCategoryMapper.ToDisplayName(state.NetworkCategory);
        var canApply = true;
        var blockReason = string.Empty;

        switch (state.NetworkCategory)
        {
            case NetworkCategory.Private:
                canApply = false;
                blockReason = "Không cần thay đổi — network đã là Private.";
                break;

            case NetworkCategory.DomainAuthenticated:
                canApply = false;
                blockReason = "Network đang là DomainAuthenticated. " +
                              "Không được phép thay đổi (Master Prompt Section 31).";
                break;

            case NetworkCategory.Unknown:
                canApply = false;
                blockReason = "Không xác định được network profile.";
                break;

            case NetworkCategory.Public:
                if (!isAdmin)
                {
                    canApply = false;
                    blockReason = "Cần quyền Administrator. " +
                                  "Đóng app và mở lại bằng 'Run as administrator'.";
                }
                break;
        }

        return new NetworkChangePreview(
            Action: NetworkChangeAction.ChangeNetworkToPrivate,
            Title: "Change Network Profile to Private",
            Reason:
                "Private network cho phép LAN discovery và printer sharing. " +
                "Public network chặn các thiết bị khác trong LAN kết nối tới printer share.",
            CurrentState: categoryName,
            ProposedState: "Private",
            CanApply: canApply,
            BlockReason: blockReason,
            RequiresAdmin: true,
            RequiresConfirmation: false);
    }

    // -----------------------------------------------------------------
    //   ACTION 2: Enable Network Discovery
    // -----------------------------------------------------------------
    private static NetworkChangePreview BuildEnableDiscovery(
        SharingState state, bool isAdmin)
    {
        var current = state.NetworkDiscoveryEnabled ? "Enabled" : "Disabled";
        var canApply = true;
        var blockReason = string.Empty;

        if (state.NetworkDiscoveryEnabled)
        {
            canApply = false;
            blockReason = "Network Discovery đã Enabled. Không cần thay đổi.";
        }
        else if (state.NetworkCategory == NetworkCategory.Public)
        {
            canApply = false;
            blockReason = "Chỉ bật Network Discovery trên Private profile. " +
                          "Hãy đổi Public → Private trước.";
        }
        else if (state.NetworkCategory != NetworkCategory.Private)
        {
            canApply = false;
            blockReason = "Network profile phải là Private để bật Network Discovery.";
        }
        else if (!isAdmin)
        {
            canApply = false;
            blockReason = "Cần quyền Administrator.";
        }

        return new NetworkChangePreview(
            Action: NetworkChangeAction.EnableNetworkDiscovery,
            Title: "Enable Network Discovery",
            Reason:
                "Bật các firewall rule cần thiết cho Network Discovery trên Private profile. " +
                "Không áp dụng cho Public hoặc Domain profile.",
            CurrentState: current,
            ProposedState: "Enabled",
            CanApply: canApply,
            BlockReason: blockReason,
            RequiresAdmin: true,
            RequiresConfirmation: false);
    }

    // -----------------------------------------------------------------
    //   ACTION 3: Enable File & Printer Sharing
    // -----------------------------------------------------------------
    private static NetworkChangePreview BuildEnableFilePrinterSharing(
        SharingState state, bool isAdmin)
    {
        var current = state.FileAndPrinterSharingEnabled ? "Enabled" : "Disabled";
        var canApply = true;
        var blockReason = string.Empty;

        if (state.FileAndPrinterSharingEnabled)
        {
            canApply = false;
            blockReason = "File & Printer Sharing đã Enabled. Không cần thay đổi.";
        }
        else if (state.NetworkCategory == NetworkCategory.Public)
        {
            canApply = false;
            blockReason = "Chỉ bật File & Printer Sharing trên Private profile. " +
                          "Hãy đổi Public → Private trước.";
        }
        else if (state.NetworkCategory != NetworkCategory.Private)
        {
            canApply = false;
            blockReason = "Network profile phải là Private.";
        }
        else if (!isAdmin)
        {
            canApply = false;
            blockReason = "Cần quyền Administrator.";
        }

        return new NetworkChangePreview(
            Action: NetworkChangeAction.EnableFileAndPrinterSharing,
            Title: "Enable File & Printer Sharing",
            Reason:
                "Bật firewall rules cho File and Printer Sharing trên Private profile. " +
                "Đây là điều kiện cần để máy khác truy cập printer share.",
            CurrentState: current,
            ProposedState: "Enabled",
            CanApply: canApply,
            BlockReason: blockReason,
            RequiresAdmin: true,
            RequiresConfirmation: false);
    }

    // -----------------------------------------------------------------
    //   ACTION 4: Turn Off Password Protected Sharing
    // -----------------------------------------------------------------
    private static NetworkChangePreview BuildTurnOffPasswordProtectedSharing(
        SharingState state, bool isAdmin)
    {
        var current = state.PasswordProtectedSharingEnabled ? "ON" : "OFF";
        var canApply = true;
        var blockReason = string.Empty;

        if (!state.PasswordProtectedSharingEnabled)
        {
            canApply = false;
            blockReason = "Password Protected Sharing đã OFF. Không cần thay đổi.";
        }
        else if (!isAdmin)
        {
            canApply = false;
            blockReason = "Cần quyền Administrator.";
        }

        return new NetworkChangePreview(
            Action: NetworkChangeAction.TurnOffPasswordProtectedSharing,
            Title: "Turn Off Password Protected Sharing",
            Reason:
                "CẢNH BÁO BẢO MẬT: Shared resources (bao gồm printer share) sẽ có thể " +
                "được truy cập mà không cần tài khoản Windows trên máy này. " +
                "Chỉ nên tắt trong LAN tin cậy, nơi bạn kiểm soát toàn bộ thiết bị.",
            CurrentState: current,
            ProposedState: "OFF",
            CanApply: canApply,
            BlockReason: blockReason,
            RequiresAdmin: true,
            RequiresConfirmation: true);
    }
}