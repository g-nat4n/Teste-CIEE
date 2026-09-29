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

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Detalhes do candidato</h1>
          <p>Informações completas do cadastro.</p>
        </div>
        <Link to="/candidatos" className="btn btn-secondary">
          Voltar para a listagem
        </Link>
      </div>

      {loading && <p className="muted">Carregando detalhes...</p>}
      {error && <Alert type="error" message={error} />}

      {!loading && candidate && (
        <div className="panel detail-grid">
          <DetailItem label="Nome completo" value={candidate.nomeCompleto} />
          <DetailItem label="E-mail" value={candidate.email} />
          <DetailItem label="Telefone" value={candidate.telefone || '—'} />
          <DetailItem label="Área de interesse" value={candidate.areaInteresse || '—'} />
          <DetailItem label="Data de cadastro" value={formatDate(candidate.dataCadastro)} />
          <div className="detail-item detail-item-full">
            <span className="detail-label">Resumo profissional</span>
            <p className="detail-value">{candidate.resumoProfissional || '—'}</p>
          </div>
        </div>
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
