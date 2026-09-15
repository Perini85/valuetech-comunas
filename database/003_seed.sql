USE [ValuetechComunas];
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Regiones
        WHERE Nombre = N'Región de Valparaíso')
    BEGIN
        INSERT INTO dbo.Regiones (Nombre)
        VALUES (N'Región de Valparaíso');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Regiones
        WHERE Nombre = N'Región Metropolitana de Santiago')
    BEGIN
        INSERT INTO dbo.Regiones (Nombre)
        VALUES (N'Región Metropolitana de Santiago');
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Regiones
        WHERE Nombre = N'Región del Biobío')
    BEGIN
        INSERT INTO dbo.Regiones (Nombre)
        VALUES (N'Región del Biobío');
    END;

    DECLARE @IdValparaiso INT =
        (SELECT IdRegion FROM dbo.Regiones
         WHERE Nombre = N'Región de Valparaíso');

    DECLARE @IdMetropolitana INT =
        (SELECT IdRegion FROM dbo.Regiones
         WHERE Nombre = N'Región Metropolitana de Santiago');

    DECLARE @IdBiobio INT =
        (SELECT IdRegion FROM dbo.Regiones
         WHERE Nombre = N'Región del Biobío');

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdValparaiso
          AND Nombre = N'Valparaíso')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdValparaiso, N'Valparaíso');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdValparaiso
          AND Nombre = N'Viña del Mar')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdValparaiso, N'Viña del Mar');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdMetropolitana
          AND Nombre = N'Santiago')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdMetropolitana, N'Santiago');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdMetropolitana
          AND Nombre = N'Providencia')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdMetropolitana, N'Providencia');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdMetropolitana
          AND Nombre = N'Las Condes')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdMetropolitana, N'Las Condes');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdBiobio
          AND Nombre = N'Concepción')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdBiobio, N'Concepción');
    END;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Comunas
        WHERE IdRegion = @IdBiobio
          AND Nombre = N'Talcahuano')
    BEGIN
        INSERT INTO dbo.Comunas (IdRegion, Nombre)
        VALUES (@IdBiobio, N'Talcahuano');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
