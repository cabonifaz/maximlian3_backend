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
        public List<int> Idiomas { get; set; }
    }

    public class InfoUsuarioEditar
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public List<int> Roles { get; set; }
        public List<int> Idiomas { get; set; }
    }

    public class UsuarioCreado
    {
        public int IdUsuario { get; set; }
        public string Username { get; set; }
    }

    public class UsuarioConsulta
    {
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public List<int> Roles { get; set; } = new();
        public List<int> Idiomas { get; set; } = new();
    }



    public class UsuarioListaResult
    {
        public List<UsuarioListaConsulta> lstUsuarios { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class UsuarioListaConsulta
    {
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Roles { get; set; }
        public string Estado { get; set; }
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

    public class EliminarUsuarioResult
    {
        public int IdUsuarioEliminar { get; set; }
        public string Username { get; set; }
    }

    public class FiltroUsuario
    {
        public int numPag { get; set; }
        public string? Filtro { get; set; }
    }
}