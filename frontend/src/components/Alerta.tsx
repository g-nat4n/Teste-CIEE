interface PropriedadesAlerta {
  tipo: 'sucesso' | 'erro' | 'info';
  mensagem: string;
}

export function Alerta({ tipo, mensagem }: PropriedadesAlerta) {
  return (
    <div className={`alerta alerta-${tipo}`} role="alert">
      {mensagem}
    </div>
  );
}
