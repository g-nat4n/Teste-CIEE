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
        Title = "API de Cadastro de Currículos",
        Version = "v1",
        Description = "API para cadastro e consulta de candidatos, com suporte à extração de dados de PDFs."
    });
});

builder.Services.AddDbContext<ContextoAplicacao>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IServicoCandidato, ServicoCandidato>();
builder.Services.AddScoped<IServicoExtracaoPdf, ServicoExtracaoPdf>();

const string PoliticaCors = "FrontendLocal";
builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCors, politica =>
    {
        politica.WithOrigins("http://localhost:5173")
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

app.UseCors(PoliticaCors);

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
