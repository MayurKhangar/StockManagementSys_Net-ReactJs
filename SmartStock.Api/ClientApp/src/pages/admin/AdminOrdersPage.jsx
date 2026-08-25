import { Fragment, useEffect, useState } from 'react';
import orderApi from '../../api/orderApi';
import Spinner from '../../components/Spinner';

const statusColors = {
  Pending: 'bg-amber-100 text-amber-700',
  Confirmed: 'bg-blue-100 text-blue-700',
  Completed: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
};

export default function AdminOrdersPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [expanded, setExpanded] = useState(null);

  useEffect(() => {
    orderApi
      .getAll()
      .then(({ data }) => setOrders(data.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800">All Orders</h1>

      <div className="bg-white rounded-lg shadow-sm border border-slate-100 overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-600 text-left">
            <tr>
              <th className="px-4 py-3">Order #</th>
              <th className="px-4 py-3">Customer</th>
              <th className="px-4 py-3">Date</th>
              <th className="px-4 py-3">Total</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {orders.map((o) => (
              <Fragment key={o.id}>
                <tr>
                  <td className="px-4 py-3 font-medium text-slate-800">{o.orderNumber}</td>
                  <td className="px-4 py-3 text-slate-500">{o.customerName}</td>
                  <td className="px-4 py-3 text-slate-500">{new Date(o.createdAt).toLocaleString()}</td>
                  <td className="px-4 py-3 text-slate-700">₹{o.totalAmount.toFixed(2)}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColors[o.status] || ''}`}>
                      {o.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      onClick={() => setExpanded(expanded === o.id ? null : o.id)}
                      className="text-indigo-600 hover:underline"
                    >
                      {expanded === o.id ? 'Hide' : 'View'}
                    </button>
                  </td>
                </tr>
                {expanded === o.id && (
                  <tr>
                    <td colSpan={6} className="px-4 py-3 bg-slate-50">
                      <table className="w-full text-xs">
                        <thead className="text-slate-500 text-left">
                          <tr>
                            <th className="py-1">Product</th>
                            <th className="py-1">Unit Price</th>
                            <th className="py-1">Qty</th>
                            <th className="py-1">Line Total</th>
                          </tr>
                        </thead>
                        <tbody>
                          {o.items.map((it, idx) => (
                            <tr key={idx}>
                              <td className="py-1">{it.productName}</td>
                              <td className="py-1">₹{it.unitPrice.toFixed(2)}</td>
                              <td className="py-1">{it.quantity}</td>
                              <td className="py-1">₹{it.lineTotal.toFixed(2)}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </td>
                  </tr>
                )}
              </Fragment>
            ))}
            {orders.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-6 text-center text-slate-400">
                  No orders yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
