# Converters

Chứa **IValueConverter** cho WPF data binding.

Ví dụ dự kiến:
- `BoolToVisibilityConverter.cs`
- `InverseBoolToVisibilityConverter.cs`
- `PrinterStatusToBrushConverter.cs` (Ready → Green, Offline → Red, ...)
- `PrinterRoleToDisplayConverter.cs` (Host → "Printer Host", Hybrid → "Host + Client")
- `DiagnosticStatusToBrushConverter.cs`
- `LogLevelToBrushConverter.cs`
- `EnumDescriptionConverter.cs`

Quy tắc:
- Chỉ chuyển đổi dữ liệu, không gọi service.
- Không có state (thread-safe).