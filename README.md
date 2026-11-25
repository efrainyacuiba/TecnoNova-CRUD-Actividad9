# TecnoNova · Actividad 10

Proyecto continuado a partir del ZIP de la Actividad 9. Conserva catálogo, CRUD de productos, ventas maestro-detalle, historial, descuentos CR-01 y auditoría. Se añade Reportes de ventas, vista imprimible y verificación SQL independiente. No usa Docker.

## 1. Ejecutar (Windows o Ubuntu)

Abre directamente la carpeta que contiene `TiendaProyecto.csproj` en VS Code. En la terminal:

```sh
dotnet run
```

Abre `http://localhost:5080` (o el URL que muestre la terminal). El proyecto requiere **SDK .NET 10**, como el proyecto original. Si solo tienes el SDK 8 o 9, debes instalar .NET 10. En la primera ejecución se necesita internet para restaurar NuGet y descargar PostgreSQL; ejecuta como usuario normal, no como administrador/root. `MysticMind.PostgresEmbed` inicia un PostgreSQL 17 real local, sin Docker y sin instalar un servicio manualmente. Si Windows informa que faltan bibliotecas de Visual C++, instala el redistribuible x64 oficial de Microsoft. Los binarios y datos se mantienen localmente; no se incluyen en el ZIP ni en Git. La base está destinada a demostración local, no producción (autenticación de confianza en localhost).

El programa genera las tablas `productos`, `ventas`, `detalle_ventas`, conserva el esquema CR-01 y carga cinco ventas **ficticias** (dos en octubre y tres el 24 de noviembre de 2026), una sola vez por base integrada. También hay seis productos del catálogo anterior. No se sobrescriben las ventas registradas. Cada venta demo tiene cabecera y detalle, descuento coherente y ajuste de stock. Los datos de tu instancia de la Actividad 9 no están en el ZIP original: si deseas consultarlos, configura el modo externo descrito abajo.

## 2. Evidencias reales

Entra a Reportes desde el menú. En el campo «Generado por» puedes escribir tu nombre y pulsar Generar reporte; los botones de prueba conservan el nombre que se está mostrando en el reporte. La Actividad 9 no implementaba login: este nombre es declarado, no una identidad autenticada.

| Captura | Acción | Resultado esperado en una BD integrada recién creada |
|---|---|---|
| A1 | Prueba A · Diario | 24/11/2026, 3 operaciones, bruto Bs 1.200, descuento Bs 70, total Bs 1.130 |
| A2 | Validar total con SQL | Resultado real de COUNT y SUM: 3 operaciones y Bs 1.130 |
| B | Prueba B · Mensual | Octubre: 2 ventas, Bs 290; noviembre: 3 ventas, Bs 1.130; gran total Bs 1.420 |
| C | Prueba C · Sin datos | Rango de 2020: mensaje amigable sin tabla vacía |

Para la evidencia A2 es preferible mostrar también la terminal de VS Code. Mantén la primera terminal ejecutando la web y abre una segunda en la misma carpeta:

```sh
dotnet run --no-build -- --verificar-sql 2026-11-24 2026-11-24
```

El comando usa la misma instancia local y ejecuta otra consulta real `SELECT COUNT, SUM...` sobre PostgreSQL, sin utilizar las cifras de la vista. Si tienes psql/pgAdmin, también puedes ejecutar `sql/04_validacion_actividad10.sql` conectado a la misma BD. Si agregaste más ventas en el periodo, los montos de ejemplo cambiarán: verifica la igualdad de los resultados reales, no elimines datos para forzar una cifra.

En cada captura deben verse el rango, la granularidad y los totales o aviso correspondiente. Si necesitas guardar el reporte operativo en PDF, pulsa Imprimir / Guardar PDF; usa A4 horizontal y desactiva encabezados/pies del navegador. La aplicación calcula la numeración de páginas lógicas. Para capturar la pantalla en Windows usa Win+Shift+S; en Ubuntu usa la herramienta de captura del sistema.

## 3. Documento final de Moodle

Abre Reportes > Informe técnico, o directamente `http://localhost:5080/actividad10/informe.html`. El informe ya contiene los bosquejos, las consultas reales y el código del DAO/controlador. Completa la carátula con todos los integrantes y el enlace al último commit. Carga cuatro imágenes (A1, A2, B, C). Pulsa Guardar informe como PDF, elige Guardar como PDF en el diálogo, A4 vertical y desactiva encabezados/pies del navegador. Revisa que se vean las capturas y los datos antes de subirlo a Moodle. El informe se edita en el navegador y los archivos elegidos no se envían al servidor; si recargas la página deberás cargar las capturas otra vez. No se incluyen capturas inventadas.

