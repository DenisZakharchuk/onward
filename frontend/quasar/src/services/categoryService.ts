import api from './api';
import type { Category, CreateCategory } from '../types';

export const categoryService = {
  async getAll(): Promise<Category[]> {
    const response = await api.get('/categories');
    return response.data;
  },

  async getById(id: string): Promise<Category> {
    const response = await api.get(`/categories/${id}`);
    return response.data;
  },

  async create(category: CreateCategory): Promise<Category> {
    const response = await api.post('/categories', category);
    return response.data;
  },

  async update(id: string, category: CreateCategory): Promise<Category> {
    const response = await api.put(`/categories/${id}`, category);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/categories/${id}`);
  },
};
