import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { Alert } from '../components/Alert';
import { getCandidate } from '../services/api';
import type { Candidate } from '../types/candidate';
import { formatDate } from '../utils/validation';

export function CandidateDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [candidate, setCandidate] = useState<Candidate | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    async function load() {
      if (!id) {
        setError('Candidato não encontrado.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        const data = await getCandidate(id);
        if (active) {
          setCandidate(data);
        }
      } catch (err) {
        if (active) {
          setError(
            err instanceof Error
              ? err.message
              : 'Não foi possível realizar a operação. Tente novamente.',
          );
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void load();
    return () => {
      active = false;
    };
  }, [id]);

  function handleExportCurriculum() {
    if (!candidate) {
      return;
    }

    const previousTitle = document.title;
    document.title = `Curriculo - ${candidate.nomeCompleto}`;
    window.print();
    document.title = previousTitle;
  }

  return (
    <section className="page">
      <div className="page-header no-print">
        <div>
          <h1>Detalhes do candidato</h1>
          <p>Informações completas do cadastro.</p>
        </div>
        <div className="page-actions">
          {candidate && (
            <button type="button" className="btn btn-primary" onClick={handleExportCurriculum}>
              Exportar currículo
            </button>
          )}
          <Link to="/candidatos" className="btn btn-secondary">
            Voltar para a listagem
          </Link>
        </div>
      </div>

      {loading && <p className="muted no-print">Carregando detalhes...</p>}
      {error && (
        <div className="no-print">
          <Alert type="error" message={error} />
        </div>
      )}

      {!loading && candidate && (
        <article className="panel detail-panel curriculum-export" id="curriculo-exportavel">
          <header className="curriculum-header">
            <img src="/logo-ciee.png" alt="CIEE Paraná" className="curriculum-logo" />
            <div>
              <p className="curriculum-kicker">Currículo do candidato</p>
              <h2 className="curriculum-name">{candidate.nomeCompleto}</h2>
              <p className="curriculum-subtitle">
                {[candidate.areaInteresse, candidate.email, candidate.telefone]
                  .filter(Boolean)
                  .join(' · ') || 'Cadastro CIEE/PR'}
              </p>
            </div>
          </header>

          <div className="detail-meta">
            <DetailItem label="Nome completo" value={candidate.nomeCompleto} />
            <DetailItem label="E-mail" value={candidate.email} />
            <DetailItem label="Telefone" value={candidate.telefone || '—'} />
            <DetailItem label="Área de interesse" value={candidate.areaInteresse || '—'} />
            <DetailItem label="Data de cadastro" value={formatDate(candidate.dataCadastro)} />
          </div>

          <DetailSection title="Resumo profissional" value={candidate.resumoProfissional} />
          <DetailSection title="Formação acadêmica" value={candidate.formacaoAcademica} />
          <DetailSection title="Cursos" value={candidate.cursos} />
          <DetailSection
            title="Experiências profissionais"
            value={candidate.experienciasProfissionais}
          />

          <p className="curriculum-footer print-only">
            Documento gerado pelo Cadastro de Currículos — CIEE/PR
          </p>
        </article>
      )}
    </section>
  );
}

function DetailItem({ label, value }: { label: string; value: string }) {
  return (
    <div className="detail-item">
      <span className="detail-label">{label}</span>
      <p className="detail-value">{value}</p>
    </div>
  );
}

function DetailSection({ title, value }: { title: string; value?: string | null }) {
  return (
    <section className="detail-section">
      <h2 className="detail-section-title">{title}</h2>
      <p className="detail-section-body">{value?.trim() || '—'}</p>
    </section>
  );
}
