/*
  SCRIPT DE INICIALIZACIÓN - MICROSERVICIO PRODUCTOS
  Datos reales para inventario de tecnología y oficina.
*/

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductsDB')
BEGIN
    CREATE DATABASE ProductsDB;
END
GO

USE ProductsDB;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Productos]') AND type in (N'U'))
BEGIN
    CREATE TABLE Productos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500),
        Category NVARCHAR(100),
        Price DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL,
        ImageUrl NVARCHAR(MAX)
    );

    -- Datos de Inicio para Productos
    INSERT INTO Productos (Name, Description, Category, Price, StockQuantity, ImageUrl)
    VALUES 
    ('MacBook Air M2', 'Laptop Apple de 13 pulgadas, 8GB RAM, 256GB SSD, color Medianoche.', 'Laptops', 1199.99, 15, '/images/default.png'),
    ('Dell XPS 15', 'Laptop potente para diseño y desarrollo, i7, 16GB RAM, 512GB SSD.', 'Laptops', 1450.50, 8, '/images/default.png'),
    ('Logitech MX Master 3S', 'Mouse inalámbrico ergonómico de alta precisión con scroll magnético.', 'Accesorios', 99.00, 25, '/images/default.png'),
    ('Keychron K2 V2', 'Teclado mecánico wireless 75% con switches Gateron Brown.', 'Accesorios', 89.99, 12, '/images/default.png'),
    ('LG UltraWide 34"', 'Monitor panorámico 21:9 para máxima productividad multitarea.', 'Monitores', 450.00, 10, '/images/default.png'),
    ('Sony WH-1000XM5', 'Auriculares inalámbricos con cancelación de ruido líder en la industria.', 'Audio', 349.99, 20, '/images/default.png'),
    ('Silla Ergonómica Pro', 'Silla de oficina con soporte lumbar ajustable y malla transpirable.', 'Muebles', 280.00, 15, '/images/default.png'),
    ('Standing Desk Motorizado', 'Escritorio con ajuste de altura eléctrico y memoria de posiciones.', 'Muebles', 399.00, 5, '/images/default.png'),
    ('iPad Air 5', 'Tablet potente con chip M1, pantalla Liquid Retina de 10.9 pulgadas.', 'Tablets', 599.00, 18, '/images/default.png'),
    ('Kindle Paperwhite', 'Lector de libros electrónicos con luz ajustable y pantalla de 6.8".', 'Gadgets', 139.99, 30, '/images/default.png');
END
GO

-- Crear tabla de historia de migraciones falsa para que EF Core no intente migrar nada si se corre localmente
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260312152148_InitialCreate', '9.0.0');
END
GO
