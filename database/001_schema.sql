USE [master];
GO

IF DB_ID(N'ValuetechComunas') IS NULL
BEGIN
    CREATE DATABASE [ValuetechComunas];
END;
GO

ALTER DATABASE [ValuetechComunas]
SET COMPATIBILITY_LEVEL = 110;
GO

USE [ValuetechComunas];
GO

IF OBJECT_ID(N'dbo.Regiones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Regiones
    (
        IdRegion INT IDENTITY(1, 1) NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Regiones
            PRIMARY KEY CLUSTERED (IdRegion),

        CONSTRAINT UQ_Regiones_Nombre
            UNIQUE (Nombre)
    );
END;
GO

IF OBJECT_ID(N'dbo.Comunas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comunas
    (
        IdComuna INT IDENTITY(1, 1) NOT NULL,
        IdRegion INT NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Comunas
            PRIMARY KEY CLUSTERED (IdComuna),

        CONSTRAINT FK_Comunas_Regiones
            FOREIGN KEY (IdRegion)
            REFERENCES dbo.Regiones (IdRegion),

        CONSTRAINT UQ_Comunas_Region_Nombre
            UNIQUE (IdRegion, Nombre)
    );
END;
GO
