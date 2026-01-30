<template>
  <div class="categories-view">
    <div class="header">
      <h1>Categories</h1>
      <button @click="showAddForm = true" class="btn-primary">Add Category</button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>
    <div v-if="loading" class="loading">Loading...</div>

    <div v-else class="categories-list">
      <div v-for="category in categories" :key="category.id" class="category-card">
        <h3>{{ category.name }}</h3>
        <p>{{ category.description || 'No description' }}</p>
        <div class="actions">
          <button @click="editCategory(category)" class="btn-edit">Edit</button>
          <button @click="deleteCategory(category.id)" class="btn-delete">Delete</button>
        </div>
      </div>
    </div>

    <div v-if="showAddForm" class="modal">
      <div class="modal-content">
        <h2>{{ editingCategory ? 'Edit Category' : 'Add Category' }}</h2>
        <form @submit.prevent="submitCategory">
          <div class="form-group">
            <label>Name *</label>
            <input v-model="form.name" required />
          </div>
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="form.description"></textarea>
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-primary">{{ editingCategory ? 'Update' : 'Create' }}</button>
            <button type="button" @click="closeForm" class="btn-secondary">Cancel</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { categoryService } from '../services/categoryService';
import type { Category, CreateCategory } from '../types';

const categories = ref<Category[]>([]);
const loading = ref(false);
const error = ref('');
const showAddForm = ref(false);
const editingCategory = ref<Category | null>(null);

const form = ref({
  name: '',
  description: '',
});

const loadCategories = async () => {
  loading.value = true;
  try {
    categories.value = await categoryService.getAll();
  } catch (e) {
    error.value = 'Failed to load categories';
  } finally {
    loading.value = false;
  }
};

const editCategory = (category: Category) => {
  editingCategory.value = category;
  form.value = {
    name: category.name,
    description: category.description || '',
  };
  showAddForm.value = true;
};

const submitCategory = async () => {
  try {
    const data: CreateCategory = {
      name: form.value.name,
      description: form.value.description,
    };

    if (editingCategory.value) {
      await categoryService.update(editingCategory.value.id, data);
    } else {
      await categoryService.create(data);
    }
    closeForm();
    loadCategories();
  } catch (e) {
    error.value = 'Failed to save category';
  }
};

const deleteCategory = async (id: string) => {
  if (confirm('Are you sure you want to delete this category?')) {
    try {
      await categoryService.delete(id);
      loadCategories();
    } catch (e) {
      error.value = 'Failed to delete category';
    }
  }
};

const closeForm = () => {
  showAddForm.value = false;
  editingCategory.value = null;
  form.value = {
    name: '',
    description: '',
  };
};

onMounted(() => {
  loadCategories();
});
</script>

<style scoped>
.categories-view {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.categories-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.category-card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  background: white;
  color: #333;
}

.category-card h3 {
  margin-top: 0;
  color: #333;
}

.category-card p {
  color: #666;
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
.form-group textarea {
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
