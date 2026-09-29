import type {
  ApiError,
  Candidate,
  CreateCandidateRequest,
  PdfExtractionResult,
} from '../types/candidate';

const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5052/api';

async function parseError(response: Response): Promise<string> {
  try {
    const data = (await response.json()) as ApiError;
    if (data.errors && data.errors.length > 0) {
      return data.errors.join(' ');
    }
    if (data.message) {
      return data.message;
    }
  } catch {
    // ignore parse errors
  }

  return 'Não foi possível realizar a operação. Tente novamente.';
}

export async function listCandidates(): Promise<Candidate[]> {
  const response = await fetch(`${API_BASE_URL}/candidates`);

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return response.json();
}

export async function getCandidate(id: string): Promise<Candidate> {
  const response = await fetch(`${API_BASE_URL}/candidates/${id}`);

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return response.json();
}

export async function createCandidate(
  payload: CreateCandidateRequest,
): Promise<{ message: string; candidate: Candidate }> {
  const response = await fetch(`${API_BASE_URL}/candidates`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return response.json();
}

export async function extractPdf(file: File): Promise<PdfExtractionResult> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/candidates/extract-pdf`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return response.json();
}
