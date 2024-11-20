using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ordreChange.Data;
using ordreChange.Repositories.Implementations;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services;
using ordreChange.Services.Helpers;
using ordreChange.Services.Implementations;
using ordreChange.Services.Implementations.RoleServices;
using ordreChange.Services.Interfaces;
using ordreChange.Services.Interfaces.IRoleServices;
using ordreChange.Strategies.Roles;
using ordreChange.UnitOfWork;
using ordreChange.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Génération et configuration de la clé secrète JWT
var secureKey = SecurityHelper.GenerateSecureKey(32);
builder.Configuration["JwtSettings:Secret"] = secureKey;

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
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

// Configure Kestrel : FIX PBM compatibilité with HTTP/2.
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

// Ajout services dans le container
builder.Services.AddDbContext<OrdreDeChangeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration du CurrencyExchangeService
builder.Services.AddHttpClient<CurrencyExchangeService>();
builder.Services.AddScoped<CurrencyExchangeService>();

/// <summary>
/// Configures the dependency injection container to use the RoleStrategyContext
/// and registers role-based strategies for handling role-specific logic.
/// </summary>
builder.Services.AddScoped<RoleStrategyContext>(provider =>
{
    // Create an instance of RoleStrategyContext
    var context = new RoleStrategyContext();

    /// <summary>
    /// Registers strategies for handling logic based on specific roles.
    /// Each strategy encapsulates the logic for a single role, allowing dynamic
    /// selection of behavior based on the user's role.
    /// </summary>
    /// <remarks>
    /// - "Acheteur" is registered with <see cref="AcheteurStrategy"/> to handle buyer-specific logic.
    /// - "Validateur" is registered with <see cref="ValidateurStrategy"/> to handle validator-specific logic.
    /// </remarks>
    context.RegisterStrategy("Acheteur", new AcheteurStrategy());
    context.RegisterStrategy("Validateur", new ValidateurStrategy());

    // Return the fully configured context to the DI container
    return context;
});

builder.Services.AddScoped<IRoleStrategy, AcheteurStrategy>();
builder.Services.AddScoped<IRoleStrategy, ValidateurStrategy>();

// Dependency Injection
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IAcheteurService, AcheteurService>();
builder.Services.AddScoped<IValidateurService, ValidateurService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrdreService, OrdreService>();

// Add Authorization
builder.Services.AddAuthorization();

// Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(); // OLD_SWAGGER Conf.
// Conf. JWT into SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "exercices_PE",
        Version = "v1",
        Description = "API pour la gestion des ordres de change",
        Contact = new OpenApiContact
        {
            Name = "Antenaina Randrianantoandro",
            Email = "antenaina.randrianantoandro@pulse.mg"
        }
    });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez 'Bearer' [espace] puis votre token JWT valide.\r\n\r\nExemple: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI(); // OLD_SWAGGER Conf.
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordre de Change API V1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Cache les modèles par défaut
    });
}

app.UseHttpsRedirection();

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();