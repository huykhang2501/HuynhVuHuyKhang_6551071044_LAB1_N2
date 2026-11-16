# Hotel Management - Lab 1

Ứng dụng C# WinForms quản lý khách sạn cho Lab 1: đăng nhập, sơ đồ phòng, quản lý phòng, quản lý loại phòng và xuất Excel.

## Chạy database

Mở PowerShell trong thư mục này và chạy:

```powershell
.\Database\setup-database.ps1 -Reset
```

Mặc định ứng dụng kết nối SQL Server `.\SQLEXPRESS`, database `HotelManagement`. Nếu máy dùng instance khác, đặt biến môi trường `HOTEL_DB_CONNECTION` trước khi chạy ứng dụng.

## Chạy ứng dụng

```powershell
dotnet run
```

Tài khoản mẫu: `admin` / `1234`.

## Chức năng đã có

- Đăng nhập và hiển thị tên nhân viên/quyền.
- Sơ đồ phòng: lọc tình trạng phòng, loại phòng, dọn dẹp và tìm theo mã.
- Quản lý phòng: thêm, sửa, xóa mềm, tìm kiếm, xuất Excel.
- Quản lý loại phòng: thêm, sửa, xóa khi không còn phòng sử dụng, tìm kiếm, xuất Excel.
