using CadastroCurriculos.Api.DTOs;

namespace CadastroCurriculos.Api.Services;

public interface ICandidateService
{
    Task<(CandidateResponse? Candidate, List<string> Errors)> CreateAsync(CreateCandidateRequest request);
    Task<IReadOnlyList<CandidateResponse>> GetAllAsync();
    Task<CandidateResponse?> GetByIdAsync(Guid id);
}
