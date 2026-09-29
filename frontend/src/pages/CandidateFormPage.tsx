import { useRef, useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert } from '../components/Alert';
import { createCandidate, extractPdf } from '../services/api';
import type { CreateCandidateRequest } from '../types/candidate';
import { validateCandidateForm, type FormErrors } from '../utils/validation';

const MAX_PDF_SIZE = 5 * 1024 * 1024;
const EMPTY_FORM: CreateCandidateRequest = {
  nomeCompleto: '',
  email: '',
  telefone: '',
  areaInteresse: '',
  resumoProfissional: '',
  formacaoAcademica: '',
  experienciasProfissionais: '',
};

export function CandidateFormPage() {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [form, setForm] = useState<CreateCandidateRequest>(EMPTY_FORM);
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});
  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'info'; text: string } | null>(
    null,
  );
  const [saving, setSaving] = useState(false);
  const [extracting, setExtracting] = useState(false);

  function updateField<K extends keyof CreateCandidateRequest>(
    key: K,
    value: CreateCandidateRequest[K],
  ) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setMessage(null);

    const errors = validateCandidateForm(form);
    setFieldErrors(errors);

    if (Object.keys(errors).length > 0) {
      return;
    }

    try {
      setSaving(true);
      const result = await createCandidate({
        nomeCompleto: form.nomeCompleto.trim(),
        email: form.email.trim(),
        telefone: form.telefone?.trim() || undefined,
        areaInteresse: form.areaInteresse?.trim() || undefined,
        resumoProfissional: form.resumoProfissional?.trim() || undefined,
        formacaoAcademica: form.formacaoAcademica?.trim() || undefined,
        experienciasProfissionais: form.experienciasProfissionais?.trim() || undefined,
      });

      setMessage({ type: 'success', text: result.message || 'Candidato cadastrado com sucesso!' });
      setForm(EMPTY_FORM);

      setTimeout(() => navigate('/candidatos'), 900);
    } catch (error) {
      setMessage({
        type: 'error',
        text:
          error instanceof Error
            ? error.message
            : 'Não foi possível realizar a operação. Tente novamente.',
      });
    } finally {
      setSaving(false);
    }
  }

  async function handlePdfSelected(file: File | null) {
    setMessage(null);

    if (!file) {
      return;
    }

    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      setMessage({ type: 'error', text: 'O arquivo selecionado não é um PDF.' });
      resetFileInput();
      return;
    }

    if (file.size > MAX_PDF_SIZE) {
      setMessage({ type: 'error', text: 'O arquivo deve ter no máximo 5 MB.' });
      resetFileInput();
      return;
    }

    try {
      setExtracting(true);
      const result = await extractPdf(file);

      if (!result.success) {
        setMessage({
          type: 'info',
          text:
            result.message ||
            'Não foi possível extrair todas as informações do currículo. Confira e complete os dados manualmente.',
        });
      } else {
        setForm((prev) => ({
          ...prev,
          nomeCompleto: result.nomeCompleto ?? prev.nomeCompleto,
          email: result.email ?? prev.email,
          telefone: result.telefone ?? prev.telefone,
          formacaoAcademica: result.formacaoAcademica ?? prev.formacaoAcademica,
          experienciasProfissionais:
            result.experienciasProfissionais ?? prev.experienciasProfissionais,
        }));

        const incomplete = !result.nomeCompleto || !result.email || !result.telefone;
        setMessage({
          type: incomplete ? 'info' : 'success',
          text: incomplete
            ? 'Não foi possível extrair todas as informações do currículo. Confira e complete os dados manualmente.'
            : result.message,
        });
      }
    } catch (error) {
      setMessage({
        type: 'error',
        text:
          error instanceof Error
            ? error.message
            : 'Não foi possível realizar a operação. Tente novamente.',
      });
    } finally {
      setExtracting(false);
      resetFileInput();
    }
  }

  function resetFileInput() {
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  }

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Novo candidato</h1>
          <p>Preencha os dados manualmente ou importe um currículo em PDF.</p>
        </div>
      </div>

      {message && <Alert type={message.type} message={message.text} />}

      <div className="panel">
        <div className="pdf-import">
          <div>
            <h2>Importar currículo PDF</h2>
            <p>Os campos encontrados serão preenchidos automaticamente no formulário abaixo.</p>
          </div>
          <label className="btn btn-secondary file-button">
            {extracting ? 'Extraindo...' : 'Selecionar PDF'}
            <input
              ref={fileInputRef}
              type="file"
              accept="application/pdf,.pdf"
              hidden
              disabled={extracting || saving}
              onChange={(event) => handlePdfSelected(event.target.files?.[0] ?? null)}
            />
          </label>
        </div>

        <form className="form" onSubmit={handleSubmit} noValidate>
          <div className="form-grid">
            <label className="field">
              <span>Nome completo *</span>
              <input
                type="text"
                value={form.nomeCompleto}
                onChange={(event) => updateField('nomeCompleto', event.target.value)}
                placeholder="Ex.: João da Silva"
              />
              {fieldErrors.nomeCompleto && (
                <small className="field-error">{fieldErrors.nomeCompleto}</small>
              )}
            </label>

            <label className="field">
              <span>E-mail *</span>
              <input
                type="email"
                value={form.email}
                onChange={(event) => updateField('email', event.target.value)}
                placeholder="Ex.: joao@email.com"
              />
              {fieldErrors.email && <small className="field-error">{fieldErrors.email}</small>}
            </label>

            <label className="field">
              <span>Telefone</span>
              <input
                type="text"
                value={form.telefone}
                onChange={(event) => updateField('telefone', event.target.value)}
                placeholder="Ex.: (41) 99999-9999"
              />
            </label>

            <label className="field">
              <span>Área ou cargo de interesse</span>
              <input
                type="text"
                value={form.areaInteresse}
                onChange={(event) => updateField('areaInteresse', event.target.value)}
                placeholder="Ex.: Desenvolvimento"
              />
            </label>
          </div>

          <label className="field">
            <span>Resumo profissional</span>
            <textarea
              rows={4}
              value={form.resumoProfissional}
              onChange={(event) => updateField('resumoProfissional', event.target.value)}
              placeholder="Breve descrição do perfil do candidato"
            />
          </label>

          <label className="field">
            <span>Formação acadêmica</span>
            <textarea
              rows={3}
              value={form.formacaoAcademica}
              onChange={(event) => updateField('formacaoAcademica', event.target.value)}
              placeholder="Ex.: Bacharelado em Ciência da Computação - Universidade XYZ (2020)"
            />
          </label>

          <label className="field">
            <span>Experiências profissionais</span>
            <textarea
              rows={5}
              value={form.experienciasProfissionais}
              onChange={(event) => updateField('experienciasProfissionais', event.target.value)}
              placeholder="Ex.: Desenvolvedora Frontend na Empresa ABC (2021-2024)"
            />
          </label>

          <div className="form-actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => navigate('/candidatos')}
              disabled={saving}
            >
              Cancelar
            </button>
            <button type="submit" className="btn btn-primary" disabled={saving || extracting}>
              {saving ? 'Salvando...' : 'Salvar candidato'}
            </button>
          </div>
        </form>
      </div>
    </section>
  );
}
