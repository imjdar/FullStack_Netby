import axios from 'axios';

// Cuando se ejecuta dentro de Docker con Nginx reverse proxy,
// las URLs son relativas (mismo origen). El proxy de Nginx redirige
// /api/products -> inventory.products.api:8080
// /api/transactions -> inventory.transactions.api:8080
//
// Cuando se ejecuta en desarrollo local (npm run dev), se usan las
// URLs directas de localhost con los puertos mapeados de Docker.
const productsApi = axios.create({
  baseURL: import.meta.env.VITE_API_PRODUCTS_URL || '',
});

const transactionsApi = axios.create({
  baseURL: import.meta.env.VITE_API_TRANSACTIONS_URL || '',
});

export const api = {
  products: {
    getAll: () => productsApi.get('/api/products'),
    getById: (id: number) => productsApi.get(`/api/products/${id}`),
    create: (formData: FormData) => productsApi.post('/api/products', formData),
    update: (id: number, formData: FormData) => productsApi.put(`/api/products/${id}`, formData),
    delete: (id: number) => productsApi.delete(`/api/products/${id}`),
  },
  transactions: {
    getAll: (params?: any) => transactionsApi.get('/api/transactions', { params }),
    create: (data: any) => transactionsApi.post('/api/transactions', data),
  }
};
