import type { RequisicaoCriarCandidato } from '../types/candidato';

const REGEX_EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export interface ErrosFormulario {
  nomeCompleto?: string;
  email?: string;
}

export function validarFormularioCandidato(valores: RequisicaoCriarCandidato): ErrosFormulario {
  const erros: ErrosFormulario = {};

  if (!valores.nomeCompleto.trim()) {
    erros.nomeCompleto = 'O nome completo é obrigatório.';
  }

  if (!valores.email.trim()) {
    erros.email = 'O e-mail é obrigatório.';
  } else if (!REGEX_EMAIL.test(valores.email.trim())) {
    erros.email = 'Informe um e-mail com formato válido.';
  }

  return erros;
}

export function formatarData(valor: string): string {
  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) {
    return valor;
  }

  return data.toLocaleString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
