# Analizador léxico OLLIN

Aplicación web ASP.NET Core para **.NET 7** que carga en una base SQL las transiciones del diagrama `q0–q239`, separa un programa fuente en múltiples tokens y recorre cada lexema como AFND.

La interfaz genera:

- Archivo de tokens con el código reconocido para cada lexema.
- Tabla de recorrido detallada, reiniciando cada token en `q0`.
- Tabla de símbolos con identificadores únicos (`ID01`, `ID02`, etc.).
- Lista de errores con línea y columna, sin detener el análisis posterior.

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

Por ejemplo, `VAR` se clasifica como palabra reservada, mientras que `VARIABLE` se clasifica como identificador y se agrega a la tabla de símbolos. El autómata actual no incluye constantes numéricas, por lo que un lexema como `10` se reporta como error léxico.

Los identificadores pueden usar letras mayúsculas o minúsculas y guion bajo. También se aceptan identificadores de un solo carácter (`A` o `_`); pueden contener dígitos, pero no terminar con uno.
