using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpFlow.API.Data;
using UpFlow.API.Services;

namespace UpFlow.API
{
    /// <summary>
    /// The application startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The <see cref="Startup"/> constructor.
        /// </summary>
        /// <param name="configuration">The DI application configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The application configuration root.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddCors(options =>
            {
                options.AddPolicy
                (
                    name: "UpFlowAllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins
                        (
                            "http://localhost:3000"
                        );
                    }
                );
            });
            services.AddMemoryCache();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddDbContext<TransactionDbContext>
            (
                options =>
                {
                    // Configure EF options here.
                    options.UseSqlServer
                    (
                        Configuration.GetConnectionString("LocalDatabase"),
                        sqlServerOptions =>
                        {
                            // Configure SQL Server options here.
                            sqlServerOptions.MigrationsAssembly("UpFlow.API");
                            sqlServerOptions.EnableRetryOnFailure();
                        }
                    );
                }
            );
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "UpFlow.API", Version = "v1" });
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UpFlow.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
