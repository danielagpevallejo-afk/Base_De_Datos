namespace OllinLexer.Models;

public sealed class Transicion
{
    public int Id { get; set; }
    public string EstadoOrigenId { get; set; } = "";
    public Estado EstadoOrigen { get; set; } = null!;
    public int SimboloId { get; set; }
    public Simbolo Simbolo { get; set; } = null!;
    public string EstadoDestinoId { get; set; } = "";
    public Estado EstadoDestino { get; set; } = null!;
}
