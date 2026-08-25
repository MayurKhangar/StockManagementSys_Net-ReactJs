import axiosClient from './axiosClient';

const reportApi = {
  getDashboard: () => axiosClient.get('/reports/dashboard'),
  getStockValuation: () => axiosClient.get('/reports/stock-valuation'),
  getTopProducts: (count) => axiosClient.get('/reports/top-products', { params: { count } }),
  getSalesTrend: (days) => axiosClient.get('/reports/sales-trend', { params: { days } }),
};

export default reportApi;
