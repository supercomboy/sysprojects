# Services

Chứa **implementation** của các service – nơi thực sự gọi Windows API, WMI, Registry, PowerShell.

Ví dụ dự kiến:
- `SystemInfoService.cs`
- `WindowsEditionService.cs`
- `PrivilegeService.cs`
- `ComputerService.cs`
- `NetworkService.cs`
- `SharingService.cs`
- `FirewallService.cs`
- `PrinterService.cs`
- `PrinterRoleDetectionService.cs`
- `SpoolerService.cs`
- `RegistryService.cs`
- `PolicyService.cs`
- `DiagnosticService.cs`
- `PrinterErrorDiagnosticService.cs`
- `RepairService.cs`
- `BackupService.cs`
- `ChangeHistoryService.cs`
- `LoggingService.cs`
- `ThemeService.cs`
- `SettingsService.cs`
- `PowerShellService.cs`

Quy tắc:
- Mọi class trong đây implements một interface trong `Interfaces/`.
- Được đăng ký trong DI container (Phase 4).
- Trả về object có cấu trúc, không trả về text thô.