import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { CandidateDetailPage } from './pages/CandidateDetailPage';
import { CandidateFormPage } from './pages/CandidateFormPage';
import { CandidateListPage } from './pages/CandidateListPage';

function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/candidatos" replace />} />
        <Route path="/candidatos" element={<CandidateListPage />} />
        <Route path="/candidatos/novo" element={<CandidateFormPage />} />
        <Route path="/candidatos/:id" element={<CandidateDetailPage />} />
        <Route path="*" element={<Navigate to="/candidatos" replace />} />
      </Routes>
    </Layout>
  );
}

export default App;
