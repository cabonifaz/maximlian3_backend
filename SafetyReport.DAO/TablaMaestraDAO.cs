using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class TablaMaestraDAO
    {
        private readonly DbConfig _dbConfig;

        public TablaMaestraDAO(DbConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        private static async Task<Respuesta> LeerRespuestaAsync<T>(SqlCommand cmd)
        {
            using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                var respuesta = new Respuesta
                {
                    IdTipoMensaje = dr["IdTipoMensaje"] != DBNull.Value
                        ? Convert.ToInt32(dr["IdTipoMensaje"])
                        : 3,
                    Mensaje = dr["Mensaje"]?.ToString() ?? string.Empty
                };

                var json = dr["Result"]?.ToString();

                respuesta.Result = !string.IsNullOrWhiteSpace(json)
                    ? JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<T>()
                    : new List<T>();

                return respuesta;
            }

            return new Respuesta
            {
                IdTipoMensaje = 3,
                Mensaje = "No se obtuvo respuesta del procedimiento.",
                Result = new List<T>()
            };
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, string? idsMaestro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("TablaMaestra_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@vchIdsMaestro", SqlDbType.VarChar, -1).Value = (object?)idsMaestro ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TablaMaestraGroup>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraGroup>()
                };
            }
        }

        public async Task<Respuesta> ListarInventarioAsync(UsuarioGeneral usuarioLogueado, int? idMaestro)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InventarioMaestros_Listar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = (object?)idMaestro ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<InventarioMaestroItem>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, TablaMaestraRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("TablaMaestra_Insertar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = request.IdMaestro;
                cmd.Parameters.Add("@vchDescripcion", SqlDbType.VarChar, 255).Value = request.Descripcion;
                cmd.Parameters.Add("@intNum1", SqlDbType.Int).Value = (object?)request.Num1 ?? DBNull.Value;

                cmd.Parameters.Add("@decNum2", SqlDbType.Decimal).Value = (object?)request.Num2 ?? DBNull.Value;
                cmd.Parameters["@decNum2"].Precision = 18;
                cmd.Parameters["@decNum2"].Scale = 6;

                cmd.Parameters.Add("@decNum3", SqlDbType.Decimal).Value = (object?)request.Num3 ?? DBNull.Value;
                cmd.Parameters["@decNum3"].Precision = 18;
                cmd.Parameters["@decNum3"].Scale = 6;

                cmd.Parameters.Add("@vchString1", SqlDbType.VarChar, 255).Value = (object?)request.String1 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString2", SqlDbType.VarChar, 255).Value = (object?)request.String2 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString3", SqlDbType.NVarChar, 255).Value = (object?)request.String3 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString4", SqlDbType.VarChar, 255).Value = (object?)request.String4 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString5", SqlDbType.VarChar, 255).Value = (object?)request.String5 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString6", SqlDbType.VarChar, 255).Value = (object?)request.String6 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString7", SqlDbType.VarChar, 255).Value = (object?)request.String7 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate1", SqlDbType.DateTime).Value = (object?)request.Date1 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate2", SqlDbType.DateTime).Value = (object?)request.Date2 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate3", SqlDbType.DateTime).Value = (object?)request.Date3 ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TablaMaestraResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarTablaMaestraRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("TablaMaestra_Actualizar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = request.IdMaestro;
                cmd.Parameters.Add("@intNum1", SqlDbType.Int).Value = (object?)request.Num1 ?? DBNull.Value;

                cmd.Parameters.Add("@decNum2", SqlDbType.Decimal).Value = (object?)request.Num2 ?? DBNull.Value;
                cmd.Parameters["@decNum2"].Precision = 18;
                cmd.Parameters["@decNum2"].Scale = 6;

                cmd.Parameters.Add("@decNum3", SqlDbType.Decimal).Value = (object?)request.Num3 ?? DBNull.Value;
                cmd.Parameters["@decNum3"].Precision = 18;
                cmd.Parameters["@decNum3"].Scale = 6;

                cmd.Parameters.Add("@vchString1", SqlDbType.VarChar, 255).Value = (object?)request.String1 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString2", SqlDbType.VarChar, 255).Value = (object?)request.String2 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString3", SqlDbType.NVarChar, 255).Value = (object?)request.String3 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString4", SqlDbType.VarChar, 255).Value = (object?)request.String4 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString5", SqlDbType.VarChar, 255).Value = (object?)request.String5 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString6", SqlDbType.VarChar, 255).Value = (object?)request.String6 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString7", SqlDbType.VarChar, 255).Value = (object?)request.String7 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate1", SqlDbType.DateTime).Value = (object?)request.Date1 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate2", SqlDbType.DateTime).Value = (object?)request.Date2 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate3", SqlDbType.DateTime).Value = (object?)request.Date3 ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TablaMaestraResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }

        public async Task<Respuesta> ObtenerAsync(UsuarioGeneral usuarioLogueado, ObtenerTablaMaestraRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("TablaMaestra_Obtener", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario",  SqlDbType.Int).Value         = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario",    SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa",  SqlDbType.Int).Value         = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol",      SqlDbType.Int).Value         = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMaestro",  SqlDbType.Int).Value         = request.idMaestro;
                cmd.Parameters.Add("@intIdBusqueda", SqlDbType.Int).Value         = (object?)request.idBusqueda ?? DBNull.Value;
                cmd.Parameters.Add("@vchBusqueda",   SqlDbType.VarChar).Value     = (object?)request.vchBusqueda ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TablaMaestraItem>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraItem>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idTablaMaestra)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("TablaMaestra_Eliminar", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdTablaMaestra", SqlDbType.Int).Value = idTablaMaestra;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TablaMaestraResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }
        public async Task<Respuesta> ActualizarTraduccionesAsync(UsuarioGeneral usuarioLogueado, int idMaestro, int? num1, decimal? num2, decimal? num3, string? string4, string? string5, string? string6, string? string7)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("TablaMaestra_ActualizarTraducciones", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsuario", SqlDbType.VarChar, 32).Value = usuarioLogueado.Usuario;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMaestro", SqlDbType.Int).Value = idMaestro;
                cmd.Parameters.Add("@intNum1", SqlDbType.Int).Value = (object?)num1 ?? DBNull.Value;

                cmd.Parameters.Add("@decNum2", SqlDbType.Decimal).Value = (object?)num2 ?? DBNull.Value;
                cmd.Parameters["@decNum2"].Precision = 18;
                cmd.Parameters["@decNum2"].Scale = 6;

                cmd.Parameters.Add("@decNum3", SqlDbType.Decimal).Value = (object?)num3 ?? DBNull.Value;
                cmd.Parameters["@decNum3"].Precision = 18;
                cmd.Parameters["@decNum3"].Scale = 6;

                cmd.Parameters.Add("@vchString4", SqlDbType.VarChar, 255).Value = (object?)string4 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString5", SqlDbType.VarChar, 255).Value = (object?)string5 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString6", SqlDbType.VarChar, 255).Value = (object?)string6 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString7", SqlDbType.VarChar, 255).Value = (object?)string7 ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<TablaMaestraResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 3,
                    Mensaje = ex.Message,
                    Result = new List<TablaMaestraResultado>()
                };
            }
        }
    }
}