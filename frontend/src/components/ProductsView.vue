<template>
  <div class="products-view">
    <div class="header">
      <h1>Products</h1>
      <button @click="showAddForm = true" class="btn-primary">Add Product</button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>

    <div v-if="loading" class="loading">Loading...</div>

    <div v-else class="products-grid">
      <div v-for="product in products" :key="product.id" class="product-card">
        <h3>{{ product.name }}</h3>
        <p class="description">{{ product.description || 'No description' }}</p>
        <div class="details">
          <p><strong>SKU:</strong> {{ product.sku || 'N/A' }}</p>
          <p><strong>Price:</strong> ${{ product.price.toFixed(2) }}</p>
          <p><strong>Category:</strong> {{ product.categoryName }}</p>
          <p><strong>Stock:</strong> {{ product.currentStock }} 
            <span v-if="product.currentStock <= product.minimumStock" class="low-stock">⚠️ Low</span>
          </p>
          <p><strong>Min Stock:</strong> {{ product.minimumStock }}</p>
        </div>
        <div class="actions">
          <button @click="editProduct(product)" class="btn-edit">Edit</button>
          <button @click="deleteProduct(product.id)" class="btn-delete">Delete</button>
        </div>
      </div>
    </div>

    <div v-if="showAddForm" class="modal">
      <div class="modal-content">
        <h2>{{ editingProduct ? 'Edit Product' : 'Add Product' }}</h2>
        <form @submit.prevent="submitProduct">
          <div class="form-group">
            <label>Name *</label>
            <input v-model="form.name" required />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description"></textarea>
          </div>
          <div class="form-group">
            <label>SKU</label>
            <input v-model="form.sku" />
          </div>
          <div class="form-group">
            <label>Price *</label>
            <input v-model.number="form.price" type="number" step="0.01" required />
          </div>
          <div class="form-group">
            <label>Category *</label>
            <select v-model="form.categoryId" required>
              <option value="">Select category</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                {{ cat.name }}
              </option>
            </select>
          </div>
          <div class="form-group" v-if="!editingProduct">
            <label>Initial Stock *</label>
            <input v-model.number="form.initialStock" type="number" required />
          </div>
          <div class="form-group">
            <label>Minimum Stock *</label>
            <input v-model.number="form.minimumStock" type="number" required />
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-primary">{{ editingProduct ? 'Update' : 'Create' }}</button>
            <button type="button" @click="closeForm" class="btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { productService } from '../services/productService';
import { categoryService } from '../services/categoryService';
import type { Product, CreateProduct, UpdateProduct, Category } from '../types';

const products = ref<Product[]>([]);
const categories = ref<Category[]>([]);
const loading = ref(false);
const error = ref('');
const showAddForm = ref(false);
const editingProduct = ref<Product | null>(null);

const form = ref({
  name: '',
  description: '',
  sku: '',
  price: 0,
  categoryId: '',
  initialStock: 0,
  minimumStock: 0,
});

const loadProducts = async () => {
  loading.value = true;
  try {
    products.value = await productService.getAll();
  } catch (e) {
    error.value = 'Failed to load products';
  } finally {
    loading.value = false;
  }
};

const loadCategories = async () => {
  try {
    categories.value = await categoryService.getAll();
  } catch (e) {
    error.value = 'Failed to load categories';
  }
};

const editProduct = (product: Product) => {
  editingProduct.value = product;
  form.value = {
    name: product.name,
    description: product.description || '',
    sku: product.sku || '',
    price: product.price,
    categoryId: product.categoryId,
    initialStock: product.currentStock,
    minimumStock: product.minimumStock,
  };
  showAddForm.value = true;
};

const submitProduct = async () => {
  try {
    if (editingProduct.value) {
      const updateData: UpdateProduct = {
        name: form.value.name,
        description: form.value.description,
        sku: form.value.sku,
        price: form.value.price,
        categoryId: form.value.categoryId,
        minimumStock: form.value.minimumStock,
      };
      await productService.update(editingProduct.value.id, updateData);
    } else {
      const createData: CreateProduct = {
        name: form.value.name,
        description: form.value.description,
        sku: form.value.sku,
        price: form.value.price,
        categoryId: form.value.categoryId,
        initialStock: form.value.initialStock,
        minimumStock: form.value.minimumStock,
      };
      await productService.create(createData);
    }
    closeForm();
    loadProducts();
  } catch (e) {
    error.value = 'Failed to save product';
  }
};

const deleteProduct = async (id: string) => {
  if (confirm('Are you sure you want to delete this product?')) {
    try {
      await productService.delete(id);
      loadProducts();
    } catch (e) {
      error.value = 'Failed to delete product';
    }
  }
};

const closeForm = () => {
  showAddForm.value = false;
  editingProduct.value = null;
  form.value = {
    name: '',
    description: '',
    sku: '',
    price: 0,
    categoryId: '',
    initialStock: 0,
    minimumStock: 0,
  };
};

onMounted(() => {
  loadProducts();
  loadCategories();
});
</script>

<style scoped>
.products-view {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.product-card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  background: white;
  color: #333;
}

.product-card h3 {
  margin-top: 0;
  color: #333;
}

.description {
  color: #666;
  font-size: 14px;
}

.details p {
  margin: 8px 0;
}

.low-stock {
  color: #ff6b6b;
  font-weight: bold;
}

.actions {
  margin-top: 16px;
  display: flex;
  gap: 8px;
}

.error {
  padding: 12px;
  background: #fee;
  color: #c00;
  border-radius: 4px;
  margin-bottom: 20px;
}

.loading {
  text-align: center;
  padding: 40px;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-content {
  background: white;
  color: #333;
  padding: 24px;
  border-radius: 8px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  margin-bottom: 4px;
  font-weight: 500;
}

.form-group input,
.form-group textarea,
.form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-group textarea {
  min-height: 80px;
  resize: vertical;
}

.form-actions {
  display: flex;
  gap: 8px;
  margin-top: 20px;
}

.btn-primary {
  background: #4CAF50;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-primary:hover {
  background: #45a049;
}

.btn-secondary {
  background: #ddd;
  color: #333;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-edit {
  background: #2196F3;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-delete {
  background: #f44336;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}
</style>
