using System.Text.RegularExpressions;
using CadastroCurriculos.Api.DTOs;

namespace CadastroCurriculos.Api.Validators;

public static class CandidateValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static List<string> Validate(CreateCandidateRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.NomeCompleto))
        {
            errors.Add("O nome completo é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("O e-mail é obrigatório.");
        }
        else if (!EmailRegex.IsMatch(request.Email.Trim()))
        {
            errors.Add("O e-mail informado não possui um formato válido.");
        }

        return errors;
    }
}
