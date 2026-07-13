namespace SafetyReport.Models
{
    public class BancoCrear
    {
        public int IdPais { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class BancoEditar
    {
        public int IdBanco { get; set; }
        public int IdPais { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class BancoCreado
    {
        public int IdBanco { get; set; }
    }

    public class BancoEliminado
    {
        public int IdBanco { get; set; }
    }

    public class BancoIdRequest
    {
        public int IdBanco { get; set; }
    }

    public class BancoObtenerRequest
    {
        public int? IdBanco { get; set; }
        public string? Nombre { get; set; }
    }

    public class BancoConsulta
    {
        public int IdBanco { get; set; }
        public int IdPais { get; set; }
        public string? Pais { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class FiltroBanco
    {
        public string? Busqueda { get; set; }
        public int NumPag { get; set; } = 1;
    }

    public class BancoListaResult
    {
        public List<BancoConsulta> lstBancos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class BancoMatchItem
    {
        public string? Nombre { get; set; }
        public string? Pais { get; set; }
    }

    public class BancoMatchResultItem
    {
        public int? IdBanco { get; set; }
    }
}
