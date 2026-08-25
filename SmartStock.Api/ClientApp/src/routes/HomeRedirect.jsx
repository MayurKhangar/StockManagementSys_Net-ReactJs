import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function HomeRedirect() {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (user.role === 'Admin' || user.role === 'StoreManager') {
    return <Navigate to="/admin/dashboard" replace />;
  }

  return <Navigate to="/shop" replace />;
}