Los bosquejos editables de Draw.io están en `docs/Bosquejo_Actividad10.drawio`; PNG en `docs/bosquejo_diario.png` y `docs/bosquejo_mensual.png`. Puedes abrir el `.drawio` desde diagrams.net, pero no necesitas hacerlo para el informe.

## 4. GitHub

Si ya tienes el repositorio del equipo, actualiza ese repositorio con el módulo en vez de crear uno distinto. Si trabajas con Git, copia el código a tu clon y confirma las diferencias. Desde la raíz del repositorio:

```sh
git status
git add .
git commit -m "Actividad 10: reportes diarios y mensuales de ventas"
git push
```

Si no tienes Git instalado, abre el repositorio en github.com, usa Add file > Upload files, arrastra los archivos/carpetas fuente conservando su estructura, revisa el listado y confirma el cambio. No subas `pg_embed`, `bin`, `obj`, `logs`, archivos de credenciales ni el ZIP como sustituto del código. Puedes usar una rama y pedir al líder que integre los cambios. Después abre Commits, selecciona el commit de Actividad 10 y copia su URL completa al informe.

## 5. Estructura

- `Data/ReporteDAO.cs`: consultas parametrizadas Npgsql, conversión de DTOs, errores orientadores y SQL de verificación.
- `Controllers/ReportesController.cs`: filtros, validación, selección de modo y respuesta MVC.
- `Models/ReporteVentas.cs`: totales decimales, agrupación y paginación lógica.
- `Views/Reportes/Ventas.cshtml` y `wwwroot/css/reporte.css`: vista e impresión.
- `Views/Reportes/ValidarSql.cshtml`: consulta SQL ejecutada y resultado independiente.
- `Data/Sql/03_a10_demo.sql`: datos reproducibles idempotentes para las pruebas.
- `sql/04_validacion_actividad10.sql`: consultas manuales de comprobación.
- `tests/TiendaProyecto.Tests`: pruebas de actividades previas y tres pruebas del modelo del reporte.

Pruebas automatizadas opcionales, desde la raíz:

```sh
dotnet test tests/TiendaProyecto.Tests/TiendaProyecto.Tests.csproj
```

## 6. Utilizar tu PostgreSQL anterior (opcional)

Por defecto se usa una instancia local independiente. Si ya tienes PostgreSQL instalado y quieres conservar los datos de la Actividad 9, copia `appsettings.Development.example.json` a `appsettings.Development.json`, modifica el modo y la conexión real. El ejemplo debe quedar así, sin publicar la contraseña:

```json
{
  "BaseDatos": { "Modo": "Externa" },
  "ConnectionStrings": { "Tienda": "Host=localhost;Port=5432;Database=tecnonova;Username=tecnonova;Password=TU_CLAVE;Timeout=5" }
}
```

La BD y el usuario deben existir. El modo externo conserva los datos y no inserta las cinco ventas de A10 automáticamente. Si necesitas datos de prueba, usa una BD de demostración y ejecuta `sql/03_a10_demo.sql` explícitamente (no hacerlo sobre producción). Para volver a la instancia automática, elimina esa configuración local o cambia Modo a Integrada.

## 7. Si falla

- `dotnet --version`: debe mostrar un SDK 10.x. Verifica que abriste la carpeta que tiene el `.csproj`.
- Si NuGet o Maven no responden, revisa la conexión a internet y ejecuta de nuevo `dotnet run`. No ejecutes como administrador/root.
- Si el puerto web está ocupado, detén la otra instancia con Ctrl+C o ejecuta `dotnet run --urls http://localhost:5081`.
- Si aparece un error de PostgreSQL, mira la terminal. El archivo técnico de errores queda bajo `bin/Debug/net10.0/logs/errores_sistema.log`.
- Si no hay ventas por defecto, usa los botones de prueba (las fechas demo son fijas de la consigna, no la fecha actual del equipo).

Dependencia del PostgreSQL integrado: https://www.nuget.org/packages/MysticMind.PostgresEmbed/5.3.0 y https://mysticmind.github.io/mysticmind-postgresembed/guide/named-instances

Nota de entrega: el código se preparó sobre el proyecto original y se revisó estáticamente. El entorno de generación no dispone de SDK .NET ni servidor PostgreSQL ni acceso a NuGet para compilar/ejecutar. Las pruebas reales y el commit deben completarse en el equipo del estudiante. Los importes listados son resultados esperados calculados de los datos demo, no resultados observados.
