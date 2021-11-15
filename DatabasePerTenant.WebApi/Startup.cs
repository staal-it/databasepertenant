using DatabasePerTenant.Data.Catalog;
using DatabasePerTenant.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DatabasePerTenant.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.BuildDependencyRegister(Configuration);
            services.SetDatabaseConnection(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(p => p.GetService<IHttpContextAccessor>()?.HttpContext);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ITenantDbManager tenantDbManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            tenantDbManager.InitializeAndRegisterAllTenants().Wait();
        }
    }
}
