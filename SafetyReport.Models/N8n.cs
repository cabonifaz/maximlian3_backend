namespace SafetyReport.Models
{
    public class N8nClienteConsulta
    {
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class N8nClienteFiltro
    {
        public string? emailBusqueda { get; set; }
    }
}
