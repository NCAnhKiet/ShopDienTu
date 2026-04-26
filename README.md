# Shop Điện Tử (Electronics E-Commerce)

Dự án website thương mại điện tử chuyên cung cấp các thiết bị điện tử, được xây dựng bằng nền tảng **ASP.NET Core 8 MVC** kết hợp với **Entity Framework Core**.

## 🛠 Công nghệ sử dụng

- **Framework:** ASP.NET Core 8 MVC
- **Database:** SQL Server
- **ORM:** Entity Framework Core 8.0
- **Authentication & Security:** 
  - JWT (JSON Web Tokens) cho bảo mật API
  - Session & Cookie cho phiên bản Web MVC
  - BCrypt.Net-Next (Mã hóa mật khẩu an toàn)
- **API Documentation:** Swagger (Swashbuckle.AspNetCore)

## 📂 Cấu trúc dự án

Dự án được thiết kế theo mô hình MVC (Model - View - Controller) và phân chia thành hai khu vực (Areas) chính:

### 1. Khu vực Người dùng (Client Site)
- **Sản phẩm (`ProductController`):** Xem danh sách và chi tiết các sản phẩm điện tử.
- **Giỏ hàng (`CartController`):** Thêm, sửa, xóa sản phẩm trong giỏ hàng (sử dụng Session).
- **Thanh toán & Đơn hàng (`CheckoutController`, `OrderController`):** Tiến hành đặt hàng và xem lại lịch sử các đơn hàng đã đặt.
- **Tài khoản (`AccessController`, `AuthController`, `ProfileController`):** Đăng ký, đăng nhập và quản lý thông tin hồ sơ cá nhân.

### 2. Khu vực Quản trị (Admin Area)
- Truy cập thông qua đường dẫn `/admin`.
- **Bảng điều khiển (`HomeAdminController`):** Thống kê và quản lý tổng quan.
- **Quản lý Tài khoản (`AccountAdminController`):** Xem và quản trị người dùng trên hệ thống.
- **Quản lý Sản phẩm API (`ProductApiController`):** Cung cấp các API riêng biệt phục vụ việc quản lý hàng hóa.
- **Nhật ký Hoạt động (`LogAdminController`):** Theo dõi lịch sử thao tác của hệ thống (lưu trong `ActivityLogs`).

## 🚀 Hướng dẫn cài đặt và chạy dự án

### Yêu cầu hệ thống
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server (hoặc SQL Server Express)
- Visual Studio 2022, Visual Studio Code hoặc Rider.

### Các bước cài đặt

1. **Clone dự án về máy:**
   ```bash
   git clone <repository-url>
   cd ShopDienTu/ShopDienTu
   ```

2. **Cấu hình Database (Connection String):**
   Mở file `appsettings.json` và cấu hình lại chuỗi kết nối cho phù hợp với SQL Server của bạn:
   ```json
   "ConnectionStrings": {
     "ShopDienTuContext": "Server=YOUR_SERVER_NAME;Database=ShopDienTu;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Cập nhật Database (Migration):**
   Mở Terminal hoặc Package Manager Console tại thư mục chứa file `ShopDienTu.csproj` và chạy lệnh:
   ```bash
   dotnet ef database update
   ```

4. **Chạy ứng dụng:**
   ```bash
   dotnet run
   ```
   Ứng dụng sẽ tự động mở trên trình duyệt tại địa chỉ `https://localhost:<port>` hoặc `http://localhost:<port>`.

5. **Truy cập API Documentation (Swagger):**
   Điều hướng đến `https://localhost:<port>/swagger` để xem và thử nghiệm các API của hệ thống.

## ✨ Tính năng nổi bật
- Phân chia Area Admin rõ ràng, bảo mật tách biệt.
- Sử dụng Distributed Memory Cache để tối ưu hiệu năng.
- Quản lý State thông qua Session và HttpContextAccessor.
- Auto-generate bảng `ActivityLogs` (trong Program.cs) để tự động ghi log theo dõi hành vi hệ thống.
- Document hóa API tự động bằng Swagger UI.