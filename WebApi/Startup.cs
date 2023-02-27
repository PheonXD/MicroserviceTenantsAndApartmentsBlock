using Grpc.Net.Client;
using TestTask.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using testTask.Services;


//@TODO: Add MapGrpcService and DB
namespace testTask.WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Context.ConnectionString = _configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<Context>(options =>
            {
                options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"),
                    assembly => assembly.MigrationsAssembly("WebApi"));
            });
            
            services.AddGrpc();
            services.AddGrpcReflection();
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TestTask v1",
                    Version = "v1"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "TestTask v1");
                x.RoutePrefix = "swagger";
            });
            app.UseEndpoints(endpoints => {
                endpoints.MapGrpcService<EdnpointService>();
                endpoints.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                endpoints.MapControllers();
                endpoints.MapGrpcReflectionService();
            });
        }
    }
}
