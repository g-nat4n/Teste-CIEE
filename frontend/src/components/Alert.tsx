interface AlertProps {
  type: 'success' | 'error' | 'info';
  message: string;
}

export function Alert({ type, message }: AlertProps) {
  return (
    <div className={`alert alert-${type}`} role="alert">
      {message}
    </div>
  );
}
