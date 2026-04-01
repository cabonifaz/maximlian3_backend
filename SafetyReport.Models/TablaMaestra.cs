namespace SafetyReport.Models
{
    public class TablaMaestraItem
    {
        public int? IdEmpresa { get; set; }
        public int? IdTablaMaestra { get; set; }
        public int? IdMaestro { get; set; }
        public string? Descripcion { get; set; }
        public int? Num1 { get; set; }
        public decimal? Num2 { get; set; }
        public decimal? Num3 { get; set; }
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
    }

    public class InventarioMaestroItem
    {
        public int IdEmpresa { get; set; }
        public int IdMaestro { get; set; }
        public string? Descripcion { get; set; }
    }

    public class TablaMaestraRequest
    {
        public int IdMaestro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? Num1 { get; set; }
        public decimal? Num2 { get; set; }
        public decimal? Num3 { get; set; }
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
    }

    public class EditarTablaMaestraRequest
    {
        public int IdMaestro { get; set; }
        public int? Num1 { get; set; }
        public decimal? Num2 { get; set; }
        public decimal? Num3 { get; set; }
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
    }

    public class TablaMaestraResultado
    {
        public int IdTablaMaestra { get; set; }
    }

    public class EliminarTablaMaestraRequest
    {
        public int IdTablaMaestra { get; set; }
    }

    public class FiltroTablaMaestraRequest
    {
        public int? IdMaestro { get; set; }
    }

    public class ObtenerTablaMaestraRequest
    {
        public int IdMaestro { get; set; }
        public int? IdBusqueda { get; set; }
        public string? VchBusqueda { get; set; }
    }
}