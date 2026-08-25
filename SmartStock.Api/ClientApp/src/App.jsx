import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import ProtectedRoute from './routes/ProtectedRoute';
import HomeRedirect from './routes/HomeRedirect';

import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import UnauthorizedPage from './pages/UnauthorizedPage';
import NotFoundPage from './pages/NotFoundPage';

import AdminLayout from './layouts/AdminLayout';
import DashboardPage from './pages/admin/DashboardPage';
import ProductsPage from './pages/admin/ProductsPage';
import CategoriesPage from './pages/admin/CategoriesPage';
import SuppliersPage from './pages/admin/SuppliersPage';
import StockPage from './pages/admin/StockPage';
import AdminOrdersPage from './pages/admin/AdminOrdersPage';
import AdminInvoicesPage from './pages/admin/AdminInvoicesPage';
import ReportsPage from './pages/admin/ReportsPage';

import CustomerLayout from './layouts/CustomerLayout';
import ShopPage from './pages/customer/ShopPage';
import CartPage from './pages/customer/CartPage';
import MyOrdersPage from './pages/customer/MyOrdersPage';
import OrderDetailPage from './pages/customer/OrderDetailPage';

const ADMIN_ROLES = ['Admin', 'StoreManager'];

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/unauthorized" element={<UnauthorizedPage />} />
            <Route path="/" element={<HomeRedirect />} />

            <Route element={<ProtectedRoute allowedRoles={ADMIN_ROLES} />}>
              <Route path="/admin" element={<AdminLayout />}>
                <Route path="dashboard" element={<DashboardPage />} />
                <Route path="products" element={<ProductsPage />} />
                <Route path="categories" element={<CategoriesPage />} />
                <Route path="suppliers" element={<SuppliersPage />} />
                <Route path="stock" element={<StockPage />} />
                <Route path="orders" element={<AdminOrdersPage />} />
                <Route path="invoices" element={<AdminInvoicesPage />} />
                <Route path="reports" element={<ReportsPage />} />
              </Route>
            </Route>

            <Route element={<ProtectedRoute allowedRoles={['Customer']} />}>
              <Route element={<CustomerLayout />}>
                <Route path="/shop" element={<ShopPage />} />
                <Route path="/cart" element={<CartPage />} />
                <Route path="/orders" element={<MyOrdersPage />} />
                <Route path="/orders/:id" element={<OrderDetailPage />} />
              </Route>
            </Route>

            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
