using CadastroCurriculos.Api.Data;
using CadastroCurriculos.Api.DTOs;
using CadastroCurriculos.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Tests;

public class ServicoCandidatoTests : IDisposable
{
    private readonly ContextoAplicacao _contexto;
    private readonly ServicoCandidato _servico;

    public ServicoCandidatoTests()
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _contexto = new ContextoAplicacao(options);
        _servico = new ServicoCandidato(_contexto);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DeveCadastrarCandidato()
    {
        var requisicao = new RequisicaoCriarCandidato
        {
            NomeCompleto = "João da Silva",
            Email = "joao@email.com",
            Telefone = "(41) 99999-9999",
            AreaInteresse = "Desenvolvimento",
            ResumoProfissional = "Desenvolvedor com experiência em Java e React."
        };

        var (candidato, erros) = await _servico.CriarAsync(requisicao);

        Assert.Empty(erros);
        Assert.NotNull(candidato);
        Assert.Equal("João da Silva", candidato!.NomeCompleto);
        Assert.Equal("joao@email.com", candidato.Email);
        Assert.NotEqual(Guid.Empty, candidato.Id);
    }

    [Fact]
    public async Task CriarAsync_SemNome_DeveRetornarErro()
    {
        var requisicao = new RequisicaoCriarCandidato
        {
            NomeCompleto = "",
            Email = "joao@email.com"
        };

        var (candidato, erros) = await _servico.CriarAsync(requisicao);

        Assert.Null(candidato);
        Assert.Contains(erros, e => e.Contains("nome", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CriarAsync_SemEmail_DeveRetornarErro()
    {
        var requisicao = new RequisicaoCriarCandidato
        {
            NomeCompleto = "João da Silva",
            Email = ""
        };

        var (candidato, erros) = await _servico.CriarAsync(requisicao);

        Assert.Null(candidato);
        Assert.Contains(erros, e => e.Contains("e-mail", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CriarAsync_ComEmailInvalido_DeveRetornarErro()
    {
        var requisicao = new RequisicaoCriarCandidato
        {
            NomeCompleto = "João da Silva",
            Email = "email-invalido"
        };

        var (candidato, erros) = await _servico.CriarAsync(requisicao);

        Assert.Null(candidato);
        Assert.Contains(erros, e => e.Contains("formato válido", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ObterPorIdAsync_CandidatoInexistente_DeveRetornarNull()
    {
        var resultado = await _servico.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ListarAsync_DeveOrdenarDoMaisRecenteParaOMaisAntigo()
    {
        await _servico.CriarAsync(new RequisicaoCriarCandidato
        {
            NomeCompleto = "Primeiro Candidato",
            Email = "primeiro@email.com"
        });

        await Task.Delay(10);

        await _servico.CriarAsync(new RequisicaoCriarCandidato
        {
            NomeCompleto = "Segundo Candidato",
            Email = "segundo@email.com"
        });

        var lista = await _servico.ListarAsync();

        Assert.Equal(2, lista.Count);
        Assert.Equal("Segundo Candidato", lista[0].NomeCompleto);
        Assert.Equal("Primeiro Candidato", lista[1].NomeCompleto);
    }

    public void Dispose()
    {
        _contexto.Dispose();
    }
}
