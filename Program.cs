using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using ordreChange.Data;
using ordreChange.Mappings;
using ordreChange.Models;
using ordreChange.Repositories.Implementations;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Helpers;
using ordreChange.Services.Implementations;
using ordreChange.Services.Interfaces;
using ordreChange.Strategies.Roles;
using ordreChange.UnitOfWork;
using ordreChange.Utilities;


//var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
var logger = NLog.LogManager.GetCurrentClassLogger();


try
{
    logger.Debug("Initialisation de l'application...");
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

        // Avoid JwtBearer de gérer automatiquement les exceptions
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.Headers.Append("Authentication-Failed", "true");
                context.Fail(context.Exception);
                return Task.CompletedTask;
            }
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

    builder.Services.AddAutoMapper(typeof(ordreChange.Mappings.AutoMapperProfile));

    // Configuration du CurrencyExchangeService
    builder.Services.AddHttpClient<CurrencyExchangeService>();
    builder.Services.AddScoped<CurrencyExchangeService>();

    builder.Services.AddScoped<RoleStrategyContext>(provider =>
    {
        var context = new RoleStrategyContext();

        context.RegisterStrategy<Ordre>("Acheteur", new OrdreAcheteurStrategy());
        context.RegisterStrategy<Ordre>("Validateur", new OrdreValidateurStrategy());


        // Return RoleStrategy config DI
        return context;
    });
    // Enregistrement stratégies
    builder.Services.AddScoped<IRoleStrategy<Ordre>, OrdreAcheteurStrategy>();
    builder.Services.AddScoped<IRoleStrategy<Ordre>, OrdreValidateurStrategy>();

    // Dependency Injection
    builder.Services.AddScoped<IAgentRepository, AgentRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IAbilityRoleService, AbilityRoleService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IOrdreService, OrdreService>();

    // Add Authorization
    builder.Services.AddAuthorization();

    // Add Controllers and Swagger
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
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

    // HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordre de Change API V1");
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            c.DefaultModelsExpandDepth(-1);
        });
    }
    app.UseMiddleware<ordreChange.Middlewares.ExceptionMiddleware>(); //  centralisation exceptions

    logger.Info("Application successfully launched");

    app.UseHttpsRedirection();

    // Add Authentication & Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Erreur critique dans l'application.");
    throw;
}
finally
{
    LogManager.Shutdown();
}