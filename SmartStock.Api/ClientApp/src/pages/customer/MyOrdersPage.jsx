import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import orderApi from '../../api/orderApi';
import Spinner from '../../components/Spinner';

const statusColors = {
  Pending: 'bg-amber-100 text-amber-700',
  Confirmed: 'bg-blue-100 text-blue-700',
  Completed: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
};

export default function MyOrdersPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    orderApi
      .getMyOrders()
      .then(({ data }) => setOrders(data.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800">My Orders</h1>

      {orders.length === 0 ? (
        <p className="text-slate-400 text-center py-10">You haven't placed any orders yet.</p>
      ) : (
        <div className="grid gap-4">
          {orders.map((o) => (
            <Link
              key={o.id}
              to={`/orders/${o.id}`}
              className="bg-white rounded-lg shadow-sm border border-slate-100 p-4 flex items-center justify-between hover:border-indigo-300 transition-colors"
            >
              <div>
                <div className="font-medium text-slate-800">{o.orderNumber}</div>
                <div className="text-sm text-slate-500">{new Date(o.createdAt).toLocaleString()}</div>
              </div>
              <div className="flex items-center gap-4">
                <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[o.status] || ''}`}>
                  {o.status}
                </span>
                <span className="font-semibold text-slate-800">₹{o.totalAmount.toFixed(2)}</span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
