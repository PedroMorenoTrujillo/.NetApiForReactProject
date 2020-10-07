using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Aplicacion.Seguridad;
using Dominio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers {
    [AllowAnonymous]
    public class UsuarioController : MiControllerBase {
        //http://localhost:5000/api/Usuario/login
        [HttpPost ("login")]
        public async Task<ActionResult<UsuarioData>> Login (Login.Ejecuta parametros) {
            return await Mediator.Send (parametros);
        }

        //http://localhost:5000/api/Usuario/registrar
        [HttpPost ("registrar")]
        public async Task<ActionResult<UsuarioData>> Registrar (Registrar.Ejecuta parametros) {
            return await Mediator.Send (parametros);
        }

        //http://localhost:5000/api/Usuario/
        [HttpGet]
        public async Task<ActionResult<UsuarioData>> DevolverUsuario () {
            return await Mediator.Send (new UsuarioActual.Ejecuta ());
        }
    }
}