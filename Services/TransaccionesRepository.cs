using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services {
    public class TransaccionesRepository : ITransaccionesRepository {
        private readonly string connectionString;

        public TransaccionesRepository(IConfiguration configuration) {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /* Creando Transaccion */
        public async Task Crear(TransaccionModel transaccion) {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>("Transacciones_NuevoRegistro", 
                                                            new { 
                                                                transaccion.UsuarioID, 
                                                                transaccion.FechaTransaccion, 
                                                                transaccion.Monto,
                                                                transaccion.CategoriaID, 
                                                                transaccion.CuentaID, 
                                                                transaccion.Nota 
                                                            },
                                                            commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        /* Obtiene una transacción por su id y el id del usuario */
        public async Task<TransaccionModel> ObtenerTransaccionById(int id, int usuarioID) {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<TransaccionModel>(@"SELECT Transacciones.*, Cat.TipoOperacionID
                                                                                 FROM Transacciones
                                                                                 INNER JOIN Categorias Cat
                                                                                 ON Cat.Id = Transacciones.CategoriaID
                                                                                 WHERE Transacciones.Id = @Id AND Transacciones.UsuarioID = @UsuarioID",
                                                                                 new { id, usuarioID });
        }

        /* Actualizando Transaccion */
        public async Task ActualizarTransaccion(TransaccionModel transaccion, decimal montoAnterior, int cuentaAnteriorID) {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Transacciones_Actualizar", new { 
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CategoriaID,
                transaccion.CuentaID,
                transaccion.Nota,
                montoAnterior, 
                cuentaAnteriorID
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        /* Borrando una Transacción */
        public async Task BorrarTransaccion(int id) {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Transacciones_Borrar", 
                                           new { id }, 
                                           commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
