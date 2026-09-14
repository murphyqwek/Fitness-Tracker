using Fintess_Tracker_Analytics.Background.Kafka;
using Fintess_Tracker_Analytics.Data;
using Fintess_Tracker_Analytics.Repository;
using Fintess_Tracker_Analytics.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace Fintess_Tracker_Analytics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            builder.Services.AddAutoMapper(
                cfg => cfg.AddMaps(typeof(Program).Assembly));

            builder.Services.AddHostedService<WorkoutCompletedConsumer>();

            var conn = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(conn));

            builder.Services.AddScoped<AnalyticService>();
            builder.Services.AddScoped<IAnalyticRepository, AnalyticRepository>();

            builder.Services.AddSingleton<IConnectionMultiplexer>(cm =>
            {
                ConfigurationOptions options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!, true);
                return ConnectionMultiplexer.Connect(options);
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var publicKeyPem =
                    builder.Configuration["Jwt:PublicKeyPem"];

                if (string.IsNullOrWhiteSpace(publicKeyPem))
                {
                    var publicKeyPath =
                        builder.Configuration["Jwt:PublicKeyPath"];

                    if (string.IsNullOrWhiteSpace(publicKeyPath))
                    {
                        throw new InvalidOperationException(
                            "Neither Jwt:PublicKeyPem nor Jwt:PublicKeyPath is configured.");
                    }

                    publicKeyPem =
                        File.ReadAllText(publicKeyPath);
                }

                var publicRsa = RSA.Create();

                publicRsa.ImportFromPem(publicKeyPem);

                var validationKey =
                    new RsaSecurityKey(publicRsa);

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,

                        ValidIssuer =
                            builder.Configuration["Jwt:Issuer"],

                        ValidAudience =
                            builder.Configuration["Jwt:Audience"],

                        ValidAlgorithms =
                        [
                            SecurityAlgorithms.RsaSha256
                        ],

                        IssuerSigningKey = validationKey
                    };
            });

            builder.Services.AddAuthorization();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });
            }

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors("AllowAll");
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}