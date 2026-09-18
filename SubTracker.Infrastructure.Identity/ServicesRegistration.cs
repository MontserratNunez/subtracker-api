using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubTracker.Core.Application.Interfaces;
using SubTracker.Core.Domain.Settings;
using SubTracker.Infrastructure.Identity.Contexts;
using SubTracker.Infrastructure.Identity.Entities;
using SubTracker.Infrastructure.Identity.Helpers;
using SubTracker.Infrastructure.Identity.Services;
using System.IdentityModel.Tokens.Jwt;

namespace SubTracker.Infrastructure.Identity
{
    public static class ServicesRegistration
    {
        public static void AddIdentityLayerIocForApi(this IServiceCollection services, IConfiguration config)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            GeneralConfiguration(services, config);

            #region Identity 
            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Tokens.PasswordResetTokenProvider = "PasswordResetTokenProvider";
            });

            services.AddIdentityCore<AppUser>().AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddSignInManager()
                .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider)
                .AddTokenProvider<PasswordResetTokenProvider>("PasswordResetTokenProvider");

            services.AddScoped<ISecurityStampValidator, SecurityStampValidator<AppUser>>();

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(24);
            });

            services.Configure<PasswordResetTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(1);
            });

            services.Configure<SecurityStampValidatorOptions>(opt =>
            {
                opt.ValidationInterval = TimeSpan.FromSeconds(10);
            });
            #endregion

            #region JWT Authentication
            services.Configure<JwtSettings>(config.GetSection("JWTSettings"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = config["JWTSettings:Issuer"],
                    ValidAudience = config["JWTSettings:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["JWTSettings:SecretKey"]!))
                };
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents()
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize("No está autorizado para acceder a este recurso.");
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize("Acceso denegado. No tiene permisos para realizar esta acción.");
                        return context.Response.WriteAsync(result);
                    },
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("accessToken"))
                        {
                            var encryptedToken = context.Request.Cookies["accessToken"];
                            if (!string.IsNullOrEmpty(encryptedToken))
                            {
                                try
                                {
                                    var provider = context.HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>();
                                    var protector = provider.CreateProtector("SubTracker.JwtCookieProtector");

                                    context.Token = protector.Unprotect(encryptedToken);
                                }
                                catch
                                {
                                    context.Token = null;
                                }
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                        var userId = context.Principal?.FindFirst("uid")?.Value;
                        var tokenStamp = context.Principal?.FindFirst("security_stamp")?.Value;

                        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenStamp))
                        {
                            context.Fail("Token no válido o datos insuficientes.");
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId);

                        if (user == null || user.SecurityStamp != tokenStamp)
                        {
                            context.Fail("La sesión ha sido revocada o ha expirado.");
                        }
                    }
                };
            });
            #endregion

            #region Services
            services.AddScoped<IAccountServiceForWebApi, AccountServiceForWebApi>();
            #endregion
        }

        #region Private methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            var connectionString = config.GetConnectionString("IdentityConnection");
            services.AddDbContext<IdentityContext>(

                (serviceProvider, opt) =>
                {
                    opt.EnableSensitiveDataLogging();
                    opt.UseSqlServer(connectionString,
                    m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Scoped
            );
            #endregion
        }    
        #endregion
    }
}

