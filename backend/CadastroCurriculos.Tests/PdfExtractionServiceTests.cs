using System.Text;
using CadastroCurriculos.Api.Services;
using Microsoft.AspNetCore.Http;

namespace CadastroCurriculos.Tests;

public class PdfExtractionServiceTests
{
    private readonly PdfExtractionService _service = new();

    [Fact]
    public async Task ExtractFromPdfAsync_PdfValidoComDados_DeveExtrairInformacoes()
    {
        var content = """
            Currículo

            Nome: Mariana Oliveira Santos
            E-mail: mariana.santos@email.com
            Telefone: (41) 99999-8888

            Área: Desenvolvimento de Software
            """;

        var file = CreatePdfFormFile(content, "curriculo.pdf");

        var result = await _service.ExtractFromPdfAsync(file);

        Assert.True(result.Success);
        Assert.Equal("mariana.santos@email.com", result.Email);
        Assert.Equal("(41) 99999-8888", result.Telefone);
        Assert.Contains("Mariana", result.NomeCompleto);
    }

    [Fact]
    public async Task ExtractFromPdfAsync_ArquivoQueNaoEPdf_DeveRetornarErro()
    {
        var bytes = Encoding.UTF8.GetBytes("isto nao e um pdf");
        var file = CreateFormFile(bytes, "arquivo.txt", "text/plain");

        var result = await _service.ExtractFromPdfAsync(file);

        Assert.False(result.Success);
        Assert.Equal("O arquivo deve estar no formato PDF.", result.Message);
    }

    [Fact]
    public async Task ExtractFromPdfAsync_ArquivoAcimaDe5Mb_DeveRetornarErro()
    {
        var bytes = new byte[(5 * 1024 * 1024) + 1];
        // Assinatura PDF no início para isolar a regra de tamanho
        Encoding.ASCII.GetBytes("%PDF-").CopyTo(bytes, 0);
        var file = CreateFormFile(bytes, "grande.pdf", "application/pdf");

        var result = await _service.ExtractFromPdfAsync(file);

        Assert.False(result.Success);
        Assert.Equal("O arquivo deve ter no máximo 5 MB.", result.Message);
    }

    [Fact]
    public async Task ExtractFromPdfAsync_PdfSemEmail_DeveRetornarEmailNulo()
    {
        var content = """
            Nome: Ana Paula Costa
            Telefone: (11) 98888-7777
            Experiência em administração.
            """;

        var file = CreatePdfFormFile(content, "sem-email.pdf");

        var result = await _service.ExtractFromPdfAsync(file);

        Assert.True(result.Success);
        Assert.Null(result.Email);
        Assert.NotNull(result.Telefone);
    }

    [Fact]
    public async Task ExtractFromPdfAsync_PdfSemTelefone_DeveRetornarTelefoneNulo()
    {
        var content = """
            Nome: Carlos Eduardo Lima
            E-mail: carlos.lima@email.com
            Resumo profissional sem telefone.
            """;

        var file = CreatePdfFormFile(content, "sem-telefone.pdf");

        var result = await _service.ExtractFromPdfAsync(file);

        Assert.True(result.Success);
        Assert.Equal("carlos.lima@email.com", result.Email);
        Assert.Null(result.Telefone);
    }

    [Fact]
    public async Task ExtractFromPdfAsync_PdfSemTextoExtrativel_DeveIndicarFalha()
    {
        // PDF válido, porém sem stream de texto utilizável
        var emptyPdf = CreateMinimalPdfWithoutText();
        var file = CreateFormFile(emptyPdf, "vazio.pdf", "application/pdf");

        var result = await _service.ExtractFromPdfAsync(file);

        Assert.False(result.Success);
        Assert.Contains("Não foi possível extrair informações", result.Message);
        Assert.Null(result.NomeCompleto);
        Assert.Null(result.Email);
        Assert.Null(result.Telefone);
    }

    [Fact]
    public void ValidateFile_ArquivoVazio_DeveRetornarErro()
    {
        var file = CreateFormFile(Array.Empty<byte>(), "vazio.pdf", "application/pdf");

        var error = PdfExtractionService.ValidateFile(file);

        Assert.Equal("O arquivo não pode estar vazio.", error);
    }

    private static IFormFile CreatePdfFormFile(string text, string fileName)
    {
        var pdfBytes = CreateSimplePdf(text);
        return CreateFormFile(pdfBytes, fileName, "application/pdf");
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
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
    private static byte[] CreateSimplePdf(string text)
    {
        // Escapa caracteres especiais do PDF
        var safeText = text
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("\r", " ")
            .Replace("\n", "\\n");

        var contentStream = $"BT /F1 12 Tf 50 750 Td ({safeText}) Tj ET";
        var contentLength = Encoding.ASCII.GetByteCount(contentStream);

        var pdf = $"""
            %PDF-1.4
            1 0 obj
            << /Type /Catalog /Pages 2 0 R >>
            endobj
            2 0 obj
            << /Type /Pages /Kids [3 0 R] /Count 1 >>
            endobj
            3 0 obj
            << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792]
               /Contents 4 0 R
               /Resources << /Font << /F1 5 0 R >> >>
            >>
            endobj
            4 0 obj
            << /Length {contentLength} >>
            stream
            {contentStream}
            endstream
            endobj
            5 0 obj
            << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
            endobj
            xref
            0 6
            0000000000 65535 f 
            0000000009 00000 n 
            0000000058 00000 n 
            0000000115 00000 n 
            0000000266 00000 n 
            0000000360 00000 n 
            trailer
            << /Size 6 /Root 1 0 R >>
            startxref
            429
            %%EOF
            """;

        return Encoding.ASCII.GetBytes(pdf.Replace("\r\n", "\n").Replace("\n", "\r\n"));
    }

    private static byte[] CreateMinimalPdfWithoutText()
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
