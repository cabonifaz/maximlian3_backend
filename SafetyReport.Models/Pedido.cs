namespace SafetyReport.Models
{
    public class PedidoArchivoRequest
    {
        public string DocumentoURL { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string FormatoDocumento { get; set; } = string.Empty;
    }

    public class Pedido
    {
        public string Codigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? RUC { get; set; }
        public string? RazonSocial { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string InvestigarRazonSocialNombres { get; set; } = string.Empty;
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
        public List<PedidoArchivoRequest> Archivos { get; set; } = new();
    }

    public class EditarPedido
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? RUC { get; set; }
        public string? RazonSocial { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string InvestigarRazonSocialNombres { get; set; } = string.Empty;
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
    }

    public class PedidoConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? RUC { get; set; }
        public string? RazonSocial { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public string? InvestigarRazonSocialNombres { get; set; }
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public string? Comentario { get; set; }
        public int IdEstado { get; set; }
    }

    public class PedidoCreado
    {
        public int IdPedido { get; set; }
    }

    public class PedidoEliminado
    {
        public int IdPedido { get; set; }
    }

    public class PedidoIdRequest
    {
        public int IdPedido { get; set; }
    }

    public class FiltroPedido
    {
        public string? Busqueda { get; set; }
        public int? IdCliente { get; set; }
        public int? IdEstado { get; set; }
        public int? NumPag { get; set; }
    }

    public class PedidoListaConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string? RUC { get; set; }
        public string? RazonSocial { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdCompania { get; set; }
        public int IdTarifario { get; set; }
        public int IdPlantilla { get; set; }
        public int IdIdioma { get; set; }
        public int IdClaseInforme { get; set; }
        public string? NumReferencia { get; set; }
        public decimal? MontoCredito { get; set; }
        public int? PlazoCredito { get; set; }
        public DateTime? FchDesde { get; set; }
        public DateTime? FchHasta { get; set; }
        public int IdEstado { get; set; }
    }

    public class PedidoListaResult
    {
        public List<PedidoListaConsulta> lstPedido { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}