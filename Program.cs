using AccountingApp.Data;
using AccountingApp.Interfaces;
using AccountingApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// ��������� ��������� HTTP-��������
ConfigurePipeline(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ��������� �������������� JWT
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "tgk",
            ValidateAudience = true,
            ValidAudience = "TgkWebApp",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TytNeHitrimSposobomYaKlady256Bit")),
            ValidateIssuerSigningKey = true,
        };

        options.RequireHttpsMetadata = true;
    });

    // ��������� ���� ������
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    // ����������� ��������
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<IAccountService, AccountService>();
    services.AddScoped<ICurrencyService, CurrencyService>();
    services.AddScoped<ITransactionService, TransactionService>();

    // ���������� ������������
    services.AddControllers();

    // ��������� Swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
}

void ConfigurePipeline(WebApplication app)
{
    // ��������� ���������� CORS (������� �����)
    app.UseCors(config =>
    {
        config.AllowAnyOrigin();
        config.AllowAnyMethod();
        config.AllowAnyHeader();
    });

    // ��������� ��������� HTTP-��������
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}