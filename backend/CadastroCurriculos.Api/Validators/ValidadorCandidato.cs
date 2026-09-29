using System.Text.RegularExpressions;
using CadastroCurriculos.Api.DTOs;

namespace CadastroCurriculos.Api.Validators;

public static class ValidadorCandidato
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<string> Validar(RequisicaoCriarCandidato requisicao)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(requisicao.NomeCompleto))
        {
            erros.Add("O nome completo é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(requisicao.Email))
        {
            erros.Add("O e-mail é obrigatório.");
        }
        else if (!EmailRegex.IsMatch(requisicao.Email.Trim()))
        {
            erros.Add("O e-mail informado não possui um formato válido.");
        }

        return erros;
    }
}
