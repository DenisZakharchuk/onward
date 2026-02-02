export interface Product {
  id: string;
  name: string;
  description?: string;
  sku?: string;
  price: number;
  categoryId: string;
  categoryName: string;
  currentStock: number;
  minStockLevel: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProduct {
  name: string;
  description?: string;
  sku: string;
  price: number;
  categoryId: string;
  currentStock: number;
  minStockLevel: number;
}

export interface UpdateProduct {
  name: string;
  description?: string;
  sku: string;
  price: number;
  categoryId: string;
  currentStock: number;
  minStockLevel: number;
}

export type CreateProductDTO = CreateProduct;
export type UpdateProductDTO = UpdateProduct;

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

export interface UpdateCategory {
  name: string;
  description?: string;
}

export type CreateCategoryDTO = CreateCategory;
export type UpdateCategoryDTO = UpdateCategory;

export type MovementType = 0 | 1 | 2;

export const MovementTypeNames = {
  0: 'In',
  1: 'Out',
  2: 'Adjustment',
} as const;

export interface StockMovement {
  id: string;
  productId: string;
  productName: string;
  movementType: MovementType;
  quantity: number;
  notes?: string;
  movementDate: string;
}

export interface CreateStockMovement {
  productId: string;
  movementType: number;
  quantity: number;
  notes?: string;
}

export type CreateStockMovementDTO = CreateStockMovement;
