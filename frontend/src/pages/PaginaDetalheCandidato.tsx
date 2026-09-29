import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { Alerta } from '../components/Alerta';
import { obterCandidato } from '../services/api';
import type { Candidato } from '../types/candidato';
import { formatarData } from '../utils/validacao';

export function PaginaDetalheCandidato() {
  const { id } = useParams<{ id: string }>();
  const [candidato, setCandidato] = useState<Candidato | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      if (!id) {
        setErro('Candidato não encontrado.');
        setCarregando(false);
        return;
      }

      try {
        setCarregando(true);
        setErro(null);
        const data = await obterCandidato(id);
        if (ativo) {
          setCandidato(data);
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
  }, [id]);

  function exportarCurriculo() {
    if (!candidato) {
      return;
    }

    const previousTitle = document.title;
    document.title = `Currículo - ${candidato.nomeCompleto}`;
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
          {candidato && (
            <button type="button" className="btn btn-primary" onClick={exportarCurriculo}>
              Exportar currículo
            </button>
          )}
          <Link to="/candidatos" className="btn btn-secondary">
            Voltar para a listagem
          </Link>
        </div>
      </div>

      {carregando && <p className="muted no-print">Carregando detalhes...</p>}
      {erro && (
        <div className="no-print">
          <Alerta tipo="erro" mensagem={erro} />
        </div>
      )}

      {!carregando && candidato && (
        <article className="panel detail-panel curriculum-export" id="curriculo-exportavel">
          <header className="curriculum-header">
            <img src="/logo-ciee.png" alt="CIEE Paraná" className="curriculum-logo" />
            <div>
              <p className="curriculum-kicker">Currículo do candidato</p>
              <h2 className="curriculum-name">{candidato.nomeCompleto}</h2>
              <p className="curriculum-subtitle">
                {[candidato.areaInteresse, candidato.email, candidato.telefone]
                  .filter(Boolean)
                  .join(' · ') || 'Cadastro CIEE/PR'}
              </p>
            </div>
          </header>

          <div className="detail-meta">
            <DetailItem label="Nome completo" value={candidato.nomeCompleto} />
            <DetailItem label="E-mail" value={candidato.email} />
            <DetailItem label="Telefone" value={candidato.telefone || '—'} />
            <DetailItem label="Área de interesse" value={candidato.areaInteresse || '—'} />
            <DetailItem label="Data de cadastro" value={formatarData(candidato.dataCadastro)} />
          </div>

          <DetailSection title="Resumo profissional" value={candidato.resumoProfissional} />
          <DetailSection title="Formação acadêmica" value={candidato.formacaoAcademica} />
          <DetailSection
            title="Experiências profissionais"
            value={candidato.experienciasProfissionais}
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
      <p className="detail-section-body">{formatCurriculumText(value)}</p>
    </section>
  );
}

/** Remove linhas em branco extras para o currículo ficar compacto na tela e no PDF. */
function formatCurriculumText(value?: string | null): string {
  if (!value?.trim()) {
    return '—';
  }

  return value
    .replace(/\r\n/g, '\n')
    .split('\n')
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
    .join('\n');
}
