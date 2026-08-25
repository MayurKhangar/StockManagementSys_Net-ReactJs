import { useEffect, useState } from 'react';
import invoiceApi from '../../api/invoiceApi';
import Spinner from '../../components/Spinner';
import { downloadInvoicePdf } from '../../utils/downloadPdf';

export default function AdminInvoicesPage() {
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    invoiceApi
      .getAll()
      .then(({ data }) => setInvoices(data.data))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-slate-800">Invoices</h1>

      <div className="bg-white rounded-lg shadow-sm border border-slate-100 overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-600 text-left">
            <tr>
              <th className="px-4 py-3">Invoice #</th>
              <th className="px-4 py-3">Order #</th>
              <th className="px-4 py-3">Customer</th>
              <th className="px-4 py-3">Date</th>
              <th className="px-4 py-3">Total</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {invoices.map((inv) => (
              <tr key={inv.id}>
                <td className="px-4 py-3 font-medium text-slate-800">{inv.invoiceNumber}</td>
                <td className="px-4 py-3 text-slate-500">{inv.orderNumber}</td>
                <td className="px-4 py-3 text-slate-500">{inv.customerName}</td>
                <td className="px-4 py-3 text-slate-500">{new Date(inv.issueDate).toLocaleDateString()}</td>
                <td className="px-4 py-3 text-slate-700">₹{inv.totalAmount.toFixed(2)}</td>
                <td className="px-4 py-3 text-right">
                  <button
                    onClick={() => downloadInvoicePdf(inv.id, inv.invoiceNumber)}
                    className="text-indigo-600 hover:underline"
                  >
                    Download PDF
                  </button>
                </td>
              </tr>
            ))}
            {invoices.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-6 text-center text-slate-400">
                  No invoices yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
