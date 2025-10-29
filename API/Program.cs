using System.Text;
using FluentValidation.AspNetCore;
using Infrastructure.IRepositories;
using Infrastructure.ISecurity;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using MongoDB.Driver;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PayChase.Auth.Infrastructure.Security;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var envFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
            Environment.SetEnvironmentVariable(parts[0], parts[1]);
    }
}
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PayChase Auth API",
        Version = "v1",
        Description = "JWT-based authentication microservice (Mongo + Redis)"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your JWT}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
});

var mongoConn = builder.Configuration["MONGO_CONN"] ?? builder.Configuration.GetConnectionString("Mongo");
if (string.IsNullOrWhiteSpace(mongoConn)) throw new InvalidOperationException("Mongo connection string missing.");
var mongoDbName = builder.Configuration["MONGO_DB"] ?? builder.Configuration["Mongo:Database"] ?? "paychase";
var mongoClient = new MongoClient(mongoConn);
builder.Services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase(mongoDbName));

var redisConn = builder.Configuration["REDIS_CONN"] ?? builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrWhiteSpace(redisConn)) throw new InvalidOperationException("Redis connection string missing.");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOrgRepository, OrgRepository>();
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
builder.Services.AddSingleton<ITokenBlacklist, RedisTokenBlacklist>();
builder.Services.AddScoped<AuthService>();

var key = builder.Configuration["JWT_KEY"] ?? builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("JWT key missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PayChase Auth API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
