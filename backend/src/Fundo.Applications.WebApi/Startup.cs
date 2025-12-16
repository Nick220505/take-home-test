using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

using Fundo.Applications.WebApi.Application.Loans;
using Fundo.Applications.WebApi.Health;
using Fundo.Applications.WebApi.Infrastructure.Persistence;
using Fundo.Applications.WebApi.Middleware;
using Fundo.Applications.WebApi.Security;
using Fundo.Applications.WebApi.Swagger;

namespace Fundo.Applications.WebApi
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
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                });

            services.AddCors(options =>
            {
                options.AddPolicy("frontend", policy =>
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            services
                .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });

            services.AddAuthorization();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Fundo Loans API", Version = "v1" });

                options.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = ApiKeyAuthenticationHandler.HeaderName,
                    Description = "API Key authentication using the X-Api-Key header."
                });

                options.OperationFilter<ApiKeyAuthOperationFilter>();
            });

            services
                .AddHealthChecks()
                .AddCheck<DbHealthCheck>("database");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("Default")));

            services.AddScoped<ILoanService, LoanService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var seedDataEnabled = _configuration.GetValue("SeedData:Enabled", env.IsDevelopment());
            var allowReseed = _configuration.GetValue("SeedData:Reseed", env.IsDevelopment());

            DbInitializer.Initialize(app.ApplicationServices, seedDataEnabled, allowReseed);

            app.UseSerilogRequestLogging();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseCors("frontend");

            app.UseAuthentication();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
