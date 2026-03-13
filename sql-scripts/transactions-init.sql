/*
  SCRIPT DE INICIALIZACIÓN - MICROSERVICIO TRANSACCIONES
*/

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TransactionsDB')
BEGIN
    CREATE DATABASE TransactionsDB;
END
GO

USE TransactionsDB;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE Transactions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Date DATETIME NOT NULL,
        Type INT NOT NULL, -- 0: Compra, 1: Venta
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        Detail NVARCHAR(MAX)
    );

    INSERT INTO Transactions (Date, Type, ProductId, Quantity, UnitPrice, TotalPrice, Detail)
    VALUES 
    (GETDATE(), 0, 1, 15, 1100.00, 16500.00, 'Inventario inicial de MacBook Air'),
    (GETDATE(), 0, 2, 8, 1350.00, 10800.00, 'Inventario inicial de Dell XPS'),
    (DATEADD(day, -1, GETDATE()), 0, 3, 25, 80.00, 2000.00, 'Compra a proveedor Logitech'),
    (DATEADD(day, -2, GETDATE()), 1, 1, 1, 1199.99, 1199.99, 'Venta cliente final'),
    (DATEADD(day, -3, GETDATE()), 1, 3, 2, 99.00, 198.00, 'Venta accesorios'),
    (DATEADD(day, -4, GETDATE()), 0, 5, 10, 400.00, 4000.00, 'Stock de monitores LG');
END
GO

-- Crear tabla de historia de migraciones falsa
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20260312230954_InitialCreate', '9.0.0');
END
GO
