# Building Material Manager - Hệ Thống Quản Lý Vật Liệu Xây Dựng

## Tổng Quan Dự Án

Đây là một ứng dụng quản lý cửa hàng vật liệu xây dựng được phát triển bằng C# Windows Forms và MySQL, được thiết kế theo yêu cầu đồ án lập trình .NET (Ý tưởng số 8).

## Các Tính Năng Đã Triển Khai

### ✅ Hoàn Thành
1. **Database Script** - `database_script.sql`
   - 6 bảng: Users, Categories, Products, Customers, Orders, OrderDetails
   - Dữ liệu mẫu để test

2. **Cấu Trúc Dự Án**
   - `App.config` - Cấu hình connection string
   - `DatabaseHelper.cs` - Class hỗ trợ kết nối và truy vấn database
   - `GlobalVariables.cs` - Lưu thông tin người dùng đăng nhập
   - `Program.cs` - Entry point của ứng dụng

3. **LoginForm** ✅
   - Đăng nhập với username/password
   - Xác thực từ database
   - Sử dụng parameterized queries (chống SQL injection)
   - Lưu thông tin user vào GlobalVariables

4. **MainForm** ✅
   - Menu điều hướng
   - Nút truy cập nhanh các chức năng
   - Hiển thị thông tin người dùng
   - Đăng xuất

5. **CategoryManagementForm** ✅
   - CRUD đầy đủ theo mẫu trong yêu cầu
   - DataGridView hiển thị danh sách
   - Các nút: Thêm, Sửa, Xóa, Lưu, Hủy, Thoát
   - Validation đầy đủ
   - Kiểm tra ràng buộc (không xóa nếu có sản phẩm)

### 🚧 Cần Triển Khai

6. **ProductManagementForm** (Cần code)
   - CRUD sản phẩm
   - ComboBox chọn danh mục
   - Tìm kiếm sản phẩm
   - Validation: Giá > 0, Tồn kho >= 0

7. **CustomerManagementForm** (Cần code)
   - CRUD khách hàng
   - Validation phone, email

8. **OrderManagementForm** (Cần code)
   - Master-detail view (Orders và OrderDetails)
   - Chọn khách hàng từ ComboBox
   - Chọn sản phẩm và số lượng
   - Tự động tính tổng tiền
   - Cập nhật tồn kho khi hoàn thành đơn hàng

## Hướng Dẫn Cài Đặt

### Bước 1: Cài Đặt MySQL Database

1. Cài đặt MySQL Server (hoặc XAMPP/WAMP)
2. Mở MySQL Workbench hoặc phpMyAdmin
3. Chạy file `database_script.sql`
4. Kiểm tra database `BuildingMaterialDB` đã được tạo

### Bước 2: Cấu Hình Connection String

Mở file `App.config` và cập nhật connection string:

```xml
<add name="BuildingMaterialDB" 
     connectionString="Server=localhost;Database=BuildingMaterialDB;User Id=root;Password=YOUR_PASSWORD;" 
     providerName="MySql.Data.MySqlClient" />
```

Thay `YOUR_PASSWORD` bằng mật khẩu MySQL của bạn (mặc định thường để trống).

### Bước 3: Tạo Project C# trong Visual Studio

1. Mở Visual Studio
2. Tạo project mới: **Windows Forms App (.NET Framework)**
3. Tên project: **BuildingMaterialManager**
4. Framework: **.NET Framework 4.7.2** hoặc cao hơn

### Bước 4: Cài Đặt MySQL.Data Package

1. Trong Visual Studio, vào **Tools** → **NuGet Package Manager** → **Manage NuGet Packages for Solution**
2. Tìm và cài đặt: **MySql.Data**
3. Hoặc dùng Package Manager Console:
```
Install-Package MySql.Data
```

### Bước 5: Copy Code Files

Copy tất cả các file `.cs` đã được tạo vào project:
- `DatabaseHelper.cs` → vào thư mục gốc
- `GlobalVariables.cs` → vào thư mục gốc
- `Program.cs` → thay thế file có sẵn
- `App.config` → thay thế file có sẵn
- Tất cả file trong `Forms/` → tạo thư mục Forms trong project

### Bước 6: Build và Run

1. Build project: **Build** → **Build Solution** (Ctrl+Shift+B)
2. Chạy: **Debug** → **Start Debugging** (F5)

## Tài Khoản Mặc Định

| Username | Password | Role  |
|----------|----------|-------|
| admin    | admin123 | Admin |
| staff1   | staff123 | Staff |
| staff2   | staff123 | Staff |

## Cấu Trúc Database

