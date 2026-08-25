import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const navItems = [
  { to: '/admin/dashboard', label: 'Dashboard' },
  { to: '/admin/products', label: 'Products' },
  { to: '/admin/categories', label: 'Categories' },
  { to: '/admin/suppliers', label: 'Suppliers' },
  { to: '/admin/stock', label: 'Stock' },
  { to: '/admin/orders', label: 'Orders' },
  { to: '/admin/invoices', label: 'Invoices' },
  { to: '/admin/reports', label: 'Reports' },
];

export default function AdminLayout() {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen flex bg-slate-50">
      <aside className="w-64 bg-slate-900 text-slate-100 flex flex-col">
        <div className="px-6 py-5 text-xl font-bold border-b border-slate-800">
          SmartStock
          <div className="text-xs font-normal text-slate-400">Admin Panel</div>
        </div>
        <nav className="flex-1 px-3 py-4 space-y-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `block px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-indigo-600 text-white'
                    : 'text-slate-300 hover:bg-slate-800 hover:text-white'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="px-4 py-4 border-t border-slate-800">
          <div className="text-sm font-medium">{user?.fullName}</div>
          <div className="text-xs text-slate-400 mb-3">{user?.role}</div>
          <button
            onClick={logout}
            className="w-full text-sm px-3 py-2 rounded-md bg-slate-800 hover:bg-slate-700 transition-colors"
          >
            Sign out
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-y-auto">
        <div className="p-6 max-w-7xl mx-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
