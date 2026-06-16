namespace SafetyReport.Models
{
    public class PlantillaDocumento
    {
        public int IdPlantillaDocumento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Formato { get; set; } = string.Empty;
        public string Estructura { get; set; } = string.Empty;
        public List<string> Imagenes { get; set; } = new();
    }

    public class FiltroGenerarDocumento
    {
        public int IdPedido { get; set; }
    }
}
