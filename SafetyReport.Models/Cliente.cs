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
        public int IdCliente { get; set; }
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
}