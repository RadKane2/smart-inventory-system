using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Security;

var builder = WebApplication.CreateBuilder(args);

// Registrar controladores
builder.Services.AddControllers();

// Configurar OpenAPI
builder.Services.AddOpenApi();

// Registrar ApplicationDbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' was not found."
        )
    )
);

builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Habilitar OpenAPI solamente en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Publicar las rutas definidas en los controladores
app.MapControllers();

app.Run();