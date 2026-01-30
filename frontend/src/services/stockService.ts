import api from './api';
import type { StockMovement, CreateStockMovement } from '../types';

export const stockService = {
  async getAll(): Promise<StockMovement[]> {
    const response = await api.get('/stock');
    return response.data;
  },

  async getByProductId(productId: string): Promise<StockMovement[]> {
    const response = await api.get(`/stock/product/${productId}`);
    return response.data;
  },

  async create(movement: CreateStockMovement): Promise<StockMovement> {
    const response = await api.post('/stock', movement);
    return response.data;
  },
};
