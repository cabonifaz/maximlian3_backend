using System;
using System.Collections.Generic;
using System.Text;

namespace SafetyReport.Models
{
    public class Roles
    {
        public int IdRol { get; set; }
        public string Rol { get; set; }
        public string Descripcion { get; set; }
    }

    public class UsuarioGeneral
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }
        public int IdEmpresa { get; set; }
        public int IdRol { get; set; }
    }

    public class Usuario
    {
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Email { get; set; }
        public List<int> Roles { get; set; }
    }

    public class EditarUsuario
    {
        public int IdUsuario { get; set; }
        public Usuario InfoUsuario { get; set; }
    }

    public class UsuarioCreado
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }
    }

    public class UsuarioConsulta
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public List<Roles> Roles { get; set; } = new();
    }

    public class UsuarioLoginResponse
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public List<Roles> Roles { get; set; } = new();
    }

    public class EliminarUsuario
    {
        public int IdUsuarioEliminar { get; set; }
    }

    public class FiltroUsuario
    {
        public string? Filtro { get; set; }
    }
}