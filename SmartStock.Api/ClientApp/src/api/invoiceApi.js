import axiosClient from './axiosClient';

const invoiceApi = {
  getAll: () => axiosClient.get('/invoices'),
  getById: (id) => axiosClient.get(`/invoices/${id}`),
  getByOrderId: (orderId) => axiosClient.get(`/invoices/order/${orderId}`),
  downloadPdf: (id) => axiosClient.get(`/invoices/${id}/pdf`, { responseType: 'blob' }),
};

export default invoiceApi;
