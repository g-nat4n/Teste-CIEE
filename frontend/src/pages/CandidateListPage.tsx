import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Alert } from '../components/Alert';
import { listCandidates } from '../services/api';
import type { Candidate } from '../types/candidate';
import { formatDate } from '../utils/validation';

export function CandidateListPage() {
  const [candidates, setCandidates] = useState<Candidate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        setLoading(true);
        setError(null);
        const data = await listCandidates();
        if (active) {
          setCandidates(data);
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
  }, []);

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Candidatos</h1>
          <p>Consulte os candidatos cadastrados pela equipe de recrutamento.</p>
        </div>
        <Link to="/candidatos/novo" className="btn btn-primary">
          Novo candidato
        </Link>
      </div>

      {error && <Alert type="error" message={error} />}

      {loading && <p className="muted">Carregando candidatos...</p>}

      {!loading && !error && candidates.length === 0 && (
        <div className="empty-state">
          <p>Nenhum candidato cadastrado.</p>
          <Link to="/candidatos/novo" className="btn btn-primary">
            Cadastrar primeiro candidato
          </Link>
        </div>
      )}

      {!loading && candidates.length > 0 && (
        <div className="table-wrapper">
          <table className="table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>E-mail</th>
                <th>Telefone</th>
                <th>Área de interesse</th>
                <th>Data de cadastro</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {candidates.map((candidate) => (
                <tr key={candidate.id}>
                  <td>{candidate.nomeCompleto}</td>
                  <td>{candidate.email}</td>
                  <td>{candidate.telefone || '—'}</td>
                  <td>{candidate.areaInteresse || '—'}</td>
                  <td>{formatDate(candidate.dataCadastro)}</td>
                  <td>
                    <Link to={`/candidatos/${candidate.id}`} className="btn btn-link">
                      Ver detalhes
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
