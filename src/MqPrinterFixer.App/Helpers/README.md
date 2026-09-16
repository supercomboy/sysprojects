# Helpers

Chứa **static utility classes, extension methods, pure functions**.

Ví dụ dự kiến:
- `ComputerNameValidator.cs` (validate hostname theo Windows rules)
- `PrinterPathParser.cs` (parse `\\HOST\Share`)
- `NetworkHelper.cs` (IP format, subnet)
- `StringExtensions.cs`
- `EnumExtensions.cs` (lấy `Description` attribute)

Quy tắc:
- Pure function, không side effect khi có thể.
- Không truy cập hệ thống (nếu cần, đưa vào Service).
- Dễ unit test.