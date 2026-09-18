using Confluent.Kafka;
using Fitness_Tracker_Application.Features.Exercise;
using Fitness_Tracker_Application.Features.Users.JWT;
using Fitness_Tracker_Application.Features.Users.Registration;
using Fitness_Tracker_Application.Repository.Exercises;
using Fitness_Tracker_Application.Repository.Outbox;
using Fitness_Tracker_Application.Repository.Refresh;
using Fitness_Tracker_Application.Repository.User;
using Fitness_Tracker_Application.Repository.Workout;
using Fitness_Tracker_Application.Service.Kafka;
using Fitness_Tracker_Application.Validation;
using Fitness_Tracker_Infrastructure.BackgroundServices;
using Fitness_Tracker_Infrastructure.Data;
using Fitness_Tracker_Infrastructure.Repository.Exercises;
using Fitness_Tracker_Infrastructure.Repository.JWT;
using Fitness_Tracker_Infrastructure.Repository.Outbox;
using Fitness_Tracker_Infrastructure.Repository.Refresh;
using Fitness_Tracker_Infrastructure.Repository.User;
using Fitness_Tracker_Infrastructure.Repository.Workout;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace Fitness_Tracker_Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHostedService<OutboxBackgroundService>();

            var conn = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(conn));

            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(RegisterUserCommand).Assembly));
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(UserRepository).Assembly));

            builder.Services.AddMemoryCache();

            builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

            builder.Services.AddMediatR(cfg => 
                {
                    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
                    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                }
            );

            builder.Services.AddSingleton<IProducer<string, string>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var config = new ProducerConfig
                {
                    BootstrapServers = configuration["Kafka:BootstrapServers"],
                    Acks = Acks.All,
                    EnableIdempotence = true,
                };

                return new ProducerBuilder<string, string>(config).Build();
            });

            builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

            builder.Services.Configure<JwtConfigDTO>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddSingleton<IJwtSigningCredentialsProvider, JwtSigningCredentialsProvider>();
            builder.Services.AddSingleton<GenerateJwtToken>();

            builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
            builder.Services.AddProblemDetails();

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

            builder.Services.AddSingleton<IConnectionMultiplexer>(cm =>
            {
                ConfigurationOptions options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!, true);
                return ConnectionMultiplexer.Connect(options);
            });

            builder.Services.AddScoped<IRefreshTokenRepository, RedisRefreshTokenRepository>();
            builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
            builder.Services.AddScoped<IUserInformationRepository, UserInfoRepository>();
            builder.Services.AddScoped<IWorkoutIdempotencyKeyRepository, RedisWorkoutIdempotencyRepository>();
            builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();

            builder.Services.AddAuthorization();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", builder =>
                    {
                        builder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                    });
                });
            }

            var app = builder.Build();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
