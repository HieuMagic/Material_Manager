-- Building Material Manager Database Script
-- Database: BuildingMaterialDB
-- Created for .NET Programming Assignment

-- Create database
CREATE DATABASE IF NOT EXISTS BuildingMaterialDB;
USE BuildingMaterialDB;

-- Drop tables if they exist (for clean installation)
DROP TABLE IF EXISTS OrderDetails;
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Users;

-- Table 1: Users (for login/logout)
CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL, -- Admin, Staff
    IsActive BIT DEFAULT 1
);

-- Table 2: Categories
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY AUTO_INCREMENT,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

-- Table 3: Products
CREATE TABLE Products (
    ProductID INT PRIMARY KEY AUTO_INCREMENT,
    ProductCode VARCHAR(50) NOT NULL UNIQUE,
    ProductName NVARCHAR(100) NOT NULL,
    CategoryID INT NOT NULL,
    Unit NVARCHAR(20), -- Cái, kg, m, m2, bao, etc.
    UnitPrice DECIMAL(10, 2) NOT NULL,
    QuantityInStock INT DEFAULT 0,
    Supplier NVARCHAR(100),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Table 4: Customers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY AUTO_INCREMENT,
    CustomerCode VARCHAR(50) NOT NULL UNIQUE,
    CustomerName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20),
    Address NVARCHAR(255),
    City NVARCHAR(100)
);

-- Table 5: Orders
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY AUTO_INCREMENT,
    OrderCode VARCHAR(50) NOT NULL UNIQUE,
    CustomerID INT NOT NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(10, 2),
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Cancelled
    UserID INT NOT NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Table 6: OrderDetails
CREATE TABLE OrderDetails (
    DetailID INT PRIMARY KEY AUTO_INCREMENT,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    TotalPrice DECIMAL(10, 2),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Insert sample data for testing

-- Insert Users
INSERT INTO Users (Username, Password, FullName, Role, IsActive) VALUES
('admin', 'admin123', 'Administrator', 'Admin', 1),
('staff1', 'staff123', 'Nguyen Van A', 'Staff', 1),
('staff2', 'staff123', 'Tran Thi B', 'Staff', 1);

-- Insert Categories
INSERT INTO Categories (CategoryName, Description) VALUES
('Xi măng', 'Các loại xi măng xây dựng'),
('Gạch', 'Gạch xây, gạch ốp lát'),
('Sắt thép', 'Thép xây dựng các loại'),
('Cát đá', 'Cát, đá xây dựng'),
('Sơn', 'Sơn nước, sơn dầu các loại');

-- Insert Products
INSERT INTO Products (ProductCode, ProductName, CategoryID, Unit, UnitPrice, QuantityInStock, Supplier) VALUES
('XM001', 'Xi măng PCB40', 1, 'Bao', 95000.00, 500, 'Công ty Xi măng Hoàng Thạch'),
('XM002', 'Xi măng PPC40', 1, 'Bao', 88000.00, 300, 'Công ty Xi măng Hoàng Thạch'),
('G001', 'Gạch đỏ 4 lỗ', 2, 'Viên', 1500.00, 10000, 'Gạch Đồng Nai'),
('G002', 'Gạch ốp lát 30x30', 2, 'Viên', 35000.00, 2000, 'Gạch Prime'),
('G003', 'Gạch ốp lát 40x40', 2, 'Viên', 52000.00, 1500, 'Gạch Prime'),
('ST001', 'Thép CB240 D10', 3, 'Cây', 120000.00, 200, 'Thép Việt Nhật'),
('ST002', 'Thép CB300 D10', 3, 'Cây', 135000.00, 150, 'Thép Việt Nhật'),
('ST003', 'Thép CB240 D8', 3, 'Cây', 85000.00, 250, 'Thép Việt Nhật'),
('CD001', 'Cát vàng', 4, 'm3', 350000.00, 50, 'Cát đá Bình Dương'),
('CD002', 'Đá 1x2', 4, 'm3', 420000.00, 40, 'Cát đá Bình Dương'),
('CD003', 'Đá 4x6', 4, 'm3', 380000.00, 60, 'Cát đá Bình Dương'),
('S001', 'Sơn Dulux ngoại thất', 5, 'Thùng', 2500000.00, 30, 'Sơn Dulux Vietnam'),
('S002', 'Sơn Dulux nội thất', 5, 'Thùng', 2200000.00, 40, 'Sơn Dulux Vietnam'),
('S003', 'Sơn Jotun ngoại thất', 5, 'Thùng', 2800000.00, 25, 'Sơn Jotun Vietnam');

-- Insert Customers
INSERT INTO Customers (CustomerCode, CustomerName, Phone, Address, City) VALUES
('KH001', 'Công ty TNHH Xây dựng Hoàng Long', '0901234567', '123 Đường ABC', 'Hồ Chí Minh'),
('KH002', 'Công ty CP Xây dựng Việt Nam', '0912345678', '456 Đường XYZ', 'Hà Nội'),
('KH003', 'Nguyễn Văn C', '0923456789', '789 Đường DEF', 'Hồ Chí Minh'),
('KH004', 'Trần Thị D', '0934567890', '321 Đường GHI', 'Đà Nẵng'),
('KH005', 'Lê Văn E', '0945678901', '654 Đường JKL', 'Hồ Chí Minh');

-- Insert Sample Orders
INSERT INTO Orders (OrderCode, CustomerID, OrderDate, TotalAmount, Status, UserID) VALUES
('DH001', 1, '2025-11-01 10:30:00', 5250000.00, 'Completed', 2),
('DH002', 2, '2025-11-03 14:20:00', 3500000.00, 'Completed', 2),
('DH003', 3, '2025-11-05 09:15:00', 1500000.00, 'Pending', 3),
('DH004', 4, '2025-11-07 16:45:00', 2750000.00, 'Pending', 2);

-- Insert OrderDetails for DH001
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, TotalPrice) VALUES
(1, 1, 50, 95000.00, 4750000.00),
(1, 6, 4, 120000.00, 480000.00),
(1, 9, 1, 350000.00, 350000.00);

-- Insert OrderDetails for DH002
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, TotalPrice) VALUES
(2, 3, 1000, 1500.00, 1500000.00),
(2, 4, 50, 35000.00, 1750000.00);

-- Insert OrderDetails for DH003
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, TotalPrice) VALUES
(3, 2, 10, 88000.00, 880000.00),
(3, 8, 5, 85000.00, 425000.00);

-- Insert OrderDetails for DH004
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, TotalPrice) VALUES
(4, 12, 1, 2500000.00, 2500000.00),
(4, 9, 1, 350000.00, 350000.00);

-- Display success message
SELECT 'Database created successfully!' AS Message;

-- Verify data
SELECT 'Users:' AS Table_Name, COUNT(*) AS Record_Count FROM Users
UNION ALL
SELECT 'Categories:', COUNT(*) FROM Categories
UNION ALL
SELECT 'Products:', COUNT(*) FROM Products
UNION ALL
SELECT 'Customers:', COUNT(*) FROM Customers
UNION ALL
SELECT 'Orders:', COUNT(*) FROM Orders
UNION ALL
SELECT 'OrderDetails:', COUNT(*) FROM OrderDetails;
