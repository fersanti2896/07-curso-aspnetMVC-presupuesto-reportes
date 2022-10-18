using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers {
    public class TransaccionesController : Controller {
        private readonly IUsuarioRepository usuarioRepository;
        private readonly ITransaccionesRepository transaccionesRepository;
        private readonly ICuentasRepository cuentasRepository;
        private readonly ICategoriasRepository categoriasRepository;
        private readonly IMapper mapper;

        public TransaccionesController(IUsuarioRepository usuarioRepository,
                                       ITransaccionesRepository transaccionesRepository, 
                                       ICuentasRepository cuentasRepository, 
                                       ICategoriasRepository categoriasRepository,
                                       IMapper mapper) {
            this.usuarioRepository = usuarioRepository;
            this.transaccionesRepository = transaccionesRepository;
            this.cuentasRepository = cuentasRepository;
            this.categoriasRepository = categoriasRepository;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index() { 
            return View();
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioID) {
            var cuentas = await cuentasRepository.ListadoCuentas(usuarioID);

            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        private async Task<IEnumerable<SelectListItem>> ListadoCategoriasByTipoOperacion(int usuarioID, TipoOperacionModel tipoOperacion) { 
            var categorias = await categoriasRepository.ObtenerCategoriasByTipoOperacion(usuarioID, tipoOperacion);

            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        /* Obtiene las categorias en base al tipo de operacion */
        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacionModel tipoOperacion) {
            var usuarioID = usuarioRepository.ObtenerUsuarioID();
            var categorias = await ListadoCategoriasByTipoOperacion(usuarioID, tipoOperacion);

            return Ok(categorias);
        }

        /* Vista para crear una Transacción */
        public async Task<IActionResult> Crear() {
            var usuarioID = usuarioRepository.ObtenerUsuarioID();
            var modelo = new TransaccionCreacionModel();

            modelo.Cuentas = await ObtenerCuentas(usuarioID);
            modelo.Categorias = await ListadoCategoriasByTipoOperacion(usuarioID, modelo.TipoOperacionId);

            return View(modelo);
        }

        /* Guarda la transacción creada en BD */
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionModel modelo) {
            var usuarioID = usuarioRepository.ObtenerUsuarioID();

            if (!ModelState.IsValid) {
                modelo.Cuentas = await ObtenerCuentas(usuarioID);
                modelo.Categorias = await ListadoCategoriasByTipoOperacion(usuarioID, modelo.TipoOperacionId);

                return View(modelo);
            }

            var cuenta = await cuentasRepository.ObtenerCuentaById(modelo.CuentaID, usuarioID);
            
            if (cuenta is null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await categoriasRepository.ObtenerCategoriaById(modelo.CategoriaID, usuarioID);

            if (categoria is null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioID = usuarioID;

            if (modelo.TipoOperacionId == TipoOperacionModel.Gasto) {
                modelo.Monto *= -1;
            }

            await transaccionesRepository.Crear(modelo);

            return RedirectToAction("Index");
        }

        /* Crea la vista para actualizar una transacción */
        [HttpGet]
        public async Task<IActionResult> Editar(int id) {
            var usuarioID = usuarioRepository.ObtenerUsuarioID();
            var transaccion = await transaccionesRepository.ObtenerTransaccionById(id, usuarioID);

            if (transaccion is null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionModel>(transaccion);
            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacionModel.Gasto) {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorID = transaccion.CuentaID;
            modelo.Categorias = await ListadoCategoriasByTipoOperacion(usuarioID, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioID);

            return View(modelo);
        }

        /* Actualiza la información de la transacción */
        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionModel modelo) { 
            var usuarioID = usuarioRepository.ObtenerUsuarioID();
            
            if (!ModelState.IsValid) {
                modelo.Cuentas = await ObtenerCuentas(usuarioID);
                modelo.Categorias = await ListadoCategoriasByTipoOperacion(usuarioID, modelo.TipoOperacionId);

                return View(modelo);
            }

            var cuenta = await cuentasRepository.ObtenerCuentaById(modelo.CuentaID, usuarioID);

            if (cuenta is null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await categoriasRepository.ObtenerCategoriaById(modelo.CategoriaID, usuarioID);

            if (categoria is null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<TransaccionModel>(modelo);

            if (modelo.TipoOperacionId == TipoOperacionModel.Gasto) {
                transaccion.Monto *= -1;
            }

            await transaccionesRepository.ActualizarTransaccion(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorID);

            return RedirectToAction("Index");
        }

        /* Elimina una transacción */
        [HttpPost]
        public async Task<IActionResult> Borrar(int id) {
            var usuarioID = usuarioRepository.ObtenerUsuarioID();
            var transaccion = await transaccionesRepository.ObtenerTransaccionById(id, usuarioID);

            if (transaccion is null) { 
                return RedirectToAction("NoEncontrado", "Home");
            }

            await transaccionesRepository.BorrarTransaccion(id);

            return RedirectToAction("Index");
        }
    }
}
