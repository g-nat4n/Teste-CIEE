using CadastroCurriculos.Api.Data;
using CadastroCurriculos.Api.DTOs;
using CadastroCurriculos.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Tests;

public class CandidateServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CandidateService _service;

    public CandidateServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new CandidateService(_context);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_DeveCadastrarCandidato()
    {
        var request = new CreateCandidateRequest
        {
            NomeCompleto = "João da Silva",
            Email = "joao@email.com",
            Telefone = "(41) 99999-9999",
            AreaInteresse = "Desenvolvimento",
            ResumoProfissional = "Desenvolvedor com experiência em Java e React."
        };

        var (candidate, errors) = await _service.CreateAsync(request);

        Assert.Empty(errors);
        Assert.NotNull(candidate);
        Assert.Equal("João da Silva", candidate!.NomeCompleto);
        Assert.Equal("joao@email.com", candidate.Email);
        Assert.NotEqual(Guid.Empty, candidate.Id);
    }

    [Fact]
    public async Task CreateAsync_SemNome_DeveRetornarErro()
    {
        var request = new CreateCandidateRequest
        {
            NomeCompleto = "",
            Email = "joao@email.com"
        };

        var (candidate, errors) = await _service.CreateAsync(request);

        Assert.Null(candidate);
        Assert.Contains(errors, e => e.Contains("nome", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateAsync_SemEmail_DeveRetornarErro()
    {
        var request = new CreateCandidateRequest
        {
            NomeCompleto = "João da Silva",
            Email = ""
        };

        var (candidate, errors) = await _service.CreateAsync(request);

        Assert.Null(candidate);
        Assert.Contains(errors, e => e.Contains("e-mail", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateAsync_ComEmailInvalido_DeveRetornarErro()
    {
        var request = new CreateCandidateRequest
        {
            NomeCompleto = "João da Silva",
            Email = "email-invalido"
        };

        var (candidate, errors) = await _service.CreateAsync(request);

        Assert.Null(candidate);
        Assert.Contains(errors, e => e.Contains("formato válido", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetByIdAsync_CandidatoInexistente_DeveRetornarNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_DeveOrdenarDoMaisRecenteParaOMaisAntigo()
    {
        await _service.CreateAsync(new CreateCandidateRequest
        {
            NomeCompleto = "Primeiro Candidato",
            Email = "primeiro@email.com"
        });

        await Task.Delay(10);

        await _service.CreateAsync(new CreateCandidateRequest
        {
            NomeCompleto = "Segundo Candidato",
            Email = "segundo@email.com"
        });

        var list = await _service.GetAllAsync();

        Assert.Equal(2, list.Count);
        Assert.Equal("Segundo Candidato", list[0].NomeCompleto);
        Assert.Equal("Primeiro Candidato", list[1].NomeCompleto);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
