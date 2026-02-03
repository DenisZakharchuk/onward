<template>
  <q-page padding>
    <div class="row items-center q-mb-md">
      <div class="text-h4">Stock Movements</div>
      <q-space />
      <q-btn
        color="primary"
        icon="add"
        label="Add Movement"
        @click="openAddDialog"
      />
    </div>

    <q-table
      :rows="movements"
      :columns="columns"
      row-key="id"
      :loading="loading"
      flat
      bordered
      :rows-per-page-options="[10, 25, 50]"
    >
      <template v-slot:body-cell-movementType="props">
        <q-td :props="props">
          <movement-type-badge :movement-type="props.row.movementType" />
        </q-td>
      </template>

      <template v-slot:body-cell-movementDate="props">
        <q-td :props="props">
          {{ formatDate(props.row.movementDate) }}
        </q-td>
      </template>
    </q-table>

    <stock-movement-form
      v-model="showDialog"
      :products="productOptions"
      @submit="handleSubmit"
    />
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import StockMovementForm from '../components/StockMovementForm.vue';
import MovementTypeBadge from '../components/MovementTypeBadge.vue';
import { stockService } from '../services/stockService';
import { productService } from '../services/productService';
import type {
  StockMovement,
  CreateStockMovementDTO,
  Product,
} from '../types';

const $q = useQuasar();

const movements = ref<StockMovement[]>([]);
const loading = ref(false);
const showDialog = ref(false);
const productOptions = ref<
  { label: string; value: string; stock: number }[]
>([]);

const columns = [
  {
    name: 'productName',
    label: 'Product',
    field: 'productName',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'movementType',
    label: 'Type',
    field: 'movementType',
    align: 'center' as const,
    sortable: true,
  },
  {
    name: 'quantity',
    label: 'Quantity',
    field: 'quantity',
    align: 'right' as const,
    sortable: true,
  },
  {
    name: 'movementDate',
    label: 'Date',
    field: 'movementDate',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'notes',
    label: 'Notes',
    field: 'notes',
    align: 'left' as const,
  },
];

onMounted(() => {
  void loadMovements();
  void loadProducts();
});

const loadMovements = async () => {
  loading.value = true;
  try {
    movements.value = await stockService.getAll();
  } catch {
    $q.notify({
      type: 'negative',
      message: 'Failed to load stock movements',
    });
  } finally {
    loading.value = false;
  }
};

const loadProducts = async () => {
  try {
    const products = await productService.getAll();
    productOptions.value = products.map((product: Product) => ({
      label: product.name,
      value: product.id,
      stock: product.currentStock,
    }));
  } catch {
    $q.notify({
      type: 'negative',
      message: 'Failed to load products',
    });
  }
};

const openAddDialog = () => {
  showDialog.value = true;
};

const handleSubmit = async (data: CreateStockMovementDTO) => {
  $q.loading.show();
  try {
    await stockService.create(data);
    $q.notify({
      type: 'positive',
      message: 'Stock movement created successfully',
    });
    await loadMovements();
    await loadProducts(); // Refresh product stock levels
  } catch {
    $q.notify({
      type: 'negative',
      message: 'Failed to create stock movement',
    });
  } finally {
    $q.loading.hide();
  }
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleString();
};
</script>
