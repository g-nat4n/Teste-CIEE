export interface Candidato {
  id: string;
  nomeCompleto: string;
  email: string;
  telefone?: string | null;
  areaInteresse?: string | null;
  resumoProfissional?: string | null;
  formacaoAcademica?: string | null;
  experienciasProfissionais?: string | null;
  dataCadastro: string;
}

export interface RequisicaoCriarCandidato {
  nomeCompleto: string;
  email: string;
  telefone?: string;
  areaInteresse?: string;
  resumoProfissional?: string;
  formacaoAcademica?: string;
  experienciasProfissionais?: string;
}

export interface ResultadoExtracaoPdf {
  sucesso: boolean;
  mensagem: string;
  nomeCompleto: string | null;
  email: string | null;
  telefone: string | null;
  formacaoAcademica: string | null;
  experienciasProfissionais: string | null;
}

export interface ErroApi {
  mensagem?: string;
  message?: string;
  erros?: string[];
  errors?: string[];
}
