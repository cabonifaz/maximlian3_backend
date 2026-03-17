namespace SafetyReport.Models
{
    public class ClienteContactoRequest
    {
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set; }
        public int IdTipoContacto { get; set; }
        public int AreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class ClienteContactoCrear
    {
        public int IdCliente { get; set; }
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set;}
        public int IdTipoContacto { get; set; }
        public int IdAreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class ClienteContactoEditar
    {
        public int IdClienteContacto { get; set; }
        public int IdCliente { get; set; }
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set;}
        public int IdTipoContacto { get; set; }
        public int AreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class ClienteContactoSeleccionado
    {
        public int IdClienteContacto { get; set; }
        public int IdCliente { get; set; }
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set; }
        public int IdTipoContacto { get; set; }
        public int AreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
    }

    public class ClienteContactoCreado
    {
        public int IdClienteContacto { get; set; }
    }

    public class ClienteContactoEliminado
    {
        public int IdClienteContacto { get; set; }
    }

    public class ClienteContactoIdRequest
    {
        public int IdClienteContacto { get; set; }
        public int IdCliente { get; set; }
    }

    public class ClienteContactoFiltro
    {
        public int IdCliente { get; set; }
        public int? NumPag { get; set; }
    }

    public class ClienteContactoListaResult
    {
        public List<ClienteContactoConsulta> lstClienteContactos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}