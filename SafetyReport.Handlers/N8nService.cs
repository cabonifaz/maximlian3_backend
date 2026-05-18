using Microsoft.IdentityModel.Tokens;
using SafetyReport.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SafetyReport.Handlers
{
    public class N8nService
    {
        private readonly N8nConfig _config;
        private readonly HttpClient _httpClient;

        public N8nService(N8nConfig config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        private string GenerarToken()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> PostAsync(string webhookUrl, object payload)
        {
            var token = GenerarToken();
            var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
