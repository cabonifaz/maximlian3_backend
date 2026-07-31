namespace SafetyReport.Models
{
    public class ClienteTarifarioRequest
    {
        public int IdProducto { get; set; }
        public int IdTipoTramite { get; set; }
        public int IdPais { get; set; }
        public int IdMoneda { get; set; }
        public int DiasMax { get; set; }
        public int DiasMin { get; set; }
        public decimal Precio { get; set; }
        public decimal Penalidad { get; set; }
    }

    public class Cliente
    {
        public int IdTipoPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreCorto { get; set; }
        public int IdPais { get; set; }
        public int IdRegistroTributario { get; set; }
        public string? NumRegistroTributario { get; set; }
        public string? Correo { get; set; }
        public string? WebSite { get; set; }
        public string? Telefono { get; set; }
        public string? Fax { get; set; }
        public string? Direccion { get; set; }
        public string? Recomendacion { get; set; }
        public int IdEmpresaAtencion { get; set; }
        public int IdIdioma { get; set; }
        public string? LogoClienteUrl { get; set; }
        public bool ImprimeLogoSafety { get; set; }
        public List<int> LstIdFormatoDocumento { get; set; } = new();
        public int IdMoneda { get; set; }
        public int IdIdiomaFacturacion { get; set; }
        public bool AplicaPenalidad { get; set; }
        public int IdPlantilla { get; set; }
        public int IdEstado { get; set; }
        public bool EmitirPrefactura { get; set; }
        public List<ClienteContactoRequest> Contactos { get; set; } = new();
        public List<ClienteTarifarioRequest> Tarifario { get; set; } = new();
    }

    public class EditarCliente
    {
        public int IdCliente { get; set; }
        public int IdTipoPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreCorto { get; set; }
        public int IdPais { get; set; }
        public int IdRegistroTributario { get; set; }
        public string? NumRegistroTributario { get; set; }
        public string? Correo { get; set; }
        public string? WebSite { get; set; }
        public string? Telefono { get; set; }
        public string? Fax { get; set; }
        public string? Direccion { get; set; }
        public string? Recomendacion { get; set; }
        public int IdEmpresaAtencion { get; set; }
        public int IdIdioma { get; set; }
        public string? LogoClienteUrl { get; set; }
        public bool ImprimeLogoSafety { get; set; }
        public List<int> LstIdFormatoDocumento { get; set; } = new();
        public int IdMoneda { get; set; }
        public int IdIdiomaFacturacion { get; set; }
        public bool AplicaPenalidad { get; set; }
        public int IdPlantilla { get; set; }
        public int IdEstado { get; set; }
        public bool EmitirPrefactura { get; set; }
    }

    public class ClienteCreado
    {
        public int IdCliente { get; set; }
    }

    public class ClienteEliminado
    {
        public int IdCliente { get; set; }
    }


    public class ClienteConsulta
    {
        public int IdCliente { get; set; }
        public int IdEmpresa { get; set; }
        public int IdTipoPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? NombreCorto { get; set; }
        public int IdPais { get; set; }
        public int IdRegistroTributario { get; set; }
        public string? NumRegistroTributario { get; set; }
        public int? IdTipoDocumentoSunat { get; set; }
        public string? Correo { get; set; }
        public string? WebSite { get; set; }
        public string? Telefono { get; set; }
        public string? Fax { get; set; }
        public string? Direccion { get; set; }
        public string? Recomendacion { get; set; }
        public int IdEmpresaAtencion { get; set; }
        public int IdIdioma { get; set; }
        public string? LogoClienteUrl { get; set; }
        public bool ImprimeLogoSafety { get; set; }
        public List<int> LstIdFormatoDocumento { get; set; } = new();
        public int IdMoneda { get; set; }
        public int IdIdiomaFacturacion { get; set; }
        public bool AplicaPenalidad { get; set; }
        public int IdPlantilla { get; set; }
        public int IdEstado { get; set; }
        public bool EmitirPrefactura { get; set; }
    }

    public class ClienteListaResult
    {
        public List<ClienteListaConsulta> lstClientes { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class ClienteListaConsulta
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string Pais { get; set; } = string.Empty;
        public string TipoPersona { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class FiltroCliente
    {
        public int? numPag { get; set; }
        public string? busqueda { get; set; }
        public int? idPais { get; set; }
        public int? idEstado { get; set; }
    }

    public class ClienteIdRequest
    {
        public int idCliente { get; set; }
    }
    public class ClienteListaCortaItem
    {
        public int IdCliente { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public int IdIdioma { get; set; }
        public bool LogoImprimible { get; set; }
        public int IdPlantilla { get; set; }
    }

    public class ClienteListaCorta
    {
        public List<ClienteListaCortaItem> lstCliente { get; set; } = new();
    }

    public class FiltroClienteFacturacion
    {
        public int? numPag { get; set; }
        public string? busqueda { get; set; }
        public int? emitirPrefactura { get; set; }
        public int? idIdiomaFacturacion { get; set; }
        public int? estadoFacturacion { get; set; }
    }

    public class ClienteListaFacturacionResult
    {
        public List<ClienteListaFacturacionConsulta> lstClientes { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class ClienteListaFacturacionConsulta
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? EmitirPrefactura { get; set; }
        public int TotalPedidos { get; set; }
        public int PedidosFacturados { get; set; }
        public string? IdIdiomaFacturacion { get; set; }
        public string? EstadoFacturacion { get; set; }
    }

    public class FiltroClientePedidosFacturacion
    {
        public int idCliente { get; set; }
        public string? busqueda { get; set; }
        public int? numPag { get; set; }
    }

    public class ClientePedidosFacturacionResult
    {
        public List<ClientePedidoFacturacionConsulta> lstPedidos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class ClientePedidoFacturacionConsulta
    {
        public int IdPedido { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? Investigado { get; set; }
        public string? AplicaPenalidad { get; set; }
        public string EstadoFacturacion { get; set; } = string.Empty;
    }

    // Resultado de SP_Cliente_ObtenerParaFacturacion — exactamente los campos que necesita el payload
    // "cliente" de facturación (PedidoFacturaHandler.GuardarBorradorFacturaAsync).
    public class ClienteParaFacturacionConsulta
    {
        public int IdCliente { get; set; }
        public int IdTipoDocumentoSunat { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public int IdPais { get; set; }
    }

    public class ClienteResumen
    {
        public int TotalClientes { get; set; }
        public int TotalActivos { get; set; }
        public int TotalInactivos { get; set; }
        public decimal? PorcentajeActivos { get; set; }
        public decimal? PorcentajeCrecimiento { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}