import { api } from './client';

export const authApi = {
  login: (email, password) =>
    api.post('/api/auth/login', { email, password }),

  register: (fullName, email, password) =>
    api.post('/api/auth/register', { fullName, email, password }),
};

export const productsApi = {
  getAll: (params) => api.get('/api/products', { params }),
  getById: (id) => api.get(`/api/products/${id}`),
  getCategories: () => api.get('/api/products/categories'),
};

export const ordersApi = {
  checkout: (shippingAddress, phoneNumber, items) =>
    api.post('/api/orders/checkout', { shippingAddress, phoneNumber, items }),

  getMyOrders: () => api.get('/api/orders/my'),
};
