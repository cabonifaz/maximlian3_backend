namespace SafetyReport.Models
{
    public class PlantillaDocumento
    {
        public string Html { get; set; } = string.Empty;
        public List<string> Imagenes { get; set; } = new();
    }

    public class FiltroGenerarDocumento
    {
        public int IdPedido { get; set; }
    }
}
