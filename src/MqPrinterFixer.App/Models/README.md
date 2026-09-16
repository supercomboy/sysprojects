# Models

Chứa **data structures, enums, DTOs, records** – không có logic nghiệp vụ.

Ví dụ dự kiến:
- `PrinterInfo.cs`
- `PrinterRole.cs` (enum: Standalone, Host, Client, Hybrid)
- `DiagnosticResult.cs`
- `RepairResult.cs`
- `ChangeTransaction.cs`
- `NetworkProfile.cs`
- `AppTheme.cs` (enum: System, Light, Dark)

Quy tắc:
- Chỉ chứa dữ liệu và các thuộc tính đơn giản.
- Không gọi Windows API, Registry, WMI, PowerShell.
- Không reference tới Service hay ViewModel.