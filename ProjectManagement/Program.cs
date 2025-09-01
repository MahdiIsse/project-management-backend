using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Infrastructure.Mappings;
using ProjectManagement.Api.Middleware;
using Serilog;
using ProjectManagement.Infrastructure.Repositories;
using ProjectManagement.Application.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
  {
      options.Password.RequireDigit = true;
      options.Password.RequireLowercase = true;
      options.Password.RequireUppercase = true;
      options.Password.RequireNonAlphanumeric = true;
      options.Password.RequiredLength = 8;

      options.User.RequireUniqueEmail = true;
      options.SignIn.RequireConfirmedEmail = false;
  })
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT Key is not configured.");
        if (string.IsNullOrEmpty(jwtIssuer))
            throw new InvalidOperationException("JWT Issuer is not configured.");
        if (string.IsNullOrEmpty(jwtAudience))
            throw new InvalidOperationException("JWT Audience is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddSwaggerGen(c =>
   {
       c.SwaggerDoc("v1", new OpenApiInfo
       {
           Title = "ProjectManagement API",
           Version = "v1",
           Description = "ProjectManagement API - Version 1.0"
       });

       var jwtSecurityScheme = new OpenApiSecurityScheme
       {
           Scheme = "bearer",
           BearerFormat = "JWT",
           Name = "Authorization",
           In = ParameterLocation.Header,
           Type = SecuritySchemeType.Http,
           Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
           Reference = new OpenApiReference
           {
               Id = JwtBearerDefaults.AuthenticationScheme,
               Type = ReferenceType.SecurityScheme
           }
       };

       c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

       c.AddSecurityRequirement(new OpenApiSecurityRequirement
          {
                   { jwtSecurityScheme, Array.Empty<string>() }
          });
   });

builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IColumnRepository, ColumnRepository>();
builder.Services.AddScoped<IColumnService, ColumnService>();
builder.Services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IAssigneeRepository, AssigneeRepository>();
builder.Services.AddScoped<IAssigneeService, AssigneeService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();

var app = builder.Build();


if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            db.Database.Migrate();
            Log.Information("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration failed - application will continue without migration");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => "API is running - " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

app.Run();

Log.CloseAndFlush();

public partial class Program { }