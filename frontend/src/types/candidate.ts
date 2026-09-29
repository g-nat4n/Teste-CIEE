export interface Candidate {
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

export interface CreateCandidateRequest {
  nomeCompleto: string;
  email: string;
  telefone?: string;
  areaInteresse?: string;
  resumoProfissional?: string;
  formacaoAcademica?: string;
  experienciasProfissionais?: string;
}

export interface PdfExtractionResult {
  success: boolean;
  message: string;
  nomeCompleto: string | null;
  email: string | null;
  telefone: string | null;
  formacaoAcademica: string | null;
  experienciasProfissionais: string | null;
}

export interface ApiError {
  message: string;
  errors?: string[];
}
