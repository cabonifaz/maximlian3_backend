namespace SafetyReport.Application.Comun
{
    public class Respuesta
    {
        public int IdTipoMensaje { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public object? Result { get; set; }
    }
}
