export interface Product {
  id: string;
  name: string;
  description?: string;
  sku?: string;
  price: number;
  categoryId: string;
  categoryName: string;
  currentStock: number;
  minimumStock: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProduct {
  name: string;
  description?: string;
  sku?: string;
  price: number;
  categoryId: string;
  initialStock: number;
  minimumStock: number;
}

export interface UpdateProduct {
  name: string;
  description?: string;
  sku?: string;
  price: number;
  categoryId: string;
  minimumStock: number;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCategory {
  name: string;
  description?: string;
}

export type MovementType = 0 | 1 | 2; // 0 = In, 1 = Out, 2 = Adjustment

export const MovementTypeNames = {
  0: 'In',
  1: 'Out',
  2: 'Adjustment',
} as const;

export interface StockMovement {
  id: string;
  productId: string;
  productName: string;
  type: MovementType;
  quantity: number;
  notes?: string;
  createdAt: string;
}

export interface CreateStockMovement {
  productId: string;
  type: MovementType;
  quantity: number;
  notes?: string;
}
