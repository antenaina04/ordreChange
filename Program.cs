using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ordreChange.Data;
using ordreChange.Repositories.Implementations;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Implementations;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;
using ordreChange.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Génération et configuration de la clé secrète JWT
var secureKey = SecurityHelper.GenerateSecureKey(32);
builder.Configuration["JwtSettings:Secret"] = secureKey;

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
//var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
var secretKey = Encoding.ASCII.GetBytes(secureKey);

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// Configure Kestrel to use TLS 1.2 and TLS 1.3 only, which are compatible with HTTP/2.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
    options.ListenLocalhost(7113, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// Add services to the container.
builder.Services.AddDbContext<OrdreDeChangeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Dependency Injection
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITauxChangeService, TauxChangeService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrdreService, OrdreService>();

// Add Authorization
builder.Services.AddAuthorization();

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();