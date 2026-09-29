import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { PaginaDetalheCandidato } from './pages/PaginaDetalheCandidato';
import { PaginaFormularioCandidato } from './pages/PaginaFormularioCandidato';
import { PaginaListaCandidatos } from './pages/PaginaListaCandidatos';

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/candidatos" replace />} />
        <Route path="/candidatos" element={<PaginaListaCandidatos />} />
        <Route path="/candidatos/novo" element={<PaginaFormularioCandidato />} />
        <Route path="/candidatos/:id" element={<PaginaDetalheCandidato />} />
        <Route path="*" element={<Navigate to="/candidatos" replace />} />
      </Routes>
    </Layout>
  );
}

export default App;
