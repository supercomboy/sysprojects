# ViewModels

Chứa **UI state, commands, observable properties, navigation state**.

Ví dụ dự kiến:
- `ViewModelBase.cs` (kế thừa ObservableObject từ CommunityToolkit.Mvvm)
- `MainViewModel.cs`
- `DashboardViewModel.cs`
- `PrintersViewModel.cs`
- `NetworkSharingViewModel.cs`
- `SettingsViewModel.cs`
- `LogsViewModel.cs`

Quy tắc:
- Không gọi trực tiếp Registry, Process.Start, PowerShell, WMI, ServiceController.
- Mọi thao tác hệ thống đi qua **Service Interface** (nhận qua constructor injection).
- Dùng `[ObservableProperty]` và `[RelayCommand]` của CommunityToolkit.Mvvm.