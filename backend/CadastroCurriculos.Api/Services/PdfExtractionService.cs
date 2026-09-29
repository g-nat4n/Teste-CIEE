using System.Text;
using System.Text.RegularExpressions;
using CadastroCurriculos.Api.DTOs;
using UglyToad.PdfPig;

namespace CadastroCurriculos.Api.Services;

public interface IPdfExtractionService
{
    Task<PdfExtractionResponse> ExtractFromPdfAsync(IFormFile file);
}

/// <summary>
/// Extrai texto de PDFs com UglyToad.PdfPig e tenta identificar nome, e-mail, telefone,
/// formação acadêmica, cursos e experiências profissionais com regras simples.
/// </summary>
public class PdfExtractionService : IPdfExtractionService
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,6}(?![a-zA-Z0-9])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Aceita: (41) 99999-9999 | 41 99999-9999 | 41999999999 | (41) 9999-9999
    private static readonly Regex PhoneRegex = new(
        @"(?:\+?55\s?)?(?:\(?\d{2}\)?[\s\-]?)?(?:9?\d{4}[\s\-]?\d{4})",
        RegexOptions.Compiled);

    private static readonly HashSet<string> NameStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "curriculo", "currículo", "curriculum", "vitae", "cv",
        "nome", "email", "e-mail", "telefone", "celular", "contato",
        "endereco", "endereço", "objetivo", "resumo", "experiencia",
        "experiência", "formação", "formacao", "educação", "educacao",
        "habilidades", "idiomas", "perfil", "profissional", "dados",
        "pessoais", "sobre", "mim", "cursos", "certificacoes", "certificações"
    };

    // Cabeçalhos conhecidos usados para delimitar o fim de uma seção
    private static readonly string[] SectionHeaders =
    [
        "formacao academica",
        "formação acadêmica",
        "formação academica",
        "formacao",
        "formação",
        "educacao",
        "educação",
        "escolaridade",
        "cursos e certificacoes",
        "cursos e certificações",
        "cursos complementares",
        "cursos",
        "certificacoes",
        "certificações",
        "experiencia profissional",
        "experiência profissional",
        "experiencias profissionais",
        "experiências profissionais",
        "experiencia",
        "experiência",
        "historico profissional",
        "histórico profissional",
        "resumo profissional",
        "resumo",
        "objetivo profissional",
        "objetivo",
        "habilidades tecnicas",
        "habilidades técnicas",
        "habilidades",
        "competencias tecnicas",
        "competências técnicas",
        "competencias",
        "competências",
        "idiomas",
        "projetos",
        "dados pessoais",
        "informacoes pessoais",
        "informações pessoais",
        "contato",
        "sobre mim",
        "perfil profissional",
        "perfil",
        "tecnologias",
        "ferramentas",
        "linguagens",
        "frameworks",
        "publicacoes",
        "publicações",
        "voluntariado",
        "premios",
        "prêmios",
        "area de interesse",
        "área de interesse",
        "area",
        "área"
    ];

    // Preferir títulos longos para evitar capturar linhas erradas
    private static readonly string[] FormacaoHeaders =
    [
        "formacao academica", "formação acadêmica", "formação academica",
        "escolaridade", "educacao", "educação", "formacao", "formação"
    ];

    private static readonly string[] ExperienciaHeaders =
    [
        "experiencia profissional", "experiência profissional",
        "experiencias profissionais", "experiências profissionais",
        "historico profissional", "histórico profissional",
        "experiencia", "experiência"
    ];

    public async Task<PdfExtractionResponse> ExtractFromPdfAsync(IFormFile file)
    {
        var validationError = ValidateFile(file);
        if (validationError is not null)
        {
            return new PdfExtractionResponse
            {
                Success = false,
                Message = validationError
            };
        }

        try
        {
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (!IsPdfContent(memoryStream))
            {
                return new PdfExtractionResponse
                {
                    Success = false,
                    Message = "O arquivo deve estar no formato PDF."
                };
            }

            memoryStream.Position = 0;
            var text = ExtractText(memoryStream);

            if (string.IsNullOrWhiteSpace(text))
            {
                return new PdfExtractionResponse
                {
                    Success = false,
                    Message = "Não foi possível extrair informações deste currículo. Preencha os dados manualmente.",
                    NomeCompleto = null,
                    Email = null,
                    Telefone = null,
                    FormacaoAcademica = null,
                    Cursos = null,
                    ExperienciasProfissionais = null
                };
            }

            var email = ExtractEmail(text);
            var telefone = ExtractPhone(text);
            var nome = ExtractName(text);
            var formacao = ExtractSection(text, FormacaoHeaders);
            var experiencias = ExtractSection(text, ExperienciaHeaders);

            var foundAny = email is not null
                || telefone is not null
                || nome is not null
                || formacao is not null
                || experiencias is not null;

            return new PdfExtractionResponse
            {
                Success = true,
                Message = foundAny
                    ? "Informações extraídas com sucesso. Confira e complete os dados se necessário."
                    : "Não foi possível extrair todas as informações do currículo. Confira e complete os dados manualmente.",
                NomeCompleto = nome,
                Email = email,
                Telefone = telefone,
                FormacaoAcademica = formacao,
                Cursos = null,
                ExperienciasProfissionais = experiencias
            };
        }
        catch (Exception)
        {
            return new PdfExtractionResponse
            {
                Success = false,
                Message = "Não foi possível extrair informações deste currículo. Preencha os dados manualmente.",
                NomeCompleto = null,
                Email = null,
                Telefone = null,
                FormacaoAcademica = null,
                Cursos = null,
                ExperienciasProfissionais = null
            };
        }
    }

    public static string? ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return "O arquivo não pode estar vazio.";
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return "O arquivo deve ter no máximo 5 MB.";
        }

        var extension = Path.GetExtension(file.FileName);
        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;

        var looksLikePdf =
            extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
            contentType is "application/pdf" or "application/x-pdf";

        if (!looksLikePdf)
        {
            return "O arquivo deve estar no formato PDF.";
        }

        return null;
    }

    private static bool IsPdfContent(Stream stream)
    {
        Span<byte> header = stackalloc byte[5];
        var read = stream.Read(header);
        return read >= 4 &&
               header[0] == (byte)'%' &&
               header[1] == (byte)'P' &&
               header[2] == (byte)'D' &&
               header[3] == (byte)'F';
    }

    private static string ExtractText(Stream stream)
    {
        var builder = new StringBuilder();

        using var document = PdfDocument.Open(stream);
        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            if (words.Count == 0)
            {
                builder.AppendLine(page.Text);
                continue;
            }

            double? previousBottom = null;
            foreach (var word in words)
            {
                var bottom = word.BoundingBox.Bottom;
                if (previousBottom is not null && Math.Abs(previousBottom.Value - bottom) > 2)
                {
                    builder.AppendLine();
                }
                else if (builder.Length > 0 && !builder.ToString().EndsWith('\n') && !char.IsWhiteSpace(builder[^1]))
                {
                    builder.Append(' ');
                }

                builder.Append(word.Text);
                previousBottom = bottom;
            }

            builder.AppendLine();
        }

        var text = builder.ToString();

        // Separa rótulos/seções colados sem espaço (exige ":" para não quebrar e-mails)
        text = Regex.Replace(
            text,
            @"(?<=\S)(?=(?:Telefone|E-mail|Email|Nome|Celular|Area|Área|Formação(?:\s+acad[eê]mica)?|Formacao(?:\s+academica)?|Educação|Educacao|Cursos|Experiência(?:\s+profissional)?|Experiencia(?:\s+profissional)?|Resumo(?:\s+profissional)?)\s*:)",
            "\n",
            RegexOptions.IgnoreCase);

        // Remove escapes literais que alguns PDFs devolvem no texto
        text = text.Replace("\\(", "(").Replace("\\)", ")").Replace("\\\\", "\\");

        return text;
    }

    /// <summary>
    /// Extrai o conteúdo de uma seção do currículo a partir de cabeçalhos conhecidos,
    /// até encontrar o próximo cabeçalho de seção ou o fim do texto.
    /// </summary>
    private static string? ExtractSection(string text, IReadOnlyList<string> targetHeaders)
    {
        var lines = text
            .Split(['\r', '\n'], StringSplitOptions.None)
            .Select(l => l.Trim())
            .ToList();

        var startIndex = -1;
        string? matchedHeader = null;

        for (var i = 0; i < lines.Count; i++)
        {
            if (!TryMatchHeader(lines[i], targetHeaders, out var header))
            {
                continue;
            }

            startIndex = i;
            matchedHeader = header;
            break;
        }

        if (startIndex < 0 || matchedHeader is null)
        {
            return null;
        }

        var content = new List<string>();
        var firstLine = StripHeaderPrefix(lines[startIndex], targetHeaders);
        if (!string.IsNullOrWhiteSpace(firstLine))
        {
            content.Add(firstLine);
        }

        for (var i = startIndex + 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (IsSectionHeader(line))
            {
                break;
            }

            // Linhas que claramente não pertencem à formação (quando extraindo formação)
            if (IsFormacaoHeaderSet(targetHeaders) && LooksLikeNonEducationContent(line))
            {
                break;
            }

            content.Add(line);
        }

        var result = string.Join("\n", content)
            .Trim()
            .Trim(':', '-', '|');

        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static bool IsFormacaoHeaderSet(IReadOnlyList<string> targetHeaders)
    {
        return targetHeaders.Any(h =>
            NormalizeHeader(h).Contains("formacao", StringComparison.Ordinal) ||
            NormalizeHeader(h).Contains("educacao", StringComparison.Ordinal) ||
            NormalizeHeader(h).Contains("escolaridade", StringComparison.Ordinal));
    }

    /// <summary>
    /// Interrompe a formação quando o texto já parece outra seção
    /// (habilidades, stacks, projetos etc.), mesmo sem cabeçalho explícito.
    /// </summary>
    private static bool LooksLikeNonEducationContent(string line)
    {
        var normalized = NormalizeHeader(line);

        // Se parece formação acadêmica, mantém
        if (Regex.IsMatch(
                normalized,
                @"\b(bacharel|licenciatura|tecnologo|tecnologo|mestrado|doutorado|graduacao|graduacao|ensino medio|ensino medio|universidade|faculdade|centro universitario|curso superior|pos graduacao|pos-graduacao)\b"))
        {
            return false;
        }

        string[] sectionLikeMarkers =
        [
            "habilidades", "competencias", "competencias", "linguagens", "frameworks",
            "banco de dados", "ferramentas", "tecnologias", "projetos", "soft skills",
            "experiencia profissional", "experiencias profissionais"
        ];

        if (sectionLikeMarkers.Any(marker =>
                normalized == NormalizeHeader(marker) ||
                normalized.StartsWith(NormalizeHeader(marker) + ":", StringComparison.Ordinal) ||
                normalized.StartsWith(NormalizeHeader(marker) + " ", StringComparison.Ordinal)))
        {
            return true;
        }

        // Linha tipicamente de stack técnica: "Java, HTML, CSS, JavaScript"
        if (Regex.IsMatch(normalized, @"^(java|html|css|javascript|typescript|python|react|node|sql)(\s*,\s*[a-z0-9.+#/ -]+){2,}$"))
        {
            return true;
        }

        return false;
    }

    private static bool TryMatchHeader(string line, IReadOnlyList<string> headers, out string matchedHeader)
    {
        matchedHeader = string.Empty;
        var normalized = NormalizeHeader(line);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        foreach (var header in headers.OrderByDescending(h => NormalizeHeader(h).Length))
        {
            var normalizedHeader = NormalizeHeader(header);
            if (normalized == normalizedHeader ||
                normalized.StartsWith(normalizedHeader + ":", StringComparison.Ordinal) ||
                normalized.StartsWith(normalizedHeader + " -", StringComparison.Ordinal) ||
                normalized.StartsWith(normalizedHeader + " ", StringComparison.Ordinal))
            {
                // Evita casar "formacao" dentro de frases longas de conteúdo
                var remainder = normalized[normalizedHeader.Length..].TrimStart(' ', ':', '-');
                if (remainder.Length > 45)
                {
                    continue;
                }

                matchedHeader = header;
                return true;
            }
        }

        return false;
    }

    private static string StripHeaderPrefix(string line, IReadOnlyList<string> headers)
    {
        var working = line.Trim().TrimEnd(':', '-', '|', '•', '*').Trim();
        foreach (var header in headers.OrderByDescending(h => h.Length))
        {
            var pattern = "^" + Regex.Escape(header) + @"\s*[:\-]?\s*";
            var stripped = Regex.Replace(working, pattern, string.Empty, RegexOptions.IgnoreCase).Trim();
            if (stripped.Length < working.Length)
            {
                return stripped;
            }
        }

        // Se a linha inteira é só o título da seção, não há conteúdo nela
        return TryMatchHeader(working, headers, out _) &&
               NormalizeHeader(working).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 3
            ? string.Empty
            : working;
    }

    private static bool IsSectionHeader(string line)
    {
        var normalized = NormalizeHeader(line);
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 55)
        {
            return false;
        }

        var wordCount = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount > 5)
        {
            return false;
        }

        // Conteúdo típico de item (com datas longas + instituição) não é cabeçalho
        if (wordCount >= 4 && Regex.IsMatch(line, @"\d{4}"))
        {
            return false;
        }

        return SectionHeaders
            .OrderByDescending(h => NormalizeHeader(h).Length)
            .Any(header =>
            {
                var normalizedHeader = NormalizeHeader(header);
                return normalized == normalizedHeader ||
                       normalized.StartsWith(normalizedHeader + ":", StringComparison.Ordinal) ||
                       normalized.StartsWith(normalizedHeader + " -", StringComparison.Ordinal) ||
                       normalized.StartsWith(normalizedHeader + " ", StringComparison.Ordinal);
            });
    }

    private static string NormalizeHeader(string value)
    {
        var normalized = value.Trim().TrimEnd(':', '-', '|', '•', '*').Trim();
        normalized = Regex.Replace(normalized, @"\s+", " ");
        return RemoveDiacritics(normalized).ToLowerInvariant();
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string? ExtractEmail(string text)
    {
        var match = EmailRegex.Match(text);
        return match.Success ? match.Value.Trim() : null;
    }

    private static string? ExtractPhone(string text)
    {
        var matches = PhoneRegex.Matches(text);
        foreach (Match match in matches)
        {
            var digits = Regex.Replace(match.Value, @"\D", string.Empty);

            // Remove código do país 55 se presente
            if (digits.StartsWith("55") && digits.Length > 11)
            {
                digits = digits[2..];
            }

            // Telefone BR: 10 ou 11 dígitos (DDD + número)
            if (digits.Length is 10 or 11)
            {
                return FormatBrazilianPhone(digits);
            }
        }

        return null;
    }

    private static string FormatBrazilianPhone(string digits)
    {
        var ddd = digits[..2];
        if (digits.Length == 11)
        {
            return $"({ddd}) {digits[2..7]}-{digits[7..]}";
        }

        return $"({ddd}) {digits[2..6]}-{digits[6..]}";
    }

    /// <summary>
    /// Estratégia de identificação do nome:
    /// 1. Procura linhas com rótulos como "Nome:" ou "Nome completo:".
    /// 2. Caso contrário, analisa as primeiras linhas do texto e escolhe a primeira
    ///    que parece um nome próprio (2+ palavras capitalizadas, sem e-mail/telefone/rótulos).
    /// </summary>
    private static string? ExtractName(string text)
    {
        var lines = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        foreach (var line in lines.Take(15))
        {
            var labeled = Regex.Match(
                line,
                @"^(?:nome(?:\s+completo)?)\s*[:\-]\s*(.+)$",
                RegexOptions.IgnoreCase);

            if (labeled.Success)
            {
                var candidate = CleanNameCandidate(labeled.Groups[1].Value);
                if (LooksLikeName(candidate))
                {
                    return candidate;
                }
            }
        }

        foreach (var line in lines.Take(10))
        {
            var candidate = CleanNameCandidate(line);
            if (LooksLikeName(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string CleanNameCandidate(string value)
    {
        value = Regex.Replace(value, @"\s+", " ").Trim();
        value = value.Trim(':', '-', '|', '•', '*');
        return value.Trim();
    }

    private static bool LooksLikeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 3 or > 80)
        {
            return false;
        }

        if (value.Contains('@') || EmailRegex.IsMatch(value))
        {
            return false;
        }

        if (Regex.IsMatch(value, @"\d{4,}"))
        {
            return false;
        }

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2)
        {
            return false;
        }

        if (words.Any(w => NameStopWords.Contains(w)))
        {
            return false;
        }

        // Maioria das palavras deve começar com letra maiúscula (padrão de nome próprio)
        var capitalized = words.Count(w => char.IsUpper(w[0]));
        return capitalized >= Math.Ceiling(words.Length * 0.6);
    }
}