```
Users (Người dùng)
├── UserID (PK)
├── Username
├── Password
├── FullName
├── Role
└── IsActive

Categories (Danh mục)
├── CategoryID (PK)
├── CategoryName
└── Description

Products (Sản phẩm)
├── ProductID (PK)
├── ProductCode
├── ProductName
├── CategoryID (FK → Categories)
├── Unit
├── UnitPrice
├── QuantityInStock
└── Supplier

Customers (Khách hàng)
├── CustomerID (PK)
├── CustomerCode
├── CustomerName
├── Phone
├── Address
└── City

Orders (Đơn hàng)
├── OrderID (PK)
├── OrderCode
├── CustomerID (FK → Customers)
├── OrderDate
├── TotalAmount
├── Status
└── UserID (FK → Users)

OrderDetails (Chi tiết đơn hàng)
├── DetailID (PK)
├── OrderID (FK → Orders)
├── ProductID (FK → Products)
├── Quantity
├── UnitPrice
└── TotalPrice
```

## Các Form Cần Triển Khai

### ProductManagementForm

**UI Components:**
- DataGridView: Hiển thị sản phẩm
- TextBox: ProductCode, ProductName, Unit, UnitPrice, QuantityInStock, Supplier
- ComboBox: Chọn Category
- TextBox: Search
- Buttons: Thêm, Sửa, Xóa, Lưu, Hủy, Tìm kiếm, Thoát

**Logic:**
```csharp
- Load Categories vào ComboBox
- JOIN Products với Categories để hiển thị CategoryName
- Validation: ProductCode unique, UnitPrice > 0, QuantityInStock >= 0
- Search: Filter theo ProductCode hoặc ProductName
```

### CustomerManagementForm

**UI Components:**
- DataGridView: Hiển thị khách hàng
- TextBox: CustomerCode, CustomerName, Phone, Address, City
- Buttons: Thêm, Sửa, Xóa, Lưu, Hủy, Thoát

**Logic:**
```csharp
- CRUD cơ bản như CategoryManagementForm
- Validation: Phone format (regex), CustomerCode unique
- Không xóa nếu có đơn hàng
```

### OrderManagementForm

**UI Components:**
- DataGridView 1: Danh sách Orders
- DataGridView 2: Chi tiết OrderDetails của order được chọn
- Order Header: OrderCode, ComboBox Customer, DateTimePicker, ComboBox Status, TextBox TotalAmount
- Order Details: ComboBox Product, TextBox Quantity, UnitPrice, TotalPrice
- Buttons: Thêm đơn, Lưu đơn, Xóa đơn, Thêm sản phẩm, Xóa sản phẩm

**Logic:**
```csharp
- Load Customers vào ComboBox
- Load Products vào ComboBox
- Khi chọn Product: Auto-fill UnitPrice
- Tính TotalPrice = Quantity * UnitPrice
- Tính TotalAmount = Sum(TotalPrice) của tất cả details
- Khi Status = "Completed": Giảm QuantityInStock trong Products
- Validation: Kiểm tra đủ tồn kho trước khi hoàn thành
```

## Tài Liệu Tham Khảo

1. **MySQL Documentation**: https://dev.mysql.com/doc/
2. **C# Windows Forms**: https://docs.microsoft.com/en-us/dotnet/desktop/winforms/
3. **MySQL Connector/NET**: https://dev.mysql.com/doc/connector-net/en/
4. **DataGridView Tutorial**: https://docs.microsoft.com/en-us/dotnet/desktop/winforms/controls/datagridview-control-windows-forms

## Lưu Ý Quan Trọng

1. ✅ **Đã có Login/Logout** - Yêu cầu bắt buộc
2. ✅ **Có ràng buộc và validation** - Yêu cầu bắt buộc
3. ✅ **Sử dụng Parameterized Queries** - Chống SQL injection
4. ✅ **Thiết kế đơn giản** - Phù hợp với yêu cầu đồ án
5. ⚠️ **Cần hoàn thiện 3 form còn lại**

## Timeline Thực Hiện

- ✅ Ngày 1-2: Database + Login + Main (Đã hoàn thành)
- ✅ Ngày 3: CategoryManagement (Đã hoàn thành)
- 🚧 Ngày 4: ProductManagement (Cần làm)
- 🚧 Ngày 5: CustomerManagement (Cần làm)
- 🚧 Ngày 6-7: OrderManagement (Cần làm)
- 📅 Ngày 8-9: Testing + Bug fixes
- 📅 Ngày 10: Báo cáo PowerPoint + Word

## Báo Cáo Cần Chuẩn Bị

### PowerPoint Presentation
1. Trang 1: Tên đồ án, thông tin sinh viên
2. Trang 2: Mục đích yêu cầu
3. Trang 3: Người dùng mục tiêu
4. Trang 4-5: Thiết kế hệ thống (ERD, Use Case)
5. Trang 6-10: Screenshots các form
6. Trang 11: Các tính năng đã cài đặt
7. Trang 12: Tài liệu tham khảo

### Screenshots Cần Chụp
- LoginForm
- MainForm
- CategoryManagementForm
- ProductManagementForm
- CustomerManagementForm
- OrderManagementForm
- Validation messages
- Database diagram

## Hỗ Trợ và Liên Hệ

Nếu gặp vấn đề:
1. Kiểm tra MySQL Server đã chạy
2. Kiểm tra connection string trong App.config
3. Kiểm tra database đã được import
4. Kiểm tra MySQL.Data package đã cài đặt

## License

Đây là dự án học tập cho môn Lập trình .NET.
