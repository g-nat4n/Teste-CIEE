export interface Candidate {
  id: string;
  nomeCompleto: string;
  email: string;
  telefone?: string | null;
  areaInteresse?: string | null;
  resumoProfissional?: string | null;
  dataCadastro: string;
}

export interface CreateCandidateRequest {
  nomeCompleto: string;
  email: string;
  telefone?: string;
  areaInteresse?: string;
  resumoProfissional?: string;
}

export interface PdfExtractionResult {
  success: boolean;
  message: string;
  nomeCompleto: string | null;
  email: string | null;
  telefone: string | null;
}

export interface ApiError {
  message: string;
  errors?: string[];
}
