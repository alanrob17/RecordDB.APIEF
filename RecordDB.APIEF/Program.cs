using Microsoft.EntityFrameworkCore;
using RecordDB.APIEF.Data;
using RecordDB.APIEF.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Services
// -----------------------------------------------------------------------

builder.Services.AddControllers();

// Entity Framework Core DbContext
builder.Services.AddDbContext<RecordDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RecordDb")));

// Repositories (scoped — one per HTTP request)
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title   = "RecordDB API (EF Core)",
        Version = "v1",
        Description = "RESTful API for the RecordDB music collection database powered by Entity Framework Core."
    });

    // Include XML comments from this assembly so Swagger shows <summary> docs.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// -----------------------------------------------------------------------
// Pipeline
// -----------------------------------------------------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "RecordDB API v1");
        options.RoutePrefix = "swagger"; // Swagger UI at /swagger
    });

    app.UseReDoc(options =>
    {
        options.DocumentTitle = "RecordDB API Documentation";
        options.SpecUrl = "/openapi/v1.json";
        options.RoutePrefix = "redoc"; // ReDoc UI at /redoc
    });

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("RecordDB API Reference (EF Core)")
               .WithTheme(ScalarTheme.Mars)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .WithOpenApiRoutePattern("/openapi/{documentName}.json");
    });

    // Conveniently redirect root / to /scalar/v1
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
