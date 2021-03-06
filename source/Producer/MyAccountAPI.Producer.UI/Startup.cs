﻿namespace MyAccountAPI.Producer.UI
{
    using Autofac;
    using MyAccountAPI.Producer.UI.Filters;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Swashbuckle.AspNetCore.Swagger;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using System;
    using Autofac.Configuration;
    using System.Linq;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(DomainExceptionFilter));
                options.Filters.Add(typeof(ValidateModelAttribute));
                options.Filters.Add(typeof(CorrelationFilter));
            });

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new ApiKeyScheme()
                    {
                        In = "header",
                        Description = "Please insert JWT with Bearer into field",
                        Name = "Authorization",
                        Type = "apiKey"
                    });

                options.DescribeAllEnumsAsStrings();

                options.IncludeXmlComments(
                    Path.ChangeExtension(
                        Assembly.GetAssembly(typeof(MyAccountAPI.Producer.UI.Controllers.CustomersController)).Location,
                        "xml"));

                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "My Account API",
                    Version = "v1",
                    Description = "The My Account Service HTTP API",
                    TermsOfService = "Terms Of Service"
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration.GetSection("Security").GetValue<string>("Issuer"),
                        ValidAudience = Configuration.GetSection("Security").GetValue<string>("Issuer"),
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                Configuration.GetSection("Security").GetValue<string>("SecretKey")))
                    };
                });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            LoadInfrastructureAssemblies();
            builder.RegisterModule(new ConfigurationModule(Configuration));
        }

        private void LoadInfrastructureAssemblies()
        {
            string[] fileNames = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(filePath => Path.GetFileName(filePath).StartsWith("MyAccountAPI.Producer.Infrastructure", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string file in fileNames)
                AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
               });
        }
    }
}
