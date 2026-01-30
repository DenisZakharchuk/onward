<template>
  <div class="stock-view">
    <div class="header">
      <h1>Stock Movements</h1>
      <button @click="showAddForm = true" class="btn-primary">Add Movement</button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>
    <div v-if="loading" class="loading">Loading...</div>

    <div v-else class="movements-table">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Product</th>
            <th>Type</th>
            <th>Quantity</th>
            <th>Notes</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="movement in movements" :key="movement.id">
            <td>{{ formatDate(movement.createdAt) }}</td>
            <td>{{ movement.productName }}</td>
            <td>
              <span :class="'type-badge type-' + getMovementTypeName(movement.type)">
                {{ getMovementTypeName(movement.type) }}
              </span>
            </td>
            <td>{{ movement.quantity }}</td>
            <td>{{ movement.notes || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showAddForm" class="modal">
      <div class="modal-content">
        <h2>Add Stock Movement</h2>
        <form @submit.prevent="submitMovement">
          <div class="form-group">
            <label>Product *</label>
            <select v-model="form.productId" required>
              <option value="">Select product</option>
              <option v-for="product in products" :key="product.id" :value="product.id">
                {{ product.name }} (Current: {{ product.currentStock }})
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Type *</label>
            <select v-model.number="form.type" required>
              <option :value="0">In</option>
              <option :value="1">Out</option>
              <option :value="2">Adjustment</option>
            </select>
          </div>
          <div class="form-group">
            <label>Quantity *</label>
            <input v-model.number="form.quantity" type="number" required />
          </div>
          <div class="form-group">
            <label>Notes</label>
            <textarea v-model="form.notes"></textarea>
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-primary">Create</button>
            <button type="button" @click="closeForm" class="btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { stockService } from '../services/stockService';
import { productService } from '../services/productService';
import type { StockMovement, CreateStockMovement, Product, MovementType } from '../types';
import { MovementTypeNames } from '../types';

const movements = ref<StockMovement[]>([]);
const products = ref<Product[]>([]);
const loading = ref(false);
const error = ref('');
const showAddForm = ref(false);

const form = ref({
  productId: '',
  type: 0 as MovementType,
  quantity: 0,
  notes: '',
});

const loadMovements = async () => {
  loading.value = true;
  try {
    movements.value = await stockService.getAll();
  } catch (e) {
    error.value = 'Failed to load stock movements';
  } finally {
    loading.value = false;
  }
};

const loadProducts = async () => {
  try {
    products.value = await productService.getAll();
  } catch (e) {
    error.value = 'Failed to load products';
  }
};

const submitMovement = async () => {
  try {
    const data: CreateStockMovement = {
      productId: form.value.productId,
      type: form.value.type,
      quantity: form.value.quantity,
      notes: form.value.notes,
    };
    await stockService.create(data);
    closeForm();
    loadMovements();
  } catch (e) {
    error.value = 'Failed to create stock movement';
  }
};

const closeForm = () => {
  showAddForm.value = false;
  form.value = {
    productId: '',
    type: 0 as MovementType,
    quantity: 0,
    notes: '',
  };
};

const getMovementTypeName = (type: MovementType): string => {
  return MovementTypeNames[type].toLowerCase();
};

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleString();
};

onMounted(() => {
  loadMovements();
  loadProducts();
});
</script>

<style scoped>
.stock-view {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.movements-table {
  background: white;
  color: #333;
  border-radius: 8px;
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #f5f5f5;
}

th, td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

th {
  font-weight: 600;
}

.type-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.type-in {
  background: #e8f5e9;
  color: #2e7d32;
}

.type-out {
  background: #ffebee;
  color: #c62828;
}

.type-adjustment {
  background: #e3f2fd;
  color: #1565c0;
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

.btn-secondary {
  background: #ddd;
  color: #333;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}
</style>
