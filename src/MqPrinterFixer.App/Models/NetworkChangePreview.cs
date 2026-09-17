namespace MqPrinterFixer.App.Models;

/// <summary>
/// Preview trước khi Apply (Master Prompt Section 45-46).
/// </summary>
public sealed record NetworkChangePreview(
    NetworkChangeAction Action,
    string Title,
    string Reason,
    string CurrentState,
    string ProposedState,
    bool CanApply,
    string BlockReason,
    bool RequiresAdmin,
    bool RequiresConfirmation);