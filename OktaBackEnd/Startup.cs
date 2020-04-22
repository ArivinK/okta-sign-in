using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Okta.AspNetCore;

namespace OktaBackEnd
{
    public class Startup
    {
        private const string AllowAllCors = "AllowAll";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Token1", options =>
             {
                 options.RequireHttpsMetadata = false;
                 options.SaveToken = true;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = Configuration["Jwt:Audience1"],
                     ValidIssuer = Configuration["Jwt:Issuer1"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key1"]))
                 };
                 options.Events = new JwtBearerEvents
                 {
                     OnTokenValidated = context =>
                     {
                         var userId = context.Principal.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

                         return Task.CompletedTask;
                     }
                 };
                 options.Events = new JwtBearerEvents
                 {
                     OnAuthenticationFailed = context =>
                     {
                         var error = context.Exception;

                         return Task.CompletedTask;
                     }
                 };
             })
            .AddJwtBearer("Token2", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Audience2"],
                    ValidIssuer = Configuration["Jwt:Issuer2"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key2"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userId = context.Principal.Claims.SingleOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

                        return Task.CompletedTask;
                    }
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var error = context.Exception;

                        return Task.CompletedTask;
                    }
                };
            })
            .AddOktaWebApi(new OktaWebApiOptions()
            {
                OktaDomain = Configuration["Okta:OktaDomain"]
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "Token1", "Token2") // Okta is a Bearer Scheme
                    .Build();
            });

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllCors,
                                  builder =>
                                  {
                                      builder.AllowAnyHeader();
                                      builder.AllowAnyMethod();
                                      builder.AllowAnyOrigin();
                                  });
            });

            // Register the Swagger services
            services.AddOpenApiDocument(document =>
            {
                document.Title = "OktaBackEnd API";
                document.Description = "Authenticate with Okta to gain access to the API";
                document.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authentication",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox : Bearer {your token}"
                });
                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Register the Swagger generator and the Swagger UI middlewares
                app.UseOpenApi();
                app.UseSwaggerUi3();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(AllowAllCors);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
