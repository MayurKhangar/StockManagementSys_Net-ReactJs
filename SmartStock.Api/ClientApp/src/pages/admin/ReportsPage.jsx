import { useEffect, useState } from 'react';
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import reportApi from '../../api/reportApi';
import Spinner from '../../components/Spinner';

const COLORS = ['#4f46e5', '#059669', '#d97706', '#dc2626', '#0891b2', '#7c3aed'];

export default function ReportsPage() {
  const [valuation, setValuation] = useState([]);
  const [topProducts, setTopProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([reportApi.getStockValuation(), reportApi.getTopProducts(8)])
      .then(([v, t]) => {
        setValuation(v.data.data);
        setTopProducts(t.data.data);
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  const totalValuation = valuation.reduce((sum, v) => sum + v.totalValue, 0);

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800">Reports</h1>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow-sm p-5 border border-slate-100">
          <h2 className="text-sm font-semibold text-slate-600 mb-4">Revenue by Top Product</h2>
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie
                data={topProducts}
                dataKey="revenue"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={90}
                label={(entry) => entry.name}
              >
                {topProducts.map((_, idx) => (
                  <Cell key={idx} fill={COLORS[idx % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(v) => `₹${v.toFixed(2)}`} />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-5 border border-slate-100">
          <h2 className="text-sm font-semibold text-slate-600 mb-1">Stock Valuation</h2>
          <p className="text-2xl font-bold text-indigo-600 mb-4">₹{totalValuation.toFixed(2)}</p>
          <div className="overflow-y-auto max-h-56">
            <table className="w-full text-sm">
              <thead className="text-slate-500 text-left sticky top-0 bg-white">
                <tr>
                  <th className="py-1">Product</th>
                  <th className="py-1">Category</th>
                  <th className="py-1">Qty</th>
                  <th className="py-1">Value</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {valuation.map((v) => (
                  <tr key={v.productId}>
                    <td className="py-1.5">{v.name}</td>
                    <td className="py-1.5 text-slate-500">{v.categoryName}</td>
                    <td className="py-1.5">{v.stockQuantity}</td>
                    <td className="py-1.5">₹{v.totalValue.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
