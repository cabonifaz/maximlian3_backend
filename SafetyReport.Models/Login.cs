using System;
using System.Collections.Generic;
using System.Text;

namespace SafetyReport.Models
{
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class LoginApiResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }

        public int? IdUsuario { get; set; }
        public string Usuario { get; set; }
        public int? IdEmpresa { get; set; }
    }
}
