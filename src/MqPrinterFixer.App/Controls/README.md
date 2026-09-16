# Controls

Chứa **custom / user control** tái sử dụng.

Ví dụ dự kiến:
- `StatusBadge.xaml` / `.cs`
- `InfoCard.xaml` / `.cs`
- `ConsoleView.xaml` / `.cs` (bottom log console)
- `SidebarNavigation.xaml` / `.cs`
- `TopSystemHeader.xaml` / `.cs`
- `PreviewChangeDialog.xaml` / `.cs`
- `ConfirmDialog.xaml` / `.cs`
- `LoadingOverlay.xaml` / `.cs`

Quy tắc:
- Mỗi control tự chứa XAML + code-behind tối thiểu.
- Style chung đặt trong `Styles/`, không hardcode trong control.