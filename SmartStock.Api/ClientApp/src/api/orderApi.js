import axiosClient from './axiosClient';

const orderApi = {
  placeOrder: (payload) => axiosClient.post('/orders', payload),
  getMyOrders: () => axiosClient.get('/orders/my'),
  getAll: () => axiosClient.get('/orders'),
  getById: (id) => axiosClient.get(`/orders/${id}`),
  cancel: (id) => axiosClient.post(`/orders/${id}/cancel`),
};

export default orderApi;
