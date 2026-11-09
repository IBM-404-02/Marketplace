

CREATE DATABASE MarketplaceDb;
GO

USE MarketplaceDb;
GO


CREATE TABLE Roles (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Categories (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE DeliveryMethods (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE OrderStatuses (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE PickupPoints (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Address      NVARCHAR(300) NOT NULL,
    WorkingHours NVARCHAR(100) NULL,
    IsActive     BIT NOT NULL DEFAULT 1
);


CREATE TABLE Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Login        NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    RoleId       INT NOT NULL,
    RegisteredAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

CREATE TABLE Products (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Description   NVARCHAR(MAX) NULL,
    Price         DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
    ImagePath     NVARCHAR(260) NULL,
    StockQuantity INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
    PackQuantity  INT NULL CHECK (PackQuantity > 0),
    IsAvailable   BIT NOT NULL DEFAULT 1
);

CREATE TABLE Orders (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    UserId           INT NOT NULL,
    OrderDate        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DeliveryMethodId INT NOT NULL,
    PickupPointId    INT NULL,
    DeliveryAddress  NVARCHAR(300) NULL,
    StatusId         INT NOT NULL,
    CONSTRAINT FK_Orders_Users           FOREIGN KEY (UserId)           REFERENCES Users(Id),
    CONSTRAINT FK_Orders_DeliveryMethods FOREIGN KEY (DeliveryMethodId) REFERENCES DeliveryMethods(Id),
    CONSTRAINT FK_Orders_PickupPoints    FOREIGN KEY (PickupPointId)    REFERENCES PickupPoints(Id),
    CONSTRAINT FK_Orders_OrderStatuses   FOREIGN KEY (StatusId)         REFERENCES OrderStatuses(Id),
    CONSTRAINT CK_Orders_Delivery CHECK (
        (PickupPointId IS NOT NULL AND DeliveryAddress IS NULL)
        OR
        (PickupPointId IS NULL AND DeliveryAddress IS NOT NULL)
    )
);


CREATE TABLE ProductCategories (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    ProductId  INT NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT FK_ProductCategories_Products   FOREIGN KEY (ProductId)  REFERENCES Products(Id),
    CONSTRAINT FK_ProductCategories_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT UQ_ProductCategories UNIQUE (ProductId, CategoryId)
);

CREATE TABLE CartItems (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity  INT NOT NULL CHECK (Quantity > 0),
    CONSTRAINT FK_CartItems_Users    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_CartItems UNIQUE (UserId, ProductId)
);

CREATE TABLE Favorites (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    ProductId INT NOT NULL,
    AddedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Favorites_Users    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_Favorites_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_Favorites UNIQUE (UserId, ProductId)
);

CREATE TABLE Reviews (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    ProductId INT NOT NULL,
    Rating    TINYINT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Text      NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Reviews_Users    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_Reviews UNIQUE (UserId, ProductId)
);

CREATE TABLE OrderItems (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL,
    ProductId       INT NOT NULL,
    Quantity        INT NOT NULL CHECK (Quantity > 0),
    PriceAtPurchase DECIMAL(10,2) NOT NULL CHECK (PriceAtPurchase >= 0),
    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_OrderItems UNIQUE (OrderId, ProductId)
);
GO

-- ---------- Тестовые данные ----------

INSERT INTO Roles (Name) VALUES (N'User'), (N'Admin');

INSERT INTO DeliveryMethods (Name) VALUES (N'Самовывоз'), (N'Доставка курьером');

INSERT INTO OrderStatuses (Name) VALUES
    (N'Оформлен'), (N'В обработке'), (N'Передан в доставку'),
    (N'Готов к выдаче'), (N'Получен'), (N'Отменён');

INSERT INTO Categories (Name) VALUES
    (N'Электроника'), (N'Канцелярия'), (N'Продукты'), (N'Подарки');

INSERT INTO PickupPoints (Address, WorkingHours) VALUES
    (N'ул. Тверская, 10', N'09:00–21:00'),
    (N'пр. Мира, 45',     N'10:00–22:00'),
    (N'ул. Ленина, 3',    N'08:00–20:00');

INSERT INTO Products (Name, Description, Price, ImagePath, StockQuantity, PackQuantity, IsAvailable) VALUES
    (N'Беспроводные наушники', N'Bluetooth 5.3, до 30 часов работы', 2490.00, N'headphones.jpg', 15,  NULL, 1),
    (N'Powerbank 10000 мАч',   N'Быстрая зарядка, два USB-порта',   1290.00, N'powerbank.jpg',  8,   NULL, 1),
    (N'Ручка гелевая синяя',   N'Толщина линии 0.5 мм',                45.00, N'pen.jpg',        200, 10,   1),
    (N'Тетрадь 96 листов',     N'Клетка, твёрдая обложка',            180.00, N'notebook.jpg',   50,  NULL, 1),
    (N'Шоколад тёмный',        N'70% какао, 100 г',                   120.00, N'chocolate.jpg',  0,   NULL, 1),
    (N'Кофе в зёрнах',         N'Арабика, 1 кг',                     1450.00, N'coffee.jpg',     12,  NULL, 1),
    (N'Подарочный набор',      N'Кружка и шоколад в коробке',         890.00, N'giftbox.jpg',    5,   NULL, 0),
    (N'USB-флешка 64 ГБ',      N'USB 3.0',                            590.00, N'flash.jpg',      30,  NULL, 1);

INSERT INTO ProductCategories (ProductId, CategoryId) VALUES
    (1, 1), (2, 1), (3, 2), (4, 2), (5, 3), (6, 3),
    (7, 4), (7, 3), (5, 4), (1, 4), (8, 1);
GO-- =============================================
-- Маркетплейс: создание базы данных
-- =============================================

CREATE DATABASE MarketplaceDb;
GO

USE MarketplaceDb;
GO

-- ---------- Справочники ----------

CREATE TABLE Roles (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Categories (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE DeliveryMethods (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE OrderStatuses (
    Id   INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE PickupPoints (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Address      NVARCHAR(300) NOT NULL,
    WorkingHours NVARCHAR(100) NULL,
    IsActive     BIT NOT NULL DEFAULT 1
);


CREATE TABLE Users (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Login        NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    RoleId       INT NOT NULL,
    RegisteredAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

CREATE TABLE Products (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Description   NVARCHAR(MAX) NULL,
    Price         DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
    ImagePath     NVARCHAR(260) NULL,
    StockQuantity INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
    PackQuantity  INT NULL CHECK (PackQuantity > 0),
    IsAvailable   BIT NOT NULL DEFAULT 1
);

CREATE TABLE Orders (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    UserId           INT NOT NULL,
    OrderDate        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DeliveryMethodId INT NOT NULL,
    PickupPointId    INT NULL,
    DeliveryAddress  NVARCHAR(300) NULL,
    StatusId         INT NOT NULL,
    CONSTRAINT FK_Orders_Users           FOREIGN KEY (UserId)           REFERENCES Users(Id),
    CONSTRAINT FK_Orders_DeliveryMethods FOREIGN KEY (DeliveryMethodId) REFERENCES DeliveryMethods(Id),
    CONSTRAINT FK_Orders_PickupPoints    FOREIGN KEY (PickupPointId)    REFERENCES PickupPoints(Id),
    CONSTRAINT FK_Orders_OrderStatuses   FOREIGN KEY (StatusId)         REFERENCES OrderStatuses(Id),
    CONSTRAINT CK_Orders_Delivery CHECK (
        (PickupPointId IS NOT NULL AND DeliveryAddress IS NULL)
        OR
        (PickupPointId IS NULL AND DeliveryAddress IS NOT NULL)
    )
);


CREATE TABLE ProductCategories (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    ProductId  INT NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT FK_ProductCategories_Products   FOREIGN KEY (ProductId)  REFERENCES Products(Id),
    CONSTRAINT FK_ProductCategories_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT UQ_ProductCategories UNIQUE (ProductId, CategoryId)
);

CREATE TABLE CartItems (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity  INT NOT NULL CHECK (Quantity > 0),
    CONSTRAINT FK_CartItems_Users    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_CartItems UNIQUE (UserId, ProductId)
);

CREATE TABLE Favorites (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    ProductId INT NOT NULL,
    AddedAt   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Favorites_Users    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_Favorites_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_Favorites UNIQUE (UserId, ProductId)
);

CREATE TABLE Reviews (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT NOT NULL,
    ProductId INT NOT NULL,
    Rating    TINYINT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Text      NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Reviews_Users    FOREIGN KEY (UserId)    REFERENCES Users(Id),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_Reviews UNIQUE (UserId, ProductId)
);

CREATE TABLE OrderItems (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL,
    ProductId       INT NOT NULL,
    Quantity        INT NOT NULL CHECK (Quantity > 0),
    PriceAtPurchase DECIMAL(10,2) NOT NULL CHECK (PriceAtPurchase >= 0),
    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_OrderItems UNIQUE (OrderId, ProductId)
);
GO


INSERT INTO Roles (Name) VALUES (N'User'), (N'Admin');

INSERT INTO DeliveryMethods (Name) VALUES (N'Самовывоз'), (N'Доставка курьером');

INSERT INTO OrderStatuses (Name) VALUES
    (N'Оформлен'), (N'В обработке'), (N'Передан в доставку'),
    (N'Готов к выдаче'), (N'Получен'), (N'Отменён');

INSERT INTO Categories (Name) VALUES
    (N'Электроника'), (N'Канцелярия'), (N'Продукты'), (N'Подарки');

INSERT INTO PickupPoints (Address, WorkingHours) VALUES
    (N'ул. Тверская, 10', N'09:00–21:00'),
    (N'пр. Мира, 45',     N'10:00–22:00'),
    (N'ул. Ленина, 3',    N'08:00–20:00');

INSERT INTO Products (Name, Description, Price, ImagePath, StockQuantity, PackQuantity, IsAvailable) VALUES
    (N'Беспроводные наушники', N'Bluetooth 5.3, до 30 часов работы', 2490.00, N'headphones.jpg', 15,  NULL, 1),
    (N'Powerbank 10000 мАч',   N'Быстрая зарядка, два USB-порта',   1290.00, N'powerbank.jpg',  8,   NULL, 1),
    (N'Ручка гелевая синяя',   N'Толщина линии 0.5 мм',                45.00, N'pen.jpg',        200, 10,   1),
    (N'Тетрадь 96 листов',     N'Клетка, твёрдая обложка',            180.00, N'notebook.jpg',   50,  NULL, 1),
    (N'Шоколад тёмный',        N'70% какао, 100 г',                   120.00, N'chocolate.jpg',  0,   NULL, 1),
    (N'Кофе в зёрнах',         N'Арабика, 1 кг',                     1450.00, N'coffee.jpg',     12,  NULL, 1),
    (N'Подарочный набор',      N'Кружка и шоколад в коробке',         890.00, N'giftbox.jpg',    5,   NULL, 0),
    (N'USB-флешка 64 ГБ',      N'USB 3.0',                            590.00, N'flash.jpg',      30,  NULL, 1);

INSERT INTO ProductCategories (ProductId, CategoryId) VALUES
    (1, 1), (2, 1), (3, 2), (4, 2), (5, 3), (6, 3),
    (7, 4), (7, 3), (5, 4), (1, 4), (8, 1);
GO