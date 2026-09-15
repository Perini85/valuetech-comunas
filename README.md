# Valuetech · Regiones y comunas

Solución .NET 8 para consultar regiones y comunas y actualizar el nombre y la información adicional de una comuna desde un cliente MVC.

## Ejecutar

Requisitos: SDK .NET 8 (global.json fija 8.0.413), SQL Server 2012 o superior y, para los comandos siguientes, `sqlcmd`. La verificación local se realizó con SQL Server 2025 Express (17.0.1000.7), usando nivel de compatibilidad 110. No se probó sobre una instalación real de SQL Server 2012.

Desde la carpeta de la solución:

```powershell
dotnet restore Valuetech.Comunas.sln --configfile nuget.config
sqlcmd -S '.\SQLEXPRESS' -E -C -b -i database/001_schema.sql
sqlcmd -S '.\SQLEXPRESS' -E -C -b -i database/002_stored_procedures.sql
sqlcmd -S '.\SQLEXPRESS' -E -C -b -i database/003_seed.sql
dotnet build Valuetech.Comunas.sln --no-restore
```

También puedes ejecutar los scripts, en ese orden, desde SQL Server Management Studio. `001` crea la base si falta y agrega la columna XML a una base anterior. `002` reemplaza los procedimientos. `003` agrega datos de ejemplo faltantes, sin borrar ni renombrar datos existentes. Los identificadores de una base existente pueden diferir de los de una instalación nueva.

En dos terminales:

```powershell
dotnet run --project src/Valuetech.Api --launch-profile http
dotnet run --project src/Valuetech.WebMvc --launch-profile http
```

- MVC: http://localhost:5098
- Swagger: http://localhost:5198/swagger
- VS Code: configuración conjunta **Valuetech: API + Web MVC**.
- Visual Studio: abrir `Valuetech.Comunas.sln` y configurar API y WebMvc como proyectos de inicio.

## Configuración

`src/Valuetech.Api/appsettings.Development.json` configura la conexión local con autenticación integrada. `src/Valuetech.WebMvc/appsettings.Development.json` define `Api:BaseUrl`. Se pueden sobrescribir mediante variables de entorno:

```powershell
$env:ConnectionStrings__SqlServer = 'Server=.\SQLEXPRESS;Database=ValuetechComunas;Integrated Security=True;TrustServerCertificate=True;'
$env:Api__BaseUrl = 'http://localhost:5198/'
```

Fuera de Development hay que proporcionar ambos valores. `Api:BaseUrl` debe terminar en `/`. El cliente utiliza `HttpClientFactory` y un timeout de 15 segundos. No hay contraseñas en el repositorio. La confianza del certificado SQL y los perfiles HTTP son configuración local; un despliegue requiere HTTPS y certificados válidos.

## Endpoints

| Método | Ruta | Resultado |
| --- | --- | --- |
| GET | `/region` | Regiones ordenadas por nombre |
| GET | `/region/{id}` | Una región |
| GET | `/region/{regionId}/comuna` | Comunas de una región |
| GET | `/region/{regionId}/comuna/{id}` | Comuna perteneciente a esa región |
| POST | `/region/{regionId}/comuna` | Actualiza una comuna existente usando MERGE |

Primero consulta las regiones y sus comunas para obtener los identificadores. Ejemplo de cuerpo POST:

```json
{
  "id": 1,
  "nombre": "Valparaíso",
  "informacionAdicional": {
    "superficie": 4799.4,
    "poblacion": 247552,
    "densidad": 51.6
  }
}
```

Los números son de ejemplo. La densidad se ingresa explícitamente: no se recalcula a partir de la población y la superficie. El POST reemplaza nombre e información adicional; `informacionAdicional: null` (o su omisión) elimina los datos adicionales. Si se incluye el objeto, sus tres propiedades son obligatorias. El formulario permite completar las tres o dejar las tres vacías.

Validaciones: identificadores positivos, nombre obligatorio de hasta 100 caracteres, superficie positiva y población/densidad no negativas. Se recortan espacios exteriores del nombre. La base impide nombres duplicados dentro de una misma región. Una actualización no inserta nuevas comunas ni cambia su región.

