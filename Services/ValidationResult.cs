namespace OllinLexer.Services;

public sealed record ValidationStep(int Paso, string Caracter, string Tipo, string EstadoAnterior, string EstadoNuevo);

public sealed record ValidationResult(
    string Cadena,
    bool Valida,
    string Categoria,
    string Token,
    string Mensaje,
    string Recorrido,
    int? PosicionError,
    IReadOnlyList<ValidationStep> Pasos);

public sealed record TokenAnalysis(
    int Numero,
    int Linea,
    int Columna,
    string Lexema,
    bool Valido,
    string Categoria,
    string Codigo,
    string Recorrido,
    IReadOnlyList<ValidationStep> Pasos);

public sealed record SymbolEntry(
    string Identificador,
    string Nombre,
    string TipoDato,
    string Valor,
    int LineaDeclaracion);

public sealed record LexicalError(
    int Linea,
    int Columna,
    string Lexema,
    string Descripcion);

public sealed record AnalysisResult(
    string Fuente,
    bool Valido,
    string Mensaje,
    IReadOnlyList<TokenAnalysis> Tokens,
    IReadOnlyList<SymbolEntry> Simbolos,
    IReadOnlyList<LexicalError> Errores)
{
    public int TotalPasos => Tokens.Sum(token => token.Pasos.Count);
}
