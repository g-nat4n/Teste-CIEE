import { Link } from 'react-router-dom';

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="container header-content">
          <Link to="/candidatos" className="brand">
            <img src="/logo-ciee.png" alt="CIEE Paraná" className="brand-logo" />
            <span className="brand-text">
              <strong>Cadastro de Currículos</strong>
              <small>CIEE / Paraná</small>
            </span>
          </Link>
          <nav className="nav">
            <Link to="/candidatos" className="nav-link">
              Candidatos
            </Link>
            <Link to="/candidatos/novo" className="btn btn-accent">
              Novo candidato
            </Link>
          </nav>
        </div>
      </header>
      <main className="container main-content">{children}</main>
      <footer className="app-footer">
        <div className="container footer-content">
          <span>Centro de Integração Empresa-Escola do Paraná</span>
          <a href="https://cieepr.org.br/" target="_blank" rel="noreferrer">
            cieepr.org.br
          </a>
        </div>
      </footer>
    </div>
  );
}
