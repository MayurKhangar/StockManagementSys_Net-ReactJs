import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import orderApi from '../../api/orderApi';
import Spinner from '../../components/Spinner';
import { downloadInvoicePdf } from '../../utils/downloadPdf';

export default function OrderDetailPage() {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [cancelling, setCancelling] = useState(false);

  const load = () => {
    setLoading(true);
    orderApi
      .getById(id)
      .then(({ data }) => setOrder(data.data))
      .finally(() => setLoading(false));
  };

  useEffect(load, [id]);

  const handleCancel = async () => {
    if (!confirm('Cancel this order? Stock will be restored.')) return;
    setCancelling(true);
    try {
      await orderApi.cancel(id);
      load();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to cancel order.');
    } finally {
      setCancelling(false);
    }
  };

  if (loading) return <Spinner />;
  if (!order) return <p className="text-slate-400 text-center py-10">Order not found.</p>;

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <Link to="/orders" className="text-sm text-indigo-600 hover:underline">
        &larr; Back to orders
      </Link>

      <div className="bg-white rounded-lg shadow-sm border border-slate-100 p-6">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-xl font-bold text-slate-800">{order.orderNumber}</h1>
            <p className="text-sm text-slate-500">{new Date(order.createdAt).toLocaleString()}</p>
          </div>
          <span className="text-xs px-3 py-1 rounded-full bg-slate-100 text-slate-700">{order.status}</span>
        </div>

        <table className="w-full text-sm mb-4">
          <thead className="text-slate-500 text-left border-b border-slate-100">
            <tr>
              <th className="py-2">Product</th>
              <th className="py-2">Unit Price</th>
              <th className="py-2">Qty</th>
              <th className="py-2 text-right">Total</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {order.items.map((it, idx) => (
              <tr key={idx}>
                <td className="py-2">{it.productName}</td>
                <td className="py-2">₹{it.unitPrice.toFixed(2)}</td>
                <td className="py-2">{it.quantity}</td>
                <td className="py-2 text-right">₹{it.lineTotal.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="space-y-1 text-sm text-right">
          <div className="text-slate-500">Subtotal: ₹{order.subTotal.toFixed(2)}</div>
          <div className="text-slate-500">Discount: -₹{order.discountAmount.toFixed(2)}</div>
          <div className="text-slate-500">Tax: ₹{order.taxAmount.toFixed(2)}</div>
          <div className="text-lg font-bold text-slate-800">Total: ₹{order.totalAmount.toFixed(2)}</div>
        </div>

        <div className="flex gap-3 mt-6">
          {order.invoiceId && (
            <button
              onClick={() => downloadInvoicePdf(order.invoiceId, order.invoiceNumber)}
              className="flex-1 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2 rounded-md"
            >
              Download Invoice
            </button>
          )}
          {(order.status === 'Pending' || order.status === 'Confirmed') && (
            <button
              onClick={handleCancel}
              disabled={cancelling}
              className="flex-1 border border-red-300 text-red-600 hover:bg-red-50 disabled:opacity-60 text-sm font-medium py-2 rounded-md"
            >
              {cancelling ? 'Cancelling...' : 'Cancel Order'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
