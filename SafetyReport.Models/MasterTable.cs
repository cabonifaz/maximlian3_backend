namespace SafetyReport.Models
{
    public class MasterTableItem
    {
        public int? IdEmpresa { get; set; }
        public int? IdMasterTable { get; set; }
        public int? IdMaster { get; set; }
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
        public int IdMaster { get; set; }
        public string? Descripcion { get; set; }
    }

    public class MasterTableRequest
    {
        public int IdMaster { get; set; }
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

    public class EditarMasterTableRequest
    {
        public int IdMaster { get; set; }
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

    public class MasterTableResultado
    {
        public int IdMasterTable { get; set; }
    }

    public class EliminarMasterTableRequest
    {
        public int IdMasterTable { get; set; }
    }

    public class FiltroMasterTableRequest
    {
        public int? IdMaster { get; set; }
    }
}