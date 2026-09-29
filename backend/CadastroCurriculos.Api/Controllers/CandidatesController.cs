using CadastroCurriculos.Api.DTOs;
using CadastroCurriculos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadastroCurriculos.Api.Controllers;

[ApiController]
[Route("api/candidates")]
[Produces("application/json")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly IPdfExtractionService _pdfExtractionService;

    public CandidatesController(
        ICandidateService candidateService,
        IPdfExtractionService pdfExtractionService)
    {
        _candidateService = candidateService;
        _pdfExtractionService = pdfExtractionService;
    }

    /// <summary>
    /// Cadastra um novo candidato.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCandidateRequest request)
    {
        var (candidate, errors) = await _candidateService.CreateAsync(request);

        if (errors.Count > 0)
        {
            return BadRequest(new { message = "Não foi possível cadastrar o candidato.", errors });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = candidate!.Id },
            new
            {
                message = "Candidato cadastrado com sucesso!",
                candidate
            });
    }

    /// <summary>
    /// Lista todos os candidatos, do mais recente para o mais antigo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CandidateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var candidates = await _candidateService.GetAllAsync();
        return Ok(candidates);
    }

    /// <summary>
    /// Retorna os detalhes de um candidato específico.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CandidateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var candidate = await _candidateService.GetByIdAsync(id);

        if (candidate is null)
        {
            return NotFound(new MessageResponse("Candidato não encontrado."));
        }

        return Ok(candidate);
    }

    /// <summary>
    /// Extrai nome, e-mail e telefone de um currículo em PDF.
    /// </summary>
    [HttpPost("extract-pdf")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(PdfExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExtractPdf(IFormFile file)
    {
        var validationError = PdfExtractionService.ValidateFile(file);
        if (validationError is not null)
        {
            return BadRequest(new MessageResponse(validationError));
        }

        // Verificação adicional do conteúdo (assinatura %PDF), não só da extensão
        await using (var headerStream = file.OpenReadStream())
        {
            var header = new byte[5];
            var read = await headerStream.ReadAsync(header.AsMemory(0, 5));
            var isPdf = read >= 4 &&
                        header[0] == (byte)'%' &&
                        header[1] == (byte)'P' &&
                        header[2] == (byte)'D' &&
                        header[3] == (byte)'F';

            if (!isPdf)
            {
                return BadRequest(new MessageResponse("O arquivo deve estar no formato PDF."));
            }
        }

        var result = await _pdfExtractionService.ExtractFromPdfAsync(file);

        // Extração falhou, mas o PDF era válido: HTTP 200 para o frontend continuar o cadastro manual
        return Ok(result);
    }
}
