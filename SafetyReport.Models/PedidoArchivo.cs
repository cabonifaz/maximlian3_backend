namespace SafetyReport.Models
{
    public class PedidoArchivoCrear
    {
        public int IdPedido { get; set; }
        public string DocumentoURL { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string FormatoDocumento { get; set; } = string.Empty;
    }

    public class PedidoArchivoEditar
    {
        public int IdPedidoArchivo { get; set; }
        public int IdPedido { get; set; }
        public string DocumentoURL { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string FormatoDocumento { get; set; } = string.Empty;
        public int IdEstado { get; set; }
    }

    public class PedidoArchivoConsulta
    {
        public int IdPedidoArchivo { get; set; }
        public int IdPedido { get; set; }
        public string DocumentoURL { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public int IdFormato { get; set; }
        public int IdEstado { get; set; }
    }

    public class PedidoArchivoCreado
    {
        public int IdPedidoArchivo { get; set; }
    }

    public class PedidoArchivoEliminado
    {
        public int IdPedidoArchivo { get; set; }
    }

    public class PedidoArchivoIdRequest
    {
        public int IdPedidoArchivo { get; set; }
        public int IdPedido { get; set; }
    }

    public class FiltroPedidoArchivo
    {
        public int IdPedido { get; set; }
        public string? Busqueda { get; set; }
        public int? IdEstado { get; set; }
        public int? NumPag { get; set; }
    }

    public class PedidoArchivoListaConsulta
    {
        public int IdPedidoArchivo { get; set; }
        public int IdPedido { get; set; }
        public string DocumentoURL { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public int IdFormato { get; set; }
        public int IdEstado { get; set; }
    }

    public class PedidoArchivoListaResult
    {
        public List<PedidoArchivoListaConsulta> lstPedidoArchivo { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}