USE [ValuetechComunas];
GO

/* 1. Listar regiones */

IF OBJECT_ID(N'dbo.Regiones_Listar', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.Regiones_Listar;
END;
GO

CREATE PROCEDURE dbo.Regiones_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdRegion AS Id,
        Nombre
    FROM dbo.Regiones
    ORDER BY Nombre;
END;
GO

/* 2. Obtener una región */

IF OBJECT_ID(N'dbo.Regiones_ObtenerPorId', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.Regiones_ObtenerPorId;
END;
GO

CREATE PROCEDURE dbo.Regiones_ObtenerPorId
    @IdRegion INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdRegion AS Id,
        Nombre
    FROM dbo.Regiones
    WHERE IdRegion = @IdRegion;
END;
GO

/* 3. Listar comunas de una región */

IF OBJECT_ID(N'dbo.Comunas_ListarPorRegion', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.Comunas_ListarPorRegion;
END;
GO

CREATE PROCEDURE dbo.Comunas_ListarPorRegion
    @IdRegion INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdComuna AS Id,
        IdRegion AS RegionId,
        Nombre
    FROM dbo.Comunas
    WHERE IdRegion = @IdRegion
    ORDER BY Nombre;
END;
GO

/* 4. Obtener una comuna */

IF OBJECT_ID(N'dbo.Comunas_ObtenerPorId', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.Comunas_ObtenerPorId;
END;
GO

CREATE PROCEDURE dbo.Comunas_ObtenerPorId
    @IdRegion INT,
    @IdComuna INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdComuna AS Id,
        IdRegion AS RegionId,
        Nombre
    FROM dbo.Comunas
    WHERE IdRegion = @IdRegion
      AND IdComuna = @IdComuna;
END;
GO

/* 5. Actualizar una comuna mediante MERGE */

IF OBJECT_ID(N'dbo.Comunas_Actualizar', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.Comunas_Actualizar;
END;
GO

CREATE PROCEDURE dbo.Comunas_Actualizar
    @IdRegion INT,
    @IdComuna INT,
    @Nombre NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre = LTRIM(RTRIM(@Nombre));

    IF @Nombre IS NULL OR @Nombre = N''
    BEGIN
        RAISERROR(
            N'El nombre de la comuna es obligatorio.',
            16,
            1);
        RETURN;
    END;

    MERGE INTO dbo.Comunas WITH (HOLDLOCK) AS Destino
    USING
    (
        SELECT
            @IdComuna AS IdComuna,
            @IdRegion AS IdRegion,
            @Nombre AS Nombre
    ) AS Origen
        ON Destino.IdComuna = Origen.IdComuna
       AND Destino.IdRegion = Origen.IdRegion
    WHEN MATCHED THEN
        UPDATE SET Nombre = Origen.Nombre
    OUTPUT
        inserted.IdComuna AS Id,
        inserted.IdRegion AS RegionId,
        inserted.Nombre;
END;
GO
