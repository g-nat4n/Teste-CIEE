namespace CadastroCurriculos.Api.DTOs;

public class RespostaExtracaoPdf
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? NomeCompleto { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? FormacaoAcademica { get; set; }
    public string? Cursos { get; set; }
    public string? ExperienciasProfissionais { get; set; }
}
