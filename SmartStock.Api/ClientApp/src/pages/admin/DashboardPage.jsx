import { useEffect, useState } from 'react';
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import reportApi from '../../api/reportApi';
import Spinner from '../../components/Spinner';

function StatCard({ label, value, accent }) {
  return (
    <div className="bg-white rounded-lg shadow-sm p-5 border border-slate-100">
      <div className="text-sm text-slate-500">{label}</div>
      <div className={`text-2xl font-bold mt-1 ${accent || 'text-slate-800'}`}>{value}</div>
    </div>
  );
}

export default function DashboardPage() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    reportApi
      .getDashboard()
      .then(({ data }) => setSummary(data.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;
  if (!summary) return <p className="text-slate-500">No data available.</p>;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800">Dashboard</h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Total Revenue" value={`₹${summary.salesSummary.totalRevenue.toFixed(2)}`} accent="text-indigo-600" />
        <StatCard label="Total Orders" value={summary.salesSummary.totalOrders} />
        <StatCard label="Stock Valuation" value={`₹${summary.totalStockValuation.toFixed(2)}`} />
        <StatCard
          label="Low Stock Items"
          value={summary.lowStockCount}
          accent={summary.lowStockCount > 0 ? 'text-red-600' : 'text-slate-800'}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow-sm p-5 border border-slate-100">
          <h2 className="text-sm font-semibold text-slate-600 mb-4">Sales Trend (14 days)</h2>
          <ResponsiveContainer width="100%" height={260}>
            <LineChart data={summary.salesTrend}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
              <XAxis dataKey="period" tick={{ fontSize: 11 }} tickFormatter={(v) => v.slice(5)} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Line type="monotone" dataKey="revenue" stroke="#4f46e5" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-5 border border-slate-100">
          <h2 className="text-sm font-semibold text-slate-600 mb-4">Top Products</h2>
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={summary.topProducts}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Bar dataKey="quantitySold" fill="#4f46e5" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}
