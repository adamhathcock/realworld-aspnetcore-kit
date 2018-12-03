using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RealWorld.Infrastructure;
using RealWorld.Infrastructure.Security;
using Serilog;
using System;
using System.Text;

namespace RealWorld
{
    public static class StartupExtensions
    {
        public static void AddJwt(this IServiceCollection services)
        {
            services.AddOptions();

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("somethinglongerforthisdumbalgorithmisrequired"));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = "issuer";
                options.Audience = "Audience";
                options.SigningCredentials = credentials;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = credentials.Key,
                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = true,
                    ValidIssuer = "issuer",
                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = true,
                    ValidAudience = "Audience",
                    // Validate the token expiry
                    ValidateLifetime = true,
                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        public static void AddAutoMapper(this IServiceCollection services)
        {
            AutoMapperConfiguration.Initialize(x =>
            {
                x.AddProfile(typeof(Features.Profiles.MappingProfile));
                x.AddProfile(typeof(Features.Users.MappingProfile));
            });

            Mapper.AssertConfigurationIsValid();
        }

        public static void AddSerilogLogging(this ILoggerFactory loggerFactory)
        {
            // Attach the sink to the logger configuration
            var log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                //just for local debug
                .WriteTo.LiterateConsole(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {SourceContext} {Message}{NewLine}{Exception}")
                .CreateLogger();

            loggerFactory.AddSerilog(log);
            Log.Logger = log;
        }
    }
}