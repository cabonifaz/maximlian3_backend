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
        public string? String4 { get; set; }
        public string? String5 { get; set; }
        public string? String6 { get; set; }
        public string? String7 { get; set; }
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
    }

    public class InventarioMaestroItem
    {
        public int IdInventario { get; set; }
        public int IdMaestro { get; set; }
        public string? Descripcion { get; set; }
        public string? Num1 { get; set; }
        public string? Num2 { get; set; }
        public string? Num3 { get; set; }
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }
        public string? Date1 { get; set; }
        public string? Date2 { get; set; }
        public string? Date3 { get; set; }
    }

    public class TablaMaestraRequest
    {
        public int IdMaestro { get; set; }
        public int? IdIdioma { get; set; }
        public string? InputText { get; set; }
        public string? InputText2 { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int? Num1 { get; set; }
        public decimal? Num2 { get; set; }
        public decimal? Num3 { get; set; }
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }
        public string? String4 { get; set; }
        public string? String5 { get; set; }
        public string? String6 { get; set; }
        public string? String7 { get; set; }
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
        public string? String4 { get; set; }
        public string? String5 { get; set; }
        public string? String6 { get; set; }
        public string? String7 { get; set; }
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

    public class TablaMaestraGroup
    {
        public int IdMaestro { get; set; }
        public List<TablaMaestraItem>? Items { get; set; }
    }

    public class FiltroTablaMaestraRequest
    {
        public string? idsMaestro { get; set; }  // comma-separated, e.g. "2,44,60"; null returns all
        public int? numPag { get; set; }
    }

    public class TablaMaestraListaResult
    {
        public List<TablaMaestraGroup> lstTablaMaestra { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class TablaMaestraCortaItem
    {
        public int Num1 { get; set; }
        public string? String1 { get; set; }
    }

    public class ObtenerTablaMaestraRequest
    {
        public int idMaestro { get; set; }
        public int? idBusqueda { get; set; }
        public string? vchBusqueda { get; set; }
    }
}
