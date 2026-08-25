import { Link } from 'react-router-dom';

export default function NotFoundPage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-slate-50 text-center px-4">
      <h1 className="text-3xl font-bold text-slate-800 mb-2">404</h1>
      <p className="text-slate-500 mb-6">This page doesn't exist.</p>
      <Link to="/" className="text-indigo-600 font-medium hover:underline">
        Go back home
      </Link>
    </div>
  );
}
