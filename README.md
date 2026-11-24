# Analizador léxico OLLIN

Aplicación web ASP.NET Core para **.NET 7** que carga en una base SQL las transiciones del diagrama `q0–q239`, recorre cadenas como AFND y muestra cada paso.

## Ejecutar

1. Abre `OllinLexer.sln` en Visual Studio 2022 con soporte para .NET 7.
2. Ejecuta `dotnet run`.
3. Abre la dirección que muestre la terminal.

La configuración predeterminada usa SQLite y crea automáticamente `ollin-automata.db`.

## Usar SQL Server LocalDB

1. Instala **SQL Server Express LocalDB** desde Visual Studio Installer.
2. En `appsettings.json`, cambia `DatabaseProvider` de `Sqlite` a `SqlServerLocalDb`.
3. Ejecuta nuevamente. Entity Framework creará `OllinAutomata` y sembrará sus tablas.

## Tablas

- `Estados`: q0–q239, tipo y categoría final.
- `Simbolos`: entradas como `A–Z`, `LET`, `DIG`, `_`, `FDC` y operadores.
- `Transiciones`: estado origen, símbolo y estado destino.

La semilla proviene de `Data/Seed/transiciones.csv`. El programa conserva simultáneamente la ruta de palabra reservada y la de identificador; si ambas aceptan, la palabra reservada tiene prioridad.
