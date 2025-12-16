using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;

namespace Fundo.Applications.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled WebApi exception: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Application shutting down.");
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseSerilog((context, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console())
                .UseStartup<Startup>();
        }
    }
}
