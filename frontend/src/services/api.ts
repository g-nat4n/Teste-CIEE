import type {
  Candidato,
  ErroApi,
  RequisicaoCriarCandidato,
  ResultadoExtracaoPdf,
} from '../types/candidato';

const URL_BASE_API = import.meta.env.VITE_API_URL ?? 'http://localhost:5052/api';

async function interpretarErro(resposta: Response): Promise<string> {
  try {
    const dados = (await resposta.json()) as ErroApi;
    const erros = dados.erros ?? dados.errors;
    if (erros && erros.length > 0) {
      return erros.join(' ');
    }
    if (dados.mensagem || dados.message) {
      return dados.mensagem || dados.message || '';
    }
  } catch {
    // ignora falha ao interpretar o JSON de erro
  }

  return 'Não foi possível realizar a operação. Tente novamente.';
}

export async function listarCandidatos(): Promise<Candidato[]> {
  const resposta = await fetch(`${URL_BASE_API}/candidatos`);

  if (!resposta.ok) {
    throw new Error(await interpretarErro(resposta));
  }

  return resposta.json();
}

export async function obterCandidato(id: string): Promise<Candidato> {
  const resposta = await fetch(`${URL_BASE_API}/candidatos/${id}`);

  if (!resposta.ok) {
    throw new Error(await interpretarErro(resposta));
  }

  return resposta.json();
}

export async function criarCandidato(
  dados: RequisicaoCriarCandidato,
): Promise<{ mensagem: string; candidato: Candidato }> {
  const resposta = await fetch(`${URL_BASE_API}/candidatos`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(dados),
  });

  if (!resposta.ok) {
    throw new Error(await interpretarErro(resposta));
  }

  return resposta.json();
}

export async function extrairPdf(arquivo: File): Promise<ResultadoExtracaoPdf> {
  const formulario = new FormData();
  formulario.append('file', arquivo);

  const resposta = await fetch(`${URL_BASE_API}/candidatos/extrair-pdf`, {
    method: 'POST',
    body: formulario,
  });

  if (!resposta.ok) {
    throw new Error(await interpretarErro(resposta));
  }

  return resposta.json();
}
