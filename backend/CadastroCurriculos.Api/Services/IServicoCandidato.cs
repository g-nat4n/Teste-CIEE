using CadastroCurriculos.Api.DTOs;

namespace CadastroCurriculos.Api.Services;

public interface IServicoCandidato
{
    Task<(RespostaCandidato? Candidato, List<string> Erros)> CriarAsync(RequisicaoCriarCandidato requisicao);
    Task<IReadOnlyList<RespostaCandidato>> ListarAsync();
    Task<RespostaCandidato?> ObterPorIdAsync(Guid id);
}
