import { useEffect, useState } from 'react';
import stockApi from '../../api/stockApi';
import productApi from '../../api/productApi';
import Spinner from '../../components/Spinner';
import Alert from '../../components/Alert';
import Modal from '../../components/Modal';

export default function StockPage() {
  const [products, setProducts] = useState([]);
  const [ledger, setLedger] = useState([]);
  const [lowStock, setLowStock] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [modalMode, setModalMode] = useState(null); // 'in' | 'adjust'
  const [form, setForm] = useState({ productId: '', quantity: '', newQuantity: '', reason: '', referenceNumber: '' });

  const load = () => {
    setLoading(true);
    Promise.all([
      productApi.getAll({ pageSize: 200 }),
      stockApi.getLedger(),
      stockApi.getLowStock(),
    ])
      .then(([p, l, ls]) => {
        setProducts(p.data.data.items);
        setLedger(l.data.data);
        setLowStock(ls.data.data);
      })
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const openModal = (mode) => {
    setModalMode(mode);
    setForm({ productId: '', quantity: '', newQuantity: '', reason: '', referenceNumber: '' });
    setError('');
  };

  const handleSubmit = async () => {
    setError('');
    try {
      if (modalMode === 'in') {
        await stockApi.stockIn({
          productId: Number(form.productId),
          quantity: Number(form.quantity),
          reason: form.reason,
          referenceNumber: form.referenceNumber,
        });
      } else {
        await stockApi.adjust({
          productId: Number(form.productId),
          newQuantity: Number(form.newQuantity),
          reason: form.reason,
        });
      }
      setModalMode(null);
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Operation failed.');
    }
  };

  if (loading) return <Spinner />;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-bold text-slate-800">Stock Management</h1>
        <div className="flex gap-2">
          <button
            onClick={() => openModal('in')}
            className="bg-green-600 hover:bg-green-700 text-white text-sm font-medium px-4 py-2 rounded-md"
          >
            + Stock In
          </button>
          <button
            onClick={() => openModal('adjust')}
            className="bg-amber-600 hover:bg-amber-700 text-white text-sm font-medium px-4 py-2 rounded-md"
          >
            Adjust Stock
          </button>
        </div>
      </div>

      {lowStock.length > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h2 className="text-sm font-semibold text-red-700 mb-2">Low Stock Alerts</h2>
          <div className="flex flex-wrap gap-2">
            {lowStock.map((p) => (
              <span key={p.productId} className="text-xs bg-white border border-red-200 text-red-700 px-2 py-1 rounded-full">
                {p.name} ({p.stockQuantity}/{p.lowStockThreshold})
              </span>
            ))}
          </div>
        </div>
      )}

      <div className="bg-white rounded-lg shadow-sm border border-slate-100 overflow-x-auto">
        <h2 className="px-4 pt-4 text-sm font-semibold text-slate-600">Transaction Ledger</h2>
        <table className="w-full text-sm mt-2">
          <thead className="bg-slate-50 text-slate-600 text-left">
            <tr>
              <th className="px-4 py-3">Date</th>
              <th className="px-4 py-3">Product</th>
              <th className="px-4 py-3">Type</th>
              <th className="px-4 py-3">Qty</th>
              <th className="px-4 py-3">Before → After</th>
              <th className="px-4 py-3">Reason</th>
              <th className="px-4 py-3">By</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {ledger.slice(0, 50).map((t) => (
              <tr key={t.id}>
                <td className="px-4 py-3 text-slate-500">{new Date(t.createdAt).toLocaleString()}</td>
                <td className="px-4 py-3 font-medium text-slate-800">{t.productName}</td>
                <td className="px-4 py-3">
                  <span
                    className={`text-xs px-2 py-0.5 rounded-full ${
                      t.type === 'In'
                        ? 'bg-green-100 text-green-700'
                        : t.type === 'Out'
                        ? 'bg-red-100 text-red-700'
                        : 'bg-amber-100 text-amber-700'
                    }`}
                  >
                    {t.type}
                  </span>
                </td>
                <td className="px-4 py-3 text-slate-600">{t.quantity}</td>
                <td className="px-4 py-3 text-slate-500">
                  {t.stockBeforeTransaction} → {t.stockAfterTransaction}
                </td>
                <td className="px-4 py-3 text-slate-500">{t.reason}</td>
                <td className="px-4 py-3 text-slate-500">{t.performedBy}</td>
              </tr>
            ))}
            {ledger.length === 0 && (
              <tr>
                <td colSpan={7} className="px-4 py-6 text-center text-slate-400">
                  No transactions yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <Modal
        open={!!modalMode}
        title={modalMode === 'in' ? 'Stock In' : 'Adjust Stock'}
        onClose={() => setModalMode(null)}
        footer={
          <>
            <button onClick={() => setModalMode(null)} className="px-4 py-2 text-sm rounded-md border border-slate-300">
              Cancel
            </button>
            <button onClick={handleSubmit} className="px-4 py-2 text-sm rounded-md bg-indigo-600 text-white">
              Submit
            </button>
          </>
        }
      >
        <Alert type="error" message={error} />
        <div className="space-y-3">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Product</label>
            <select
              value={form.productId}
              onChange={(e) => setForm({ ...form, productId: e.target.value })}
              className="w-full px-3 py-2 border border-slate-300 rounded-md text-sm"
            >
              <option value="">Select...</option>
              {products.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} (current: {p.stockQuantity})
                </option>
              ))}
            </select>
          </div>
          {modalMode === 'in' ? (
            <>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Quantity to add</label>
                <input
                  type="number"
                  value={form.quantity}
                  onChange={(e) => setForm({ ...form, quantity: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-md text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Reference Number</label>
                <input
                  value={form.referenceNumber}
                  onChange={(e) => setForm({ ...form, referenceNumber: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-md text-sm"
                />
              </div>
            </>
          ) : (
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">New Quantity</label>
              <input
                type="number"
                value={form.newQuantity}
                onChange={(e) => setForm({ ...form, newQuantity: e.target.value })}
                className="w-full px-3 py-2 border border-slate-300 rounded-md text-sm"
              />
            </div>
          )}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Reason</label>
            <input
              value={form.reason}
              onChange={(e) => setForm({ ...form, reason: e.target.value })}
              className="w-full px-3 py-2 border border-slate-300 rounded-md text-sm"
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}
