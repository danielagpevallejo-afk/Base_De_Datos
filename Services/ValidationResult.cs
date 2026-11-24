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
