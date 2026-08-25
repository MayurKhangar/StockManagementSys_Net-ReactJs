import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../../context/CartContext';
import orderApi from '../../api/orderApi';
import Alert from '../../components/Alert';

export default function CartPage() {
  const { items, updateQuantity, removeItem, clearCart, subTotal } = useCart();
  const [placing, setPlacing] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handlePlaceOrder = async () => {
    setError('');
    setPlacing(true);
    try {
      const { data } = await orderApi.placeOrder({
        items: items.map((i) => ({ productId: i.productId, quantity: i.quantity })),
        discountAmount: 0,
      });
      clearCart();
      navigate(`/orders/${data.data.id}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to place order.');
    } finally {
      setPlacing(false);
    }
  };

  if (items.length === 0) {
    return (
      <div className="text-center py-16">
        <p className="text-slate-500">Your cart is empty.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <h1 className="text-2xl font-bold text-slate-800">Your Cart</h1>

      <Alert type="error" message={error} />

      <div className="bg-white rounded-lg shadow-sm border border-slate-100 divide-y divide-slate-100">
        {items.map((item) => (
          <div key={item.productId} className="flex items-center justify-between p-4">
            <div>
              <div className="font-medium text-slate-800">{item.name}</div>
              <div className="text-sm text-slate-500">₹{item.price.toFixed(2)} each</div>
            </div>
            <div className="flex items-center gap-3">
              <input
                type="number"
                min={1}
                max={item.stockQuantity}
                value={item.quantity}
                onChange={(e) => updateQuantity(item.productId, Number(e.target.value))}
                className="w-16 px-2 py-1 border border-slate-300 rounded-md text-sm text-center"
              />
              <span className="w-20 text-right text-sm text-slate-700">
                ₹{(item.price * item.quantity).toFixed(2)}
              </span>
              <button
                onClick={() => removeItem(item.productId)}
                className="text-red-500 hover:underline text-sm"
              >
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-slate-100 p-4 flex items-center justify-between">
        <span className="text-slate-600">Subtotal ({items.length} items)</span>
        <span className="text-xl font-bold text-slate-800">₹{subTotal.toFixed(2)}</span>
      </div>

      <button
        onClick={handlePlaceOrder}
        disabled={placing}
        className="w-full bg-indigo-600 hover:bg-indigo-700 disabled:opacity-60 text-white font-medium py-3 rounded-md transition-colors"
      >
        {placing ? 'Placing order...' : 'Place Order'}
      </button>
    </div>
  );
}
