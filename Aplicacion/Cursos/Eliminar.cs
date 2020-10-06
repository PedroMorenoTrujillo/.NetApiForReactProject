using System.Net;
using System;
using System.Threading;
using System.Threading.Tasks;
using Aplicacion.ManejadorError;
using MediatR;
using Persistencia;

namespace Aplicacion.Cursos
{
    public class Eliminar
    {
        public class Ejectuta : IRequest {
            public int Id { get; set; }
        }

        public class Manejador : IRequestHandler<Ejectuta>
        {
            private readonly CursosOnlineContext _context;
            public Manejador(CursosOnlineContext context){
                _context = context;
            }
            public async Task<Unit> Handle(Ejectuta request, CancellationToken cancellationToken)
            {
                var curso = await _context.Curso.FindAsync(request.Id);
                if (curso == null){
                    //Usamos la excepcion personalizada
                    //throw new Exception("No se puede eliminar el curso");
                    throw new ManejadorExcepcion(HttpStatusCode.NotFound, new {mensaje = "No se encontro el curso"});
                }
                _context.Remove(curso);
                var resultado = await _context.SaveChangesAsync();
                if (resultado > 0 ){
                    return Unit.Value;
                }
                throw new Exception("No se pudieron guardar los cambios");
            }
        }
    }
}