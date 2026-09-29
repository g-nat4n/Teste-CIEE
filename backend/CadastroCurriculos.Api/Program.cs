using CadastroCurriculos.Api.Data;
using CadastroCurriculos.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Um pouco acima de 5 MB para o multipart; a regra de negócio valida 5 MB no serviço
    options.Limits.MaxRequestBodySize = 6 * 1024 * 1024;
});

builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 6 * 1024 * 1024;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cadastro de Currículos API",
        Version = "v1",
        Description = "API para cadastro e consulta de candidatos, com suporte à extração de dados de PDFs."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<IPdfExtractionService, PdfExtractionService>();

const string CorsPolicy = "FrontendLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
