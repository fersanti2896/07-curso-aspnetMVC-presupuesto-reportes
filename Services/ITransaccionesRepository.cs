using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Services {
    public interface ITransaccionesRepository {
        Task ActualizarTransaccion(TransaccionModel transaccion, decimal montoAnterior, int cuentaAnterior);
        Task BorrarTransaccion(int id);
        Task Crear(TransaccionModel transaccion);
        Task<TransaccionModel> ObtenerTransaccionById(int id, int usuarioID);
    }
}
