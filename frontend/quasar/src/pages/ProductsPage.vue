<template>
  <q-page padding>
    <div class="row items-center q-mb-md">
      <div class="text-h4">Products</div>
      <q-space />
      <q-btn color="primary" icon="add" label="Add Product" @click="openAddDialog" />
    </div>

    <q-table
      :rows="products"
      :columns="columns"
      row-key="id"
      :loading="loading"
      flat
      bordered
      :rows-per-page-options="[10, 25, 50]"
    >
      <template v-slot:body-cell-currentStock="props">
        <q-td :props="props">
          {{ props.row.currentStock }}
          <low-stock-badge
            :current-stock="props.row.currentStock"
            :min-stock-level="props.row.minStockLevel"
            class="q-ml-sm"
          />
        </q-td>
      </template>

      <template v-slot:body-cell-price="props">
        <q-td :props="props"> ${{ props.row.price.toFixed(2) }} </q-td>
      </template>

      <template v-slot:body-cell-actions="props">
        <q-td :props="props">
          <q-btn
            flat
            dense
            round
            color="primary"
            icon="edit"
            @click="openEditDialog(props.row)"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn
            flat
            dense
            round
            color="negative"
            icon="delete"
            @click="confirmDelete(props.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>

    <product-form
      v-model="showDialog"
      :product="selectedProduct"
      :categories="categoryOptions"
      @submit="handleSubmit"
    />
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import ProductForm from '../components/ProductForm.vue';
import LowStockBadge from '../components/LowStockBadge.vue';
import { productService } from '../services/productService';
import { categoryService } from '../services/categoryService';
import type {
  Product,
  CreateProductDTO,
  UpdateProductDTO,
  Category,
} from '../types';

const $q = useQuasar();

const products = ref<Product[]>([]);
const loading = ref(false);
const showDialog = ref(false);
const selectedProduct = ref<Product | undefined>(undefined);
const categoryOptions = ref<{ label: string; value: string }[]>([]);

const columns = [
  {
    name: 'name',
    label: 'Name',
    field: 'name',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'sku',
    label: 'SKU',
    field: 'sku',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'price',
    label: 'Price',
    field: 'price',
    align: 'right' as const,
    sortable: true,
  },
  {
    name: 'currentStock',
    label: 'Stock',
    field: 'currentStock',
    align: 'center' as const,
    sortable: true,
  },
  {
    name: 'minStockLevel',
    label: 'Min Stock',
    field: 'minStockLevel',
    align: 'center' as const,
    sortable: true,
  },
  {
    name: 'actions',
    label: 'Actions',
    field: 'actions',
    align: 'center' as const,
  },
];

onMounted(() => {
  loadProducts();
  loadCategories();
});

const loadProducts = async () => {
  loading.value = true;
  try {
    products.value = await productService.getAll();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load products',
    });
  } finally {
    loading.value = false;
  }
};

const loadCategories = async () => {
  try {
    const categories = await categoryService.getAll();
    categoryOptions.value = categories.map((cat: Category) => ({
      label: cat.name,
      value: cat.id,
    }));
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load categories',
    });
  }
};

const openAddDialog = () => {
  selectedProduct.value = undefined;
  showDialog.value = true;
};

const openEditDialog = (product: Product) => {
  selectedProduct.value = product;
  showDialog.value = true;
};

const handleSubmit = async (
  data: CreateProductDTO | UpdateProductDTO
) => {
  $q.loading.show();
  try {
    if (selectedProduct.value) {
      await productService.update(selectedProduct.value.id, data);
      $q.notify({
        type: 'positive',
        message: 'Product updated successfully',
      });
    } else {
      await productService.create(data as CreateProductDTO);
      $q.notify({
        type: 'positive',
        message: 'Product created successfully',
      });
    }
    await loadProducts();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to save product',
    });
  } finally {
    $q.loading.hide();
  }
};

const confirmDelete = (product: Product) => {
  $q.dialog({
    title: 'Confirm Delete',
    message: `Are you sure you want to delete "${product.name}"?`,
    cancel: true,
    persistent: true,
  }).onOk(async () => {
    $q.loading.show();
    try {
      await productService.delete(product.id);
      $q.notify({
        type: 'positive',
        message: 'Product deleted successfully',
      });
      await loadProducts();
    } catch (error) {
      $q.notify({
        type: 'negative',
        message: 'Failed to delete product',
      });
    } finally {
      $q.loading.hide();
    }
  });
};
</script>