Respuestas: `200`, `400` para datos inválidos, `404` para recursos inexistentes o comunas de otra región, `409` para duplicados y `500` para errores inesperados. Los errores usan `application/problem+json`, identificador de seguimiento y, en validaciones, errores por campo. Una región existente sin comunas devuelve `[]`; una región inexistente devuelve `404`.

## Arquitectura

Flujo de una operación:

```text
MVC → HttpClient → API Controller → MediatR → Handler
                                      ↓
                         Interfaz de repositorio (Application)
                                      ↓
                      Repositorio Dapper (Infrastructure)
                                      ↓
                       Procedimiento almacenado → SQL Server
```

- **Domain**: entidades e invariantes; sin dependencias externas.
- **Application**: consultas, comandos, handlers, DTO, interfaces de persistencia y FluentValidation. Behaviors de validación, logging y rendimiento.
- **Infrastructure**: conexiones SQL, Dapper, procedimientos y conversión XML. Propaga cancelación y traduce violaciones de unicidad a conflictos.
- **Api**: rutas, contratos JSON, Swagger y manejo global de excepciones.
- **WebMvc**: vistas Razor y modelos propios. Consume exclusivamente HTTP; no referencia los proyectos de API, Application o Infrastructure.
- **Tests**: pruebas unitarias de casos de uso, validación, dominio y cliente MVC; integración HTTP y SQL real.

No se agregó un servicio intermedio que solo delegue llamadas: cada handler representa el caso de uso. Todo acceso de datos de la aplicación utiliza procedimientos almacenados. Los scripts de instalación y la preparación/verificación de pruebas sí usan SQL directo.

`InformacionAdicional` es una columna SQL `xml`, convertida a objetos JSON al cruzar la API. Su formato es el sugerido en la prueba:

```xml
<Info>
  <Superficie>4799.4</Superficie>
  <Poblacion Densidad="51.6">247552</Poblacion>
</Info>
```

La actualización usa `MERGE ... WITH (HOLDLOCK)`, con coincidencia por comuna y región y solamente `WHEN MATCHED`. El nombre y el XML se guardan en la misma sentencia. La concurrencia sigue la regla de última escritura: no hay control de versiones del formulario.

## Verificación

Solo pruebas unitarias, sin SQL Server:

```powershell
dotnet test tests/Valuetech.UnitTests --no-restore
```

Suite completa con SQL Server:

```powershell
$env:VALUETECH_TEST_SQLSERVER = 'Server=.\SQLEXPRESS;Database=master;Integrated Security=True;TrustServerCertificate=True;'
dotnet test Valuetech.Comunas.sln --no-restore
```

La cuenta de pruebas necesita permiso para crear y eliminar bases. La fixture crea una base `ValuetechTests_<GUID>`, instala esquema/procedimientos/seed, ejecuta la API contra ella y elimina únicamente esa base al terminar. No modifica `ValuetechComunas`. Si la variable no está configurada, las pruebas SQL se marcan como omitidas. Una interrupción forzada del proceso puede impedir la limpieza de la base temporal.

Cobertura: consultas, pertenencia a región, normalización, cancelación propagada, validación antes de acceder al repositorio, XML y decimales, información opcional, ausencia de inserciones, conflictos de unicidad, escrituras concurrentes, JSON mal formado y errores internos sin detalles sensibles.

También se verificó manualmente el flujo MVC: regiones → comunas → editar → guardar → volver a consultar, validación de información incompleta y rechazo `400` de POST sin token antifalsificación.

## Alcance de seguridad

El formulario usa antifalsificación, Razor codifica la salida y la acción de edición limita los campos enlazados. SQL utiliza parámetros; los errores inesperados se registran en servidor y no exponen detalles de conexión al cliente. Esta entrega de demostración tiene endpoints anónimos: autenticación y autorización no forman parte de la implementación. No incluye altas/bajas de regiones o comunas porque los requisitos solicitan consulta y actualización.

Referencia técnica: [tipo XML de SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/xml/xml-data-type-and-columns-sql-server).
