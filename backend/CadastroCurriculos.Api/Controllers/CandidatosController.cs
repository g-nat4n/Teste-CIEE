using CadastroCurriculos.Api.DTOs;
using CadastroCurriculos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CadastroCurriculos.Api.Controllers;

[ApiController]
[Route("api/candidatos")]
[Produces("application/json")]
public class CandidatosController : ControllerBase
{
    private readonly IServicoCandidato _servicoCandidato;
    private readonly IServicoExtracaoPdf _servicoExtracaoPdf;

    public CandidatosController(
        IServicoCandidato servicoCandidato,
        IServicoExtracaoPdf servicoExtracaoPdf)
    {
        _servicoCandidato = servicoCandidato;
        _servicoExtracaoPdf = servicoExtracaoPdf;
    }

    /// <summary>
    /// Cadastra um novo candidato.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] RequisicaoCriarCandidato requisicao)
    {
        var (candidato, erros) = await _servicoCandidato.CriarAsync(requisicao);

        if (erros.Count > 0)
        {
            return BadRequest(new { mensagem = "Não foi possível cadastrar o candidato.", erros });
        }

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = candidato!.Id },
            new
            {
                mensagem = "Candidato cadastrado com sucesso!",
                candidato
            });
    }

    /// <summary>
    /// Lista todos os candidatos, do mais recente para o mais antigo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RespostaCandidato>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var candidatos = await _servicoCandidato.ListarAsync();
        return Ok(candidatos);
    }

    /// <summary>
    /// Retorna os detalhes de um candidato específico.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaCandidato), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaMensagem), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var candidato = await _servicoCandidato.ObterPorIdAsync(id);

        if (candidato is null)
        {
            return NotFound(new RespostaMensagem("Candidato não encontrado."));
        }

        return Ok(candidato);
    }

    /// <summary>
    /// Extrai nome, e-mail, telefone e demais dados de um currículo em PDF.
    /// </summary>
    [HttpPost("extrair-pdf")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(RespostaExtracaoPdf), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaMensagem), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExtrairPdf(IFormFile file)
    {
        var erroValidacao = ServicoExtracaoPdf.ValidarArquivo(file);
        if (erroValidacao is not null)
        {
            return BadRequest(new RespostaMensagem(erroValidacao));
        }

        // Verificação adicional do conteúdo (assinatura %PDF), não só da extensão
        await using (var fluxoCabecalho = file.OpenReadStream())
        {
            var cabecalho = new byte[5];
            var lidos = await fluxoCabecalho.ReadAsync(cabecalho.AsMemory(0, 5));
            var ehPdf = lidos >= 4 &&
                        cabecalho[0] == (byte)'%' &&
                        cabecalho[1] == (byte)'P' &&
                        cabecalho[2] == (byte)'D' &&
                        cabecalho[3] == (byte)'F';

            if (!ehPdf)
            {
                return BadRequest(new RespostaMensagem("O arquivo deve estar no formato PDF."));
            }
        }

        var resultado = await _servicoExtracaoPdf.ExtrairDoPdfAsync(file);

        // Extração falhou, mas o PDF era válido: HTTP 200 para o frontend continuar o cadastro manual
        return Ok(resultado);
    }
}
