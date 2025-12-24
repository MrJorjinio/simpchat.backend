using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Simpchat.Application.Interfaces.Services;
using Simpchat.Shared.Config;
using Simpchat.Web.Middlewares;
using Simpchat.Web.Services;
using System.Text;

namespace Simpchat.Web
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWeb(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddJwtAuthentication(config)
                .AddSwagger()
                .AddChatHubService();

            return services;
        }

        private static IServiceCollection AddChatHubService(this IServiceCollection services)
        {
            services.AddScoped<IChatHubService, ChatHubService>();
            services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();
            return services;
        }

        private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings == null)
                throw new Exception("JwtSettings section is missing from configuration.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };

                    // Handle JWT from query string for SignalR
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        private static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpchatWeb", Version = "v1" });

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                     {
                         { securitySchema, new[] { "Bearer" } }
                     });
            });

            return services;
        }
    }
}
