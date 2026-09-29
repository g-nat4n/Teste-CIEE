namespace CadastroCurriculos.Api.DTOs;

public class PdfExtractionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? NomeCompleto { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? FormacaoAcademica { get; set; }
    public string? Cursos { get; set; }
    public string? ExperienciasProfissionais { get; set; }
}
