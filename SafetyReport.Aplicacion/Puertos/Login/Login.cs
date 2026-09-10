namespace SafetyReport.Application.Puertos.Login
{
    public class LoginRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LoginApiResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        public int? IdUsuario { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int? IdEmpresa { get; set; }
    }
}
