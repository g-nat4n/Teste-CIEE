namespace CadastroCurriculos.Api.DTOs;

public class MessageResponse
{
    public string Message { get; set; } = string.Empty;

    public MessageResponse()
    {
    }

    public MessageResponse(string message)
    {
        Message = message;
    }
}
