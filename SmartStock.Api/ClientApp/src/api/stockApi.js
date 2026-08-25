import axiosClient from './axiosClient';

const stockApi = {
  stockIn: (payload) => axiosClient.post('/stock/in', payload),
  adjust: (payload) => axiosClient.post('/stock/adjust', payload),
  getLedger: (productId) => axiosClient.get('/stock/ledger', { params: { productId } }),
  getLowStock: () => axiosClient.get('/stock/low-stock'),
};

export default stockApi;
