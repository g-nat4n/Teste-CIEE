namespace CadastroCurriculos.Api.DTOs;

public class CandidateResponse
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? AreaInteresse { get; set; }
    public string? ResumoProfissional { get; set; }
    public string? FormacaoAcademica { get; set; }
    public string? Cursos { get; set; }
    public string? ExperienciasProfissionais { get; set; }
    public DateTime DataCadastro { get; set; }
}
