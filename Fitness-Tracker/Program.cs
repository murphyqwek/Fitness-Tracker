using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Fitness_Tracker_Infrastructure.Data;
using Fitness_Tracker_Application.Features.Users.Registration;

using System.Text;
using Fitness_Tracker_Infrastructure.Repository.User;
using Fitness_Tracker_Application.Repository.Refresh;
using Fitness_Tracker_Infrastructure.Repository.Refresh;
using StackExchange.Redis;
using Fitness_Tracker_Application.Features.Users.JWT;
namespace Fitness_Tracker_Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var conn = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(conn));

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

            builder.Services.AddScoped<Fitness_Tracker_Application.Repository.User.IUserRepository, UserRepository>();

            builder.Services.Configure<JwtConfigDTO>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddScoped<GenerateJwtToken>();

            var jwtKey = builder.Configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                jwtKey = "temporary_secret_key_for_migrations_only_32_chars_long";
            }

            builder.Services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("accessToken"))
                        {
                            context.Token = context.Request.Cookies["accessToken"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(cm =>
            {
                ConfigurationOptions options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis")!, true);
                return ConnectionMultiplexer.Connect(options);
            });

            builder.Services.AddScoped<IRefreshTokenRepository, RedisRefreshTokenRepository>();

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            if (app.Environment.IsDevelopment())
            {
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
