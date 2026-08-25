import { useEffect, useState } from 'react';
import supplierApi from '../../api/supplierApi';
import Spinner from '../../components/Spinner';
import Alert from '../../components/Alert';
import Modal from '../../components/Modal';

const emptyForm = { name: '', contactPerson: '', email: '', phoneNumber: '', address: '' };

export default function SuppliersPage() {
  const [suppliers, setSuppliers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const load = () => {
    setLoading(true);
    supplierApi
      .getAll()
      .then(({ data }) => setSuppliers(data.data))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const openCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setError('');
    setModalOpen(true);
  };

  const openEdit = (s) => {
    setEditingId(s.id);
    setForm({
      name: s.name,
      contactPerson: s.contactPerson || '',
      email: s.email || '',
      phoneNumber: s.phoneNumber || '',
      address: s.address || '',
    });
    setError('');
    setModalOpen(true);
  };

  const handleSave = async () => {
    setError('');
    try {
      if (editingId) {
        await supplierApi.update(editingId, form);
      } else {
        await supplierApi.create(form);
      }
      setModalOpen(false);
      load();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save supplier.');
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Delete this supplier?')) return;
    try {
      await supplierApi.remove(id);
      load();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to delete supplier.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-800">Suppliers</h1>
        <button
          onClick={openCreate}
          className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-md"
        >
          + New Supplier
        </button>
      </div>

      {loading ? (
        <Spinner />
      ) : (
        <div className="bg-white rounded-lg shadow-sm border border-slate-100 overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 text-slate-600 text-left">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Contact</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Phone</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {suppliers.map((s) => (
                <tr key={s.id}>
                  <td className="px-4 py-3 font-medium text-slate-800">{s.name}</td>
                  <td className="px-4 py-3 text-slate-500">{s.contactPerson}</td>
                  <td className="px-4 py-3 text-slate-500">{s.email}</td>
                  <td className="px-4 py-3 text-slate-500">{s.phoneNumber}</td>
                  <td className="px-4 py-3 text-right space-x-2">
                    <button onClick={() => openEdit(s)} className="text-indigo-600 hover:underline">
                      Edit
                    </button>
                    <button onClick={() => handleDelete(s.id)} className="text-red-600 hover:underline">
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
              {suppliers.length === 0 && (
                <tr>
                  <td colSpan={5} className="px-4 py-6 text-center text-slate-400">
                    No suppliers yet.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <Modal
        open={modalOpen}
        title={editingId ? 'Edit Supplier' : 'New Supplier'}
        onClose={() => setModalOpen(false)}
        footer={
          <>
            <button onClick={() => setModalOpen(false)} className="px-4 py-2 text-sm rounded-md border border-slate-300">
              Cancel
            </button>
            <button onClick={handleSave} className="px-4 py-2 text-sm rounded-md bg-indigo-600 text-white">
              Save
            </button>
          </>
        }
      >
        <Alert type="error" message={error} />
        <div className="space-y-3">
          {[
            ['name', 'Name'],
            ['contactPerson', 'Contact Person'],
            ['email', 'Email'],
            ['phoneNumber', 'Phone Number'],
            ['address', 'Address'],
          ].map(([key, label]) => (
            <div key={key}>
              <label className="block text-sm font-medium text-slate-700 mb-1">{label}</label>
              <input
                value={form[key]}
                onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                className="w-full px-3 py-2 border border-slate-300 rounded-md text-sm"
              />
            </div>
          ))}
        </div>
      </Modal>
    </div>
  );
}
