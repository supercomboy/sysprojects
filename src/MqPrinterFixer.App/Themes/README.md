# Themes

Chứa **ResourceDictionary** định nghĩa bảng màu cho từng theme.

Ví dụ dự kiến:
- `LightTheme.xaml` (Phase 5)
- `DarkTheme.xaml` (Phase 5)
- `ThemeResources.xaml` (common keys, phase 5)

Key chuẩn dự kiến:
- `AppBackgroundBrush`
- `CardBackgroundBrush`
- `PrimaryBrush`
- `SuccessBrush`
- `WarningBrush`
- `ErrorBrush`
- `TextPrimaryBrush`
- `TextSecondaryBrush`
- `BorderBrush`
- ...

Quy tắc:
- View chỉ dùng `{DynamicResource ...}`, không hardcode màu.
- Đổi theme runtime → swap ResourceDictionary trong `App.Resources.MergedDictionaries`.