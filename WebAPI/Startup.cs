using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aplicacion.Contratos;
using Aplicacion.Cursos;
using Dominio;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Persistencia;
using Seguridad;
using WebAPI.Middleware;

namespace WebAPI {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            //Agregar el contexto para la base de datos y añadir la cadena de conexion que usara sqlServer
            //La cadena de conexion viene del archivo appsettings.json
            services.AddDbContext<CursosOnlineContext> (opt => {
                opt.UseSqlServer (Configuration.GetConnectionString ("DefaultConnection"));
            });

            //Agergar el MediatR
            services.AddMediatR (typeof (Consulta.Manejador).Assembly);

            //services.AddControllers()
            //Configuramos el nuevo metodo con la libreria fluent para las validaciones
            services.AddControllers (opt => {
                    var policy = new AuthorizationPolicyBuilder ().RequireAuthenticatedUser ().Build ();
                    opt.Filters.Add (new AuthorizeFilter (policy));
                })
                .AddFluentValidation (cfg => cfg.RegisterValidatorsFromAssemblyContaining<Nuevo> ());

            //Configuracion necesaria para que funcione el CoreIdentity desde la API
            var builder = services.AddIdentityCore<Usuario> ();
            var identityBuilder = new IdentityBuilder (builder.UserType, builder.Services);
            identityBuilder.AddEntityFrameworkStores<CursosOnlineContext> ();
            identityBuilder.AddSignInManager<SignInManager<Usuario>> ();
            services.TryAddSingleton<ISystemClock, SystemClock> ();

            //Logica para la autorizacion del JWT
            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes ("Mi palabra secreta"));
            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme).AddJwtBearer (opt => {
                opt.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateAudience = false,
                ValidateIssuer = false
                };
            });

            //Una vez creada la carpeta seguridad ...
            services.AddScoped<IJwtGenerador, JwtGenerador> ();
            services.AddScoped<IUsuarioSesion, UsuarioSesion> ();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            //middleware para la gestion de errores personalizados
            app.UseMiddleware<ManejadorErrorMiddleware> ();

            if (env.IsDevelopment ()) {
                //Como hay creado un middleware no voy a usar este metodo
                //app.UseDeveloperExceptionPage();
            }

            //Comentado porque solo se usa en ambiente de produccion
            //app.UseHttpsRedirection();

            //Uso de la autenticacion que se ha creado en el metodo configure service
            app.UseAuthentication ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}