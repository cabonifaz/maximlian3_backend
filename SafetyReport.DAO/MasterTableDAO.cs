using Microsoft.Data.SqlClient;
using SafetyReport.Models;
using System.Data;
using System.Text.Json;

namespace SafetyReport.DAO
{
    public class MasterTableDAO
    {
        private readonly DbConfig _dbConfig;

        public MasterTableDAO(DbConfig dbConfig)
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
                        : 0,
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
                IdTipoMensaje = 1,
                Mensaje = "No se obtuvo respuesta del procedimiento.",
                Result = new List<T>()
            };
        }

        public async Task<Respuesta> ListarAsync(UsuarioGeneral usuarioLogueado, int? idMaster)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("MasterTable_LST", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMaster", SqlDbType.Int).Value = (object?)idMaster ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<MasterTableItem>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableItem>()
                };
            }
        }

        public async Task<Respuesta> ListarInventarioAsync(UsuarioGeneral usuarioLogueado)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("InventarioMaestros_LST", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<InventarioMaestroItem>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<InventarioMaestroItem>()
                };
            }
        }

        public async Task<Respuesta> CrearAsync(UsuarioGeneral usuarioLogueado, MasterTableRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("MasterTable_INS", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdMaster", SqlDbType.Int).Value = request.IdMaster;
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
                cmd.Parameters.Add("@vchString3", SqlDbType.VarChar, 255).Value = (object?)request.String3 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate1", SqlDbType.DateTime).Value = (object?)request.Date1 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate2", SqlDbType.DateTime).Value = (object?)request.Date2 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate3", SqlDbType.DateTime).Value = (object?)request.Date3 ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<MasterTableResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableResultado>()
                };
            }
        }

        public async Task<Respuesta> EditarAsync(UsuarioGeneral usuarioLogueado, EditarMasterTableRequest request)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("MasterTable_UPD", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;

                cmd.Parameters.Add("@intIdMaster", SqlDbType.Int).Value = request.IdMaster;
                cmd.Parameters.Add("@intNum1", SqlDbType.Int).Value = (object?)request.Num1 ?? DBNull.Value;

                cmd.Parameters.Add("@decNum2", SqlDbType.Decimal).Value = (object?)request.Num2 ?? DBNull.Value;
                cmd.Parameters["@decNum2"].Precision = 18;
                cmd.Parameters["@decNum2"].Scale = 6;

                cmd.Parameters.Add("@decNum3", SqlDbType.Decimal).Value = (object?)request.Num3 ?? DBNull.Value;
                cmd.Parameters["@decNum3"].Precision = 18;
                cmd.Parameters["@decNum3"].Scale = 6;

                cmd.Parameters.Add("@vchString1", SqlDbType.VarChar, 255).Value = (object?)request.String1 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString2", SqlDbType.VarChar, 255).Value = (object?)request.String2 ?? DBNull.Value;
                cmd.Parameters.Add("@vchString3", SqlDbType.VarChar, 255).Value = (object?)request.String3 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate1", SqlDbType.DateTime).Value = (object?)request.Date1 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate2", SqlDbType.DateTime).Value = (object?)request.Date2 ?? DBNull.Value;
                cmd.Parameters.Add("@dtDate3", SqlDbType.DateTime).Value = (object?)request.Date3 ?? DBNull.Value;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<MasterTableResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableResultado>()
                };
            }
        }

        public async Task<Respuesta> EliminarAsync(UsuarioGeneral usuarioLogueado, int idMasterTable)
        {
            try
            {
                using SqlConnection cn = new(_dbConfig.ConnectionString);
                using SqlCommand cmd = new("MasterTable_DEL", cn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@intIdUsuario", SqlDbType.Int).Value = usuarioLogueado.IdUsuario;
                cmd.Parameters.Add("@vchUsername", SqlDbType.VarChar, 32).Value = usuarioLogueado.Username;
                cmd.Parameters.Add("@intIdEmpresa", SqlDbType.Int).Value = usuarioLogueado.IdEmpresa;
                cmd.Parameters.Add("@intIdRol", SqlDbType.Int).Value = usuarioLogueado.IdRol;
                cmd.Parameters.Add("@intIdMasterTable", SqlDbType.Int).Value = idMasterTable;

                await cn.OpenAsync();
                return await LeerRespuestaAsync<MasterTableResultado>(cmd);
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = ex.Message,
                    Result = new List<MasterTableResultado>()
                };
            }
        }
    }
}