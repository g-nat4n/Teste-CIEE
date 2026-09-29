import type { CreateCandidateRequest } from '../types/candidate';

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export interface FormErrors {
  nomeCompleto?: string;
  email?: string;
}

export function validateCandidateForm(values: CreateCandidateRequest): FormErrors {
  const errors: FormErrors = {};

  if (!values.nomeCompleto.trim()) {
    errors.nomeCompleto = 'O nome completo é obrigatório.';
  }

  if (!values.email.trim()) {
    errors.email = 'O e-mail é obrigatório.';
  } else if (!EMAIL_REGEX.test(values.email.trim())) {
    errors.email = 'Informe um e-mail com formato válido.';
  }

  return errors;
}

export function formatDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
