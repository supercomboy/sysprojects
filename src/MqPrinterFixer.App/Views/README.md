# Views

Chứa **WPF Window / UserControl / Page** – tầng hiển thị.

Ví dụ dự kiến:
- `MainWindow.xaml` (sẽ chuyển từ root sang đây ở Phase 6)
- `DashboardView.xaml`
- `PrintersView.xaml`
- `NetworkSharingView.xaml`
- `Error0x11BView.xaml`
- `Error0x709View.xaml`
- `AdvancedView.xaml`
- `LogsView.xaml`
- `SettingsView.xaml`

Quy tắc:
- XAML chỉ chứa layout, binding, style reference.
- Code-behind chỉ cho hành vi UI rất nhỏ (drag, close, focus).
- Không đặt business logic ở đây.