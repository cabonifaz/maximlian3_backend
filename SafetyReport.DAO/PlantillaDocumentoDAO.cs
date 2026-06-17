using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class PlantillaDocumentoDAO
    {
        private readonly DbConfig _dbConfig;

        public PlantillaDocumentoDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<PlantillaDocumento?> ObtenerPorIdAsync(int idPlantilla)
        {
            using SqlConnection cn = new(_dbConfig.ConnectionString);
            using SqlCommand cmd = new("PlantillaDocumento_Obtener", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@intIdPlantillaDocumento", SqlDbType.Int).Value = idPlantilla;
            await cn.OpenAsync();

            using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new PlantillaDocumento
            {
                Html       = dr["Html"]?.ToString() ?? string.Empty,
                Imagenes   = ParsearImagenes(dr["Imagenes"]?.ToString())
            };
        }

        private static List<string> ParsearImagenes(string? imagenesJson)
        {
            if (string.IsNullOrWhiteSpace(imagenesJson))
                return new();

            using var document = JsonDocument.Parse(imagenesJson);
            if (document.RootElement.ValueKind == JsonValueKind.Array)
                return document.RootElement
                    .EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetString() ?? string.Empty)
                    .ToList();

            if (document.RootElement.ValueKind == JsonValueKind.Object)
                return document.RootElement
                    .EnumerateObject()
                    .Where(prop => prop.Value.ValueKind == JsonValueKind.String)
                    .Select(prop => prop.Value.GetString() ?? string.Empty)
                    .ToList();

            return new();
        }
    }
}
