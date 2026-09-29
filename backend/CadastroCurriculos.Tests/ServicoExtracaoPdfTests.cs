using System.Text;
using CadastroCurriculos.Api.Services;
using Microsoft.AspNetCore.Http;

namespace CadastroCurriculos.Tests;

public class ServicoExtracaoPdfTests
{
    private readonly ServicoExtracaoPdf _service = new();

    [Fact]
    public async Task ExtrairDoPdfAsync_PdfValidoComDados_DeveExtrairInformacoes()
    {
        var content = """
            Currículo

            Nome: Mariana Oliveira Santos
            E-mail: mariana.santos@email.com
            Telefone: (41) 99999-8888

            Área: Desenvolvimento de Software
            """;

        var file = CriarArquivoFormularioPdf(content, "curriculo.pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.True(result.Sucesso);
        Assert.Equal("mariana.santos@email.com", result.Email);
        Assert.Equal("(41) 99999-8888", result.Telefone);
        Assert.Contains("Mariana", result.NomeCompleto);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_ComSecoes_DeveExtrairFormacaoEExperiencia()
    {
        var content = """
            Nome: Mariana Oliveira Santos
            E-mail: mariana.santos@email.com
            Telefone: (41) 99999-8888

            Formacao academica:
            Bacharelado em Ciencia da Computacao - UFPR (2018-2022)

            Experiencia profissional:
            Desenvolvedora Frontend - Tech Solutions (2022-2024)
            Estagiaria de Desenvolvimento - SoftHouse (2021-2022)
            """;

        var file = CriarArquivoFormularioPdf(content, "curriculo-secoes.pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.True(result.Sucesso);
        Assert.Contains("Ciencia da Computacao", result.FormacaoAcademica);
        Assert.DoesNotContain("Tech Solutions", result.FormacaoAcademica);
        Assert.Contains("Tech Solutions", result.ExperienciasProfissionais);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_Formacao_NaoDeveIncluirHabilidadesOuProjetos()
    {
        var content = """
            Nome: Gustavo Natan
            E-mail: gustavo@email.com
            Telefone: (41) 98888-7777

            Formacao academica:
            Analise e Desenvolvimento de Sistemas - Universidade Positivo (2024-2026)
            Design Grafico - Universidade Positivo (2021-2023)

            Habilidades Tecnicas
            Linguagens: Java, HTML, CSS, JavaScript, Python
            Frameworks: Spring Boot, React

            Projetos
            Sistema de cadastro de curriculos
            """;

        var file = CriarArquivoFormularioPdf(content, "curriculo-formacao.pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.True(result.Sucesso);
        Assert.Contains("Universidade Positivo", result.FormacaoAcademica);
        Assert.Contains("Design Grafico", result.FormacaoAcademica);
        Assert.DoesNotContain("Habilidades", result.FormacaoAcademica);
        Assert.DoesNotContain("Java", result.FormacaoAcademica);
        Assert.DoesNotContain("Projetos", result.FormacaoAcademica);
        Assert.DoesNotContain("Spring Boot", result.FormacaoAcademica);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_ArquivoQueNaoEPdf_DeveRetornarErro()
    {
        var bytes = Encoding.UTF8.GetBytes("isto nao e um pdf");
        var file = CriarArquivoFormulario(bytes, "arquivo.txt", "text/plain");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.False(result.Sucesso);
        Assert.Equal("O arquivo deve estar no formato PDF.", result.Mensagem);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_ArquivoAcimaDe5Mb_DeveRetornarErro()
    {
        var bytes = new byte[(5 * 1024 * 1024) + 1];
        // Assinatura PDF no início para isolar a regra de tamanho
        Encoding.ASCII.GetBytes("%PDF-").CopyTo(bytes, 0);
        var file = CriarArquivoFormulario(bytes, "grande.pdf", "application/pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.False(result.Sucesso);
        Assert.Equal("O arquivo deve ter no máximo 5 MB.", result.Mensagem);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_PdfSemEmail_DeveRetornarEmailNulo()
    {
        var content = """
            Nome: Ana Paula Costa
            Telefone: (11) 98888-7777
            Experiência em administração.
            """;

        var file = CriarArquivoFormularioPdf(content, "sem-email.pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.True(result.Sucesso);
        Assert.Null(result.Email);
        Assert.NotNull(result.Telefone);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_PdfSemTelefone_DeveRetornarTelefoneNulo()
    {
        var content = """
            Nome: Carlos Eduardo Lima
            E-mail: carlos.lima@email.com
            Resumo profissional sem telefone.
            """;

        var file = CriarArquivoFormularioPdf(content, "sem-telefone.pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.True(result.Sucesso);
        Assert.Equal("carlos.lima@email.com", result.Email);
        Assert.Null(result.Telefone);
    }

    [Fact]
    public async Task ExtrairDoPdfAsync_PdfSemTextoExtrativel_DeveIndicarFalha()
    {
        // PDF válido, porém sem stream de texto utilizável
        var emptyPdf = CriarPdfMinimoSemTexto();
        var file = CriarArquivoFormulario(emptyPdf, "vazio.pdf", "application/pdf");

        var result = await _service.ExtrairDoPdfAsync(file);

        Assert.False(result.Sucesso);
        Assert.Contains("Não foi possível extrair informações", result.Mensagem);
        Assert.Null(result.NomeCompleto);
        Assert.Null(result.Email);
        Assert.Null(result.Telefone);
    }

    [Fact]
    public void ValidarArquivo_ArquivoVazio_DeveRetornarErro()
    {
        var file = CriarArquivoFormulario(Array.Empty<byte>(), "vazio.pdf", "application/pdf");

        var error = ServicoExtracaoPdf.ValidarArquivo(file);

        Assert.Equal("O arquivo não pode estar vazio.", error);
    }

    private static IFormFile CriarArquivoFormularioPdf(string text, string fileName)
    {
        var pdfBytes = CriarPdfSimples(text);
        return CriarArquivoFormulario(pdfBytes, fileName, "application/pdf");
    }

    private static IFormFile CriarArquivoFormulario(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    /// <summary>
    /// Gera um PDF mínimo com texto em Helvetica para os testes de extração.
    /// </summary>
    private static byte[] CriarPdfSimples(string text)
    {
        var lines = text
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(l => l.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)"))
            .ToList();

        var contentParts = new List<string> { "BT", "/F1 12 Tf" };
        var y = 750;
        foreach (var line in lines)
        {
            contentParts.Add($"1 0 0 1 50 {y} Tm ({line}) Tj");
            y -= 18;
        }
        contentParts.Add("ET");

        var contentStream = string.Join("\n", contentParts);
        var contentLength = Encoding.ASCII.GetByteCount(contentStream);

        var objects = new List<byte[]>
        {
            Encoding.ASCII.GetBytes("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"),
            Encoding.ASCII.GetBytes("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n"),
            Encoding.ASCII.GetBytes(
                "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792]\n" +
                "   /Contents 4 0 R\n" +
                "   /Resources << /Font << /F1 5 0 R >> >>\n>>\nendobj\n"),
            Encoding.ASCII.GetBytes($"4 0 obj\n<< /Length {contentLength} >>\nstream\n")
                .Concat(Encoding.ASCII.GetBytes(contentStream))
                .Concat(Encoding.ASCII.GetBytes("\nendstream\nendobj\n"))
                .ToArray(),
            Encoding.ASCII.GetBytes("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n")
        };

        var output = new List<byte>();
        output.AddRange(Encoding.ASCII.GetBytes("%PDF-1.4\n"));
        var offsets = new List<int> { 0 };

        foreach (var obj in objects)
        {
            offsets.Add(output.Count);
            output.AddRange(obj);
        }

        var xrefPos = output.Count;
        output.AddRange(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n"));
        output.AddRange(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
        for (var i = 1; i < offsets.Count; i++)
        {
            output.AddRange(Encoding.ASCII.GetBytes($"{offsets[i]:D10} 00000 n \n"));
        }

        output.AddRange(Encoding.ASCII.GetBytes(
            $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF\n"));

        return output.ToArray();
    }

    private static byte[] CriarPdfMinimoSemTexto()
    {
        const string pdf = """
            %PDF-1.4
            1 0 obj
            << /Type /Catalog /Pages 2 0 R >>
            endobj
            2 0 obj
            << /Type /Pages /Kids [3 0 R] /Count 1 >>
            endobj
            3 0 obj
            << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>
            endobj
            xref
            0 4
            0000000000 65535 f 
            0000000009 00000 n 
            0000000058 00000 n 
            0000000115 00000 n 
            trailer
            << /Size 4 /Root 1 0 R >>
            startxref
            190
            %%EOF
            """;

        return Encoding.ASCII.GetBytes(pdf.Replace("\r\n", "\n").Replace("\n", "\r\n"));
    }
}
