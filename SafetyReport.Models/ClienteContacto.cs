namespace SafetyReport.Models
{
    public class ClienteContactoRequest
    {
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set; }
        public int IdTipoContacto { get; set; }
        public string? TipoContacto { get; set; }
        public int AreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool EnviarCorreo { get; set; }
    }

    public class ClienteContactoCrear
    {
        public int IdCliente { get; set; }
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set;}
        public int IdTipoContacto { get; set; }
        public string? TipoContacto { get; set; }
        public int IdAreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool EnviarCorreo { get; set; }
    }

    public class ClienteContactoEditar
    {
        public int IdClienteContacto { get; set; }
        public int IdCliente { get; set; }
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set;}
        public int IdTipoContacto { get; set; }
        public int IdAreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool EnviarCorreo { get; set; }
    }

    public class ClienteContactoSeleccionado
    {
        public int IdClienteContacto { get; set; }
        public int IdCliente { get; set; }
        public string? Codigo { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public int IdTipoPersonaContacto { get; set; }
        public int IdTipoContacto { get; set; }
        public int IdAreaTrabajo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool EnviarCorreo { get; set; }
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
        public int idClienteContacto { get; set; }
        public int idCliente { get; set; }
    }

    public class ClienteContactoFiltro
    {
        public int idCliente { get; set; }
        public string? busqueda { get; set; }
        public int? numPag { get; set; }
    }

    public class ClienteContactoListaResult
    {
        public List<ClienteContactoListaDetalleResult> lstClienteContactos { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }


    public class ClienteContactoListaDetalleResult
    {
        public int IdClienteContacto { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string TipoContacto { get; set; } = string.Empty;
        public string AreaTrabajo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool EnviarCorreo { get; set; }
    }
}