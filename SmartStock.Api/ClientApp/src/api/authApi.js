import axiosClient from './axiosClient';

const authApi = {
  register: (payload) => axiosClient.post('/auth/register', payload),
  login: (payload) => axiosClient.post('/auth/login', payload),
  refreshToken: (payload) => axiosClient.post('/auth/refresh-token', payload),
  revokeToken: () => axiosClient.post('/auth/revoke-token'),
};

export default authApi;
