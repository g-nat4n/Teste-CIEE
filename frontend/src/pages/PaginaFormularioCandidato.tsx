import { useRef, useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alerta } from '../components/Alerta';
import { criarCandidato, extrairPdf } from '../services/api';
import type { RequisicaoCriarCandidato } from '../types/candidato';
import { validarFormularioCandidato, type ErrosFormulario } from '../utils/validacao';

const TAMANHO_MAXIMO_PDF = 5 * 1024 * 1024;
const FORMULARIO_VAZIO: RequisicaoCriarCandidato = {
  nomeCompleto: '',
  email: '',
  telefone: '',
  areaInteresse: '',
  resumoProfissional: '',
  formacaoAcademica: '',
  experienciasProfissionais: '',
};

export function PaginaFormularioCandidato() {
  const navigate = useNavigate();
  const referenciaArquivo = useRef<HTMLInputElement>(null);

  const [form, setForm] = useState<RequisicaoCriarCandidato>(FORMULARIO_VAZIO);
  const [errosCampos, setErrosCampos] = useState<ErrosFormulario>({});
  const [mensagem, setMensagem] = useState<{ tipo: 'sucesso' | 'erro' | 'info'; texto: string } | null>(
    null,
  );
  const [salvando, setSalvando] = useState(false);
  const [extraindo, setExtraindo] = useState(false);

  function atualizarCampo<K extends keyof RequisicaoCriarCandidato>(
    key: K,
    value: RequisicaoCriarCandidato[K],
  ) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  async function aoEnviar(event: FormEvent) {
    event.preventDefault();
    setMensagem(null);

    const erros = validarFormularioCandidato(form);
    setErrosCampos(erros);

    if (Object.keys(erros).length > 0) {
      return;
    }

    try {
      setSalvando(true);
      const resultado = await criarCandidato({
        nomeCompleto: form.nomeCompleto.trim(),
        email: form.email.trim(),
        telefone: form.telefone?.trim() || undefined,
        areaInteresse: form.areaInteresse?.trim() || undefined,
        resumoProfissional: form.resumoProfissional?.trim() || undefined,
        formacaoAcademica: form.formacaoAcademica?.trim() || undefined,
        experienciasProfissionais: form.experienciasProfissionais?.trim() || undefined,
      });

      setMensagem({ tipo: 'sucesso', texto: resultado.mensagem || 'Candidato cadastrado com sucesso!' });
      setForm(FORMULARIO_VAZIO);

      setTimeout(() => navigate('/candidatos'), 900);
    } catch (error) {
      setMensagem({
        tipo: 'erro',
        texto:
          error instanceof Error
            ? error.message
            : 'Não foi possível realizar a operação. Tente novamente.',
      });
    } finally {
      setSalvando(false);
    }
  }

  async function aoSelecionarPdf(file: File | null) {
    setMensagem(null);

    if (!file) {
      return;
    }

    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      setMensagem({ tipo: 'erro', texto: 'O arquivo selecionado não é um PDF.' });
      limparCampoArquivo();
      return;
    }

    if (file.size > TAMANHO_MAXIMO_PDF) {
      setMensagem({ tipo: 'erro', texto: 'O arquivo deve ter no máximo 5 MB.' });
      limparCampoArquivo();
      return;
    }

    try {
      setExtraindo(true);
      const resultado = await extrairPdf(file);

      if (!resultado.sucesso) {
        setMensagem({
          tipo: 'info',
          texto:
            resultado.mensagem ||
            'Não foi possível extrair todas as informações do currículo. Confira e complete os dados manualmente.',
        });
      } else {
        setForm((prev) => ({
          ...prev,
          nomeCompleto: resultado.nomeCompleto ?? prev.nomeCompleto,
          email: resultado.email ?? prev.email,
          telefone: resultado.telefone ?? prev.telefone,
          formacaoAcademica: resultado.formacaoAcademica ?? prev.formacaoAcademica,
          experienciasProfissionais:
            resultado.experienciasProfissionais ?? prev.experienciasProfissionais,
        }));

        const incompleto = !resultado.nomeCompleto || !resultado.email || !resultado.telefone;
        setMensagem({
          tipo: incompleto ? 'info' : 'sucesso',
          texto: incompleto
            ? 'Não foi possível extrair todas as informações do currículo. Confira e complete os dados manualmente.'
            : resultado.mensagem,
        });
      }
    } catch (error) {
      setMensagem({
        tipo: 'erro',
        texto:
          error instanceof Error
            ? error.message
            : 'Não foi possível realizar a operação. Tente novamente.',
      });
    } finally {
      setExtraindo(false);
      limparCampoArquivo();
    }
  }

  function limparCampoArquivo() {
    if (referenciaArquivo.current) {
      referenciaArquivo.current.value = '';
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

      {mensagem && <Alerta tipo={mensagem.tipo} mensagem={mensagem.texto} />}

      <div className="panel">
        <div className="pdf-import">
          <div>
            <h2>Importar currículo PDF</h2>
            <p>Os campos encontrados serão preenchidos automaticamente no formulário abaixo.</p>
          </div>
          <label className="btn btn-secondary file-button">
            {extraindo ? 'Extraindo...' : 'Selecionar PDF'}
            <input
              ref={referenciaArquivo}
              type="file"
              accept="application/pdf,.pdf"
              hidden
              disabled={extraindo || salvando}
              onChange={(event) => aoSelecionarPdf(event.target.files?.[0] ?? null)}
            />
          </label>
        </div>

        <form className="form" onSubmit={aoEnviar} noValidate>
          <div className="form-grid">
            <label className="field">
              <span>Nome completo *</span>
              <input
                type="text"
                value={form.nomeCompleto}
                onChange={(event) => atualizarCampo('nomeCompleto', event.target.value)}
                placeholder="Ex.: João da Silva"
              />
              {errosCampos.nomeCompleto && (
                <small className="field-error">{errosCampos.nomeCompleto}</small>
              )}
            </label>

            <label className="field">
              <span>E-mail *</span>
              <input
                type="email"
                value={form.email}
                onChange={(event) => atualizarCampo('email', event.target.value)}
                placeholder="Ex.: joao@email.com"
              />
              {errosCampos.email && <small className="field-error">{errosCampos.email}</small>}
            </label>

            <label className="field">
              <span>Telefone</span>
              <input
                type="text"
                value={form.telefone}
                onChange={(event) => atualizarCampo('telefone', event.target.value)}
                placeholder="Ex.: (41) 99999-9999"
              />
            </label>

            <label className="field">
              <span>Área ou cargo de interesse</span>
              <input
                type="text"
                value={form.areaInteresse}
                onChange={(event) => atualizarCampo('areaInteresse', event.target.value)}
                placeholder="Ex.: Desenvolvimento"
              />
            </label>
          </div>

          <label className="field">
            <span>Resumo profissional</span>
            <textarea
              rows={4}
              value={form.resumoProfissional}
              onChange={(event) => atualizarCampo('resumoProfissional', event.target.value)}
              placeholder="Breve descrição do perfil do candidato"
            />
          </label>

          <label className="field">
            <span>Formação acadêmica</span>
            <textarea
              rows={3}
              value={form.formacaoAcademica}
              onChange={(event) => atualizarCampo('formacaoAcademica', event.target.value)}
              placeholder="Ex.: Bacharelado em Ciência da Computação - Universidade XYZ (2020)"
            />
          </label>

          <label className="field">
            <span>Experiências profissionais</span>
            <textarea
              rows={5}
              value={form.experienciasProfissionais}
              onChange={(event) => atualizarCampo('experienciasProfissionais', event.target.value)}
              placeholder="Ex.: Desenvolvedora Frontend na Empresa ABC (2021-2024)"
            />
          </label>

          <div className="form-actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => navigate('/candidatos')}
              disabled={salvando}
            >
              Cancelar
            </button>
            <button type="submit" className="btn btn-primary" disabled={salvando || extraindo}>
              {salvando ? 'Salvando...' : 'Salvar candidato'}
            </button>
          </div>
        </form>
      </div>
    </section>
  );
}
