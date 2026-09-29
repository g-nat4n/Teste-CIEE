import { Link } from 'react-router-dom';

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="container header-content">
          <Link to="/candidatos" className="brand">
            Cadastro de Currículos
          </Link>
          <nav className="nav">
            <Link to="/candidatos">Candidatos</Link>
            <Link to="/candidatos/novo" className="btn btn-primary">
              Novo candidato
            </Link>
          </nav>
        </div>
      </header>
      <main className="container main-content">{children}</main>
    </div>
  );
}
