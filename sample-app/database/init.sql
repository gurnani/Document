


CREATE TABLE Categories (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500),
    ImageUrl nvarchar(500),
    ParentCategoryId int NULL,
    IsActive bit NOT NULL DEFAULT 1,
    SortOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Products (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(2000),
    Price decimal(18,2) NOT NULL,
    ImageUrl nvarchar(500),
    CategoryId int NOT NULL,
    InStock bit NOT NULL DEFAULT 1,
    StockQuantity int NOT NULL DEFAULT 0,
    Tags nvarchar(1000),
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1,
    IsFeatured bit NOT NULL DEFAULT 0,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

/*
CREATE TABLE AspNetUsers (
    Id nvarchar(450) PRIMARY KEY,
    UserName nvarchar(256),
    NormalizedUserName nvarchar(256),
    Email nvarchar(256),
    NormalizedEmail nvarchar(256),
    EmailConfirmed bit NOT NULL,
    PasswordHash nvarchar(max),
    SecurityStamp nvarchar(max),
    ConcurrencyStamp nvarchar(max),
    PhoneNumber nvarchar(max),
    PhoneNumberConfirmed bit NOT NULL,
    TwoFactorEnabled bit NOT NULL,
    LockoutEnd datetimeoffset,
    LockoutEnabled bit NOT NULL,
    AccessFailedCount int NOT NULL,
    FirstName nvarchar(100) NOT NULL,
    LastName nvarchar(100) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt datetime2,
    IsActive bit NOT NULL DEFAULT 1
);
*/

CREATE TABLE Orders (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    Status int NOT NULL DEFAULT 0, -- 0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled, 5=Refunded
    Subtotal decimal(18,2) NOT NULL,
    Tax decimal(18,2) NOT NULL,
    Shipping decimal(18,2) NOT NULL,
    Total decimal(18,2) NOT NULL,
    TrackingNumber nvarchar(100),
    ShippedAt datetime2,
    DeliveredAt datetime2,
    OrderNotes nvarchar(1000),
    
    ShippingFirstName nvarchar(100) NOT NULL,
    ShippingLastName nvarchar(100) NOT NULL,
    ShippingCompany nvarchar(100),
    ShippingAddress1 nvarchar(200) NOT NULL,
    ShippingAddress2 nvarchar(200),
    ShippingCity nvarchar(100) NOT NULL,
    ShippingState nvarchar(100) NOT NULL,
    ShippingZipCode nvarchar(20) NOT NULL,
    ShippingCountry nvarchar(100) NOT NULL,
    ShippingPhone nvarchar(20),
    
    BillingFirstName nvarchar(100) NOT NULL,
    BillingLastName nvarchar(100) NOT NULL,
    BillingCompany nvarchar(100),
    BillingAddress1 nvarchar(200) NOT NULL,
    BillingAddress2 nvarchar(200),
    BillingCity nvarchar(100) NOT NULL,
    BillingState nvarchar(100) NOT NULL,
    BillingZipCode nvarchar(20) NOT NULL,
    BillingCountry nvarchar(100) NOT NULL,
    
    PaymentType int NOT NULL, -- 0=CreditCard, 1=PayPal, 2=ApplePay, 3=GooglePay
    PaymentReference nvarchar(100),
    
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

CREATE TABLE OrderItems (
    Id int IDENTITY(1,1) PRIMARY KEY,
    OrderId int NOT NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL,
    Price decimal(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE CartItems (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    UNIQUE(UserId, ProductId)
);

CREATE TABLE Reviews (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    ProductId int NOT NULL,
    Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Title nvarchar(200) NOT NULL,
    Comment nvarchar(2000) NOT NULL,
    IsVerifiedPurchase bit NOT NULL DEFAULT 0,
    HelpfulCount int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    UNIQUE(UserId, ProductId)
);

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_IsFeatured ON Products(IsFeatured);
CREATE INDEX IX_Products_Name ON Products(Name);

CREATE INDEX IX_Categories_ParentCategoryId ON Categories(ParentCategoryId);
CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);
CREATE INDEX IX_Categories_Name ON Categories(Name);

CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);

CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);

CREATE INDEX IX_CartItems_UserId ON CartItems(UserId);
CREATE INDEX IX_CartItems_ProductId ON CartItems(ProductId);

CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Reviews_Rating ON Reviews(Rating);
CREATE INDEX IX_Reviews_IsActive ON Reviews(IsActive);

INSERT INTO Categories (Name, Description, IsActive, SortOrder) VALUES
('Electronics', 'Electronic devices and gadgets', 1, 1),
('Clothing', 'Fashion and apparel', 1, 2),
('Books', 'Books and literature', 1, 3),
('Home & Garden', 'Home improvement and gardening', 1, 4),
('Sports', 'Sports and outdoor activities', 1, 5);

DECLARE @ElectronicsId int = (SELECT Id FROM Categories WHERE Name = 'Electronics');
DECLARE @ClothingId int = (SELECT Id FROM Categories WHERE Name = 'Clothing');

INSERT INTO Categories (Name, Description, ParentCategoryId, IsActive, SortOrder) VALUES
('Smartphones', 'Mobile phones and accessories', @ElectronicsId, 1, 1),
('Laptops', 'Laptops and computers', @ElectronicsId, 1, 2),
('Men''s Clothing', 'Clothing for men', @ClothingId, 1, 1),
('Women''s Clothing', 'Clothing for women', @ClothingId, 1, 2);

DECLARE @SmartphonesId int = (SELECT Id FROM Categories WHERE Name = 'Smartphones');
DECLARE @LaptopsId int = (SELECT Id FROM Categories WHERE Name = 'Laptops');
DECLARE @BooksId int = (SELECT Id FROM Categories WHERE Name = 'Books');

INSERT INTO Products (Name, Description, Price, CategoryId, ImageUrl, InStock, StockQuantity, Tags, IsFeatured, IsActive) VALUES
('iPhone 15 Pro', 'Latest iPhone with advanced camera system and A17 Pro chip', 999.99, @SmartphonesId, 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=400', 1, 50, 'smartphone,apple,premium', 1, 1),
('MacBook Pro 14"', 'Powerful laptop with M3 chip for professionals', 1999.99, @LaptopsId, 'https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=400', 1, 25, 'laptop,apple,professional', 1, 1),
('Samsung Galaxy S24', 'Android smartphone with excellent camera and display', 799.99, @SmartphonesId, 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400', 1, 40, 'smartphone,samsung,android', 0, 1),
('The Art of Programming', 'Comprehensive guide to software development best practices', 49.99, @BooksId, 'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400', 1, 30, 'programming,technology,education', 0, 1),
('Modern Web Development', 'Learn the latest web technologies and frameworks', 39.99, @BooksId, 'https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400', 1, 45, 'web development,javascript,react', 1, 1),
('Wireless Headphones', 'High-quality wireless headphones with noise cancellation', 199.99, @ElectronicsId, 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400', 1, 60, 'headphones,wireless,audio', 1, 1);
