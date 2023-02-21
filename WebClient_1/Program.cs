
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json;

namespace WebClient_1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            // Add services to the container.

            builder.Services.AddControllers();

            var url = "https://localhost:20001";
            #region Swagger
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WebClient_1",
                    Version = "V1"
                });







                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{url}/connect/authorize", UriKind.Absolute),
                            TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                        {
                            { "api", "Default scope" }
                        }
                        }
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
            });
            });

            #endregion


            #region  jwt
            services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, "Bearer", options =>
    {
        options.SaveToken = true;
        options.Audience = "client-id-code";
        options.Authority = url;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // Audience should be defined on the authorization server or disabled as shown
            ClockSkew = new TimeSpan(0, 0, 30)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                // Ensure we always have an error and error description.
                if (string.IsNullOrEmpty(context.Error))
                {
                    context.Error = "invalid_token";
                }

                if (string.IsNullOrEmpty(context.ErrorDescription))
                {
                    context.ErrorDescription = "This request requires a valid JWT access token to be provided";
                }

                // Add some extra context for expired tokens.
                if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                {
                    var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                    context.Response.Headers.Add("x-token-expired", authenticationException?.Expires.ToString("o"));
                    context.ErrorDescription = $"The token expired on {authenticationException?.Expires:o}";
                }

                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = context.Error,
                    error_description = context.ErrorDescription
                }));
            }
        };
    });
            services.AddAuthorization();
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, policy =>
            //    {
            //        policy.RequireAuthenticatedUser();
            //        policy.RequireClaim("scope", "api");
            //    });
            //});
            #endregion
            #region OpenIddict
            services
               .AddOpenIddict()
               .AddValidation(options =>
               {
                   // Import the configuration from the local OpenIddict server instance.
                   options.UseLocalServer();

                   // Register the ASP.NET Core host.
                   options.UseAspNetCore();
               });
            #endregion
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();

                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.SetIsOriginAllowed(host => true);
                    builder.AllowCredentials();

                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(settings =>
                {
                    settings.SwaggerEndpoint("/swagger/v1/swagger.json", "WebClient_1");
                    settings.HeadContent = $"123";
                    settings.DocumentTitle = $"WebClient_1";
                    settings.DefaultModelExpandDepth(0);
                    settings.DefaultModelRendering(ModelRendering.Model);
                    settings.DefaultModelsExpandDepth(0);
                    settings.DocExpansion(DocExpansion.None);
                    settings.OAuthScopeSeparator(" ");
                    settings.OAuthClientId("client-id-code");
                    settings.OAuthClientSecret("client-secret-code");
                    settings.DisplayRequestDuration();
                    settings.OAuthAppName("WebClient_1");
                });
            }
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseCors("CorsPolicy");
            app.MapControllers();

            app.Run();
        }
    }
}