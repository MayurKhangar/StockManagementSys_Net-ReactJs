import { useEffect, useState } from 'react';
import productApi from '../../api/productApi';
import categoryApi from '../../api/categoryApi';
import { useCart } from '../../context/CartContext';
import Spinner from '../../components/Spinner';

export default function ShopPage() {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [loading, setLoading] = useState(true);
  const { addItem } = useCart();
  const [addedId, setAddedId] = useState(null);

  const load = () => {
    setLoading(true);
    productApi
      .getAll({ search, categoryId: categoryId || undefined, activeOnly: true, pageSize: 60 })
      .then(({ data }) => setProducts(data.data.items))
      .finally(() => setLoading(false));
  };

  useEffect(load, [categoryId]);
  useEffect(() => {
    categoryApi.getAll().then(({ data }) => setCategories(data.data));
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    load();
  };

  const handleAdd = (product) => {
    addItem(product, 1);
    setAddedId(product.id);
    setTimeout(() => setAddedId(null), 1200);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-bold text-slate-800">Shop</h1>
        <div className="flex gap-2">
          <form onSubmit={handleSearch}>
            <input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search products..."
              className="px-3 py-2 border border-slate-300 rounded-md text-sm w-56"
            />
          </form>
          <select
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            className="px-3 py-2 border border-slate-300 rounded-md text-sm"
          >
            <option value="">All categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
      </div>

      {loading ? (
        <Spinner />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {products.map((p) => (
            <div key={p.id} className="bg-white rounded-lg shadow-sm border border-slate-100 p-4 flex flex-col">
              <div className="text-xs text-slate-400">{p.categoryName}</div>
              <div className="font-semibold text-slate-800 mt-1">{p.name}</div>
              <div className="text-sm text-slate-500 line-clamp-2 flex-1 mt-1">{p.description}</div>
              <div className="flex items-center justify-between mt-3">
                <span className="text-lg font-bold text-indigo-600">₹{p.price.toFixed(2)}</span>
                <span className={`text-xs ${p.stockQuantity === 0 ? 'text-red-500' : 'text-slate-400'}`}>
                  {p.stockQuantity === 0 ? 'Out of stock' : `${p.stockQuantity} in stock`}
                </span>
              </div>
              <button
                disabled={p.stockQuantity === 0}
                onClick={() => handleAdd(p)}
                className="mt-3 w-full bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-medium py-2 rounded-md transition-colors"
              >
                {addedId === p.id ? 'Added ✓' : 'Add to cart'}
              </button>
            </div>
          ))}
          {products.length === 0 && <p className="text-slate-400 col-span-full text-center py-10">No products found.</p>}
        </div>
      )}
    </div>
  );
}
