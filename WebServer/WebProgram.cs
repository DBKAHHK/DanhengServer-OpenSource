using EggLink.DanhengServer.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.IO;
using System.Net;

namespace EggLink.DanhengServer.WebServer
{
    public class WebProgram
    {
        public static void Main(string[] args, int port, string address)
        {
            BuildWebHost(args, port, address).Run();
        }

        public static IWebHost BuildWebHost(string[] args, int port, string address)
        {
            var builder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                })
                .UseUrls(address);

            if (ConfigManager.Config.HttpServer.UseSSL)
            {
                builder.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        listenOptions.UseHttps(
                            ConfigManager.Config.KeyStore.KeyStorePath,
                            ConfigManager.Config.KeyStore.KeyStorePassword
                        );
                    });
                });
            }

            return builder.Build();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                        .AllowAnyOrigin()   // Allow any origin
                        .AllowAnyMethod()   // Allow any method (GET, POST, PUT, DELETE, etc.)
                        .AllowAnyHeader()); // Allow any headers
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                using var buffer = new MemoryStream();
                var request = context.Request;
                var response = context.Response;

                var bodyStream = response.Body;
                response.Body = buffer;

                await next.Invoke();
                buffer.Position = 0;
                context.Response.Headers["Content-Length"] = (response.ContentLength ?? buffer.Length).ToString();
                context.Response.Headers.Remove("Transfer-Encoding");
                await buffer.CopyToAsync(bodyStream);
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
