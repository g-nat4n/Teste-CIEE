using CadastroCurriculos.Api.Data;
using CadastroCurriculos.Api.DTOs;
using CadastroCurriculos.Api.Models;
using CadastroCurriculos.Api.Validators;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Api.Services;

public class ServicoCandidato : IServicoCandidato
{
    private readonly ContextoAplicacao _contexto;

    public ServicoCandidato(ContextoAplicacao contexto)
    {
        _contexto = contexto;
    }

    public async Task<(RespostaCandidato? Candidato, List<string> Erros)> CriarAsync(RequisicaoCriarCandidato requisicao)
    {
        var erros = ValidadorCandidato.Validar(requisicao);
        if (erros.Count > 0)
        {
            return (null, erros);
        }

        var candidato = new Candidato
        {
            Id = Guid.NewGuid(),
            NomeCompleto = requisicao.NomeCompleto.Trim(),
            Email = requisicao.Email.Trim(),
            Telefone = NormalizarOpcional(requisicao.Telefone),
            AreaInteresse = NormalizarOpcional(requisicao.AreaInteresse),
            ResumoProfissional = NormalizarOpcional(requisicao.ResumoProfissional),
            FormacaoAcademica = NormalizarOpcional(requisicao.FormacaoAcademica),
            Cursos = NormalizarOpcional(requisicao.Cursos),
            ExperienciasProfissionais = NormalizarOpcional(requisicao.ExperienciasProfissionais),
            DataCadastro = DateTime.UtcNow
        };

        _contexto.Candidatos.Add(candidato);
        await _contexto.SaveChangesAsync();

        return (MapearParaResposta(candidato), erros);
    }

    public async Task<IReadOnlyList<RespostaCandidato>> ListarAsync()
    {
        var candidatos = await _contexto.Candidatos
            .AsNoTracking()
            .OrderByDescending(c => c.DataCadastro)
            .ToListAsync();

        return candidatos.Select(MapearParaResposta).ToList();
    }

    public async Task<RespostaCandidato?> ObterPorIdAsync(Guid id)
    {
        var candidato = await _contexto.Candidatos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return candidato is null ? null : MapearParaResposta(candidato);
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static RespostaCandidato MapearParaResposta(Candidato candidato)
    {
        return new RespostaCandidato
        {
            Id = candidato.Id,
            NomeCompleto = candidato.NomeCompleto,
            Email = candidato.Email,
            Telefone = candidato.Telefone,
            AreaInteresse = candidato.AreaInteresse,
            ResumoProfissional = candidato.ResumoProfissional,
            FormacaoAcademica = candidato.FormacaoAcademica,
            Cursos = candidato.Cursos,
            ExperienciasProfissionais = candidato.ExperienciasProfissionais,
            DataCadastro = candidato.DataCadastro
        };
    }
}
