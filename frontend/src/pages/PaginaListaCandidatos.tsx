import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Alerta } from '../components/Alerta';
import { listarCandidatos } from '../services/api';
import type { Candidato } from '../types/candidato';
import { formatarData } from '../utils/validacao';

export function PaginaListaCandidatos() {
  const [candidatos, setCandidatos] = useState<Candidato[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      try {
        setCarregando(true);
        setErro(null);
        const dados = await listarCandidatos();
        if (ativo) {
          setCandidatos(dados);
        }
      } catch (err) {
        if (ativo) {
          setErro(
            err instanceof Error
              ? err.message
              : 'Não foi possível realizar a operação. Tente novamente.',
          );
        }
      } finally {
        if (ativo) {
          setCarregando(false);
        }
      }
    }

    void carregar();
    return () => {
      ativo = false;
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

      {erro && <Alerta tipo="erro" mensagem={erro} />}

      {carregando && <p className="muted">Carregando candidatos...</p>}

      {!carregando && !erro && candidatos.length === 0 && (
        <div className="empty-state">
          <p>Nenhum candidato cadastrado.</p>
          <Link to="/candidatos/novo" className="btn btn-primary">
            Cadastrar primeiro candidato
          </Link>
        </div>
      )}

      {!carregando && candidatos.length > 0 && (
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
              {candidatos.map((candidato) => (
                <tr key={candidato.id}>
                  <td>{candidato.nomeCompleto}</td>
                  <td>{candidato.email}</td>
                  <td>{candidato.telefone || '—'}</td>
                  <td>{candidato.areaInteresse || '—'}</td>
                  <td>{formatarData(candidato.dataCadastro)}</td>
                  <td>
                    <Link to={`/candidatos/${candidato.id}`} className="btn btn-link">
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
