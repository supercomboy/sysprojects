# Interfaces

Chứa **abstraction** cho tất cả service.

Ví dụ dự kiến:
- `ISystemInfoService.cs`
- `INetworkService.cs`
- `IPrinterService.cs`
- `IPrinterRoleDetectionService.cs`
- `ISpoolerService.cs`
- `IRegistryService.cs`
- `IPolicyService.cs`
- `IDiagnosticService.cs`
- `IRepairService.cs`
- `IBackupService.cs`
- `IChangeHistoryService.cs`
- `ILoggingService.cs`
- `IThemeService.cs`
- `ISettingsService.cs`
- `IPowerShellService.cs`

Quy tắc:
- Interface sống **độc lập** với implementation.
- ViewModel chỉ biết interface, không biết class cụ thể.
- Dễ mock trong unit test.