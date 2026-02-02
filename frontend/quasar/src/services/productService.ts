import api from './api';
import type { Product, CreateProduct, UpdateProduct } from '../types';

export const productService = {
  async getAll(): Promise<Product[]> {
    const response = await api.get('/products');
    return response.data;
  },

  async getById(id: string): Promise<Product> {
    const response = await api.get(`/products/${id}`);
    return response.data;
  },

  async getLowStock(): Promise<Product[]> {
    const response = await api.get('/products/low-stock');
    return response.data;
  },

  async create(product: CreateProduct): Promise<Product> {
    const response = await api.post('/products', product);
    return response.data;
  },

  async update(id: string, product: UpdateProduct): Promise<Product> {
    const response = await api.put(`/products/${id}`, product);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/products/${id}`);
  },
};
