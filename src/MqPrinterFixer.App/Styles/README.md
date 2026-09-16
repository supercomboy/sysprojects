# Styles

Chứa **Style và ControlTemplate** cho control tái sử dụng.

Ví dụ dự kiến:
- `Buttons.xaml` (Primary, Secondary, Danger, Navigation)
- `Cards.xaml`
- `Typography.xaml`
- `TextBoxes.xaml`
- `ComboBoxes.xaml`
- `Toggles.xaml`
- `DataGrid.xaml`
- `StatusBadge.xaml`
- `Dialogs.xaml`
- `Scrollbars.xaml`

Quy tắc:
- Style tham chiếu `DynamicResource` tới brush định nghĩa ở `Themes/`.
- Không hardcode màu trực tiếp trong Style file.
- Mỗi file là một `ResourceDictionary` được merge vào `App.xaml`.