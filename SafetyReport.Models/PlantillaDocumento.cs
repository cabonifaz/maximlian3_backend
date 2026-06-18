namespace SafetyReport.Models
{
    public class PlantillaDocumento
    {
        public string Contenido { get; set; } = string.Empty;
        public List<string> Imagenes { get; set; } = new();
    }

    public class FiltroGenerarDocumento
    {
        public int IdPedido { get; set; }
        public int IdIdioma { get; set; }
    }
}
