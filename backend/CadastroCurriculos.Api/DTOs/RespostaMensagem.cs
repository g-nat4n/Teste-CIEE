namespace CadastroCurriculos.Api.DTOs;

public class RespostaMensagem
{
    public string Mensagem { get; set; } = string.Empty;

    public RespostaMensagem()
    {
    }

    public RespostaMensagem(string mensagem)
    {
        Mensagem = mensagem;
    }
}
