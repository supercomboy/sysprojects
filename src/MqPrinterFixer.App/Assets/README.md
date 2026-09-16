# Assets

Chứa **file nhị phân**: icon `.ico`, `.png`, `.svg`, font `.ttf`.

Ví dụ dự kiến:
- `app-icon.ico` (icon file .exe)
- `logo-256.png`
- `logo-512.png`
- `Inter-Regular.ttf` (nếu dùng font custom)

Quy tắc:
- Trong csproj, đặt `Build Action = Resource` cho file cần nhúng vào assembly.
- Icon .exe khai báo qua `<ApplicationIcon>Assets\app-icon.ico</ApplicationIcon>`.