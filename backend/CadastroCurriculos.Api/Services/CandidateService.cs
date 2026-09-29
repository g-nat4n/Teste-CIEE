using CadastroCurriculos.Api.Data;
using CadastroCurriculos.Api.DTOs;
using CadastroCurriculos.Api.Models;
using CadastroCurriculos.Api.Validators;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Api.Services;

public class CandidateService : ICandidateService
{
    private readonly AppDbContext _context;

    public CandidateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(CandidateResponse? Candidate, List<string> Errors)> CreateAsync(CreateCandidateRequest request)
    {
        var errors = CandidateValidator.Validate(request);
        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            NomeCompleto = request.NomeCompleto.Trim(),
            Email = request.Email.Trim(),
            Telefone = NormalizeOptional(request.Telefone),
            AreaInteresse = NormalizeOptional(request.AreaInteresse),
            ResumoProfissional = NormalizeOptional(request.ResumoProfissional),
            DataCadastro = DateTime.UtcNow
        };

        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync();

        return (MapToResponse(candidate), errors);
    }

    public async Task<IReadOnlyList<CandidateResponse>> GetAllAsync()
    {
        var candidates = await _context.Candidates
            .AsNoTracking()
            .OrderByDescending(c => c.DataCadastro)
            .ToListAsync();

        return candidates.Select(MapToResponse).ToList();
    }

    public async Task<CandidateResponse?> GetByIdAsync(Guid id)
    {
        var candidate = await _context.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return candidate is null ? null : MapToResponse(candidate);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static CandidateResponse MapToResponse(Candidate candidate)
    {
        return new CandidateResponse
        {
            Id = candidate.Id,
            NomeCompleto = candidate.NomeCompleto,
            Email = candidate.Email,
            Telefone = candidate.Telefone,
            AreaInteresse = candidate.AreaInteresse,
            ResumoProfissional = candidate.ResumoProfissional,
            DataCadastro = candidate.DataCadastro
        };
    }
}
