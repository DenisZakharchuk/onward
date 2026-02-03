<template>
  <q-dialog v-model="showDialog" @hide="onClose">
    <q-card style="min-width: 500px">
      <q-card-section>
        <div class="text-h6">{{ isEdit ? 'Edit Product' : 'Add Product' }}</div>
      </q-card-section>

      <q-card-section>
        <q-form @submit="onSubmit">
          <q-input
            v-model="formData.name"
            label="Product Name *"
            :rules="[(val) => !!val || 'Name is required']"
            outlined
            class="q-mb-md"
          />

          <q-input
            v-model="formData.description"
            label="Description"
            type="textarea"
            outlined
            rows="3"
            class="q-mb-md"
          />

          <q-input
            v-model="formData.sku"
            label="SKU *"
            :rules="[(val) => !!val || 'SKU is required']"
            outlined
            class="q-mb-md"
          />

          <q-input
            v-model.number="formData.price"
            label="Price *"
            type="number"
            step="0.01"
            min="0"
            :rules="[(val) => val >= 0 || 'Price must be positive']"
            outlined
            class="q-mb-md"
          />

          <q-select
            v-model="formData.categoryId"
            :options="categoryOptions"
            option-value="value"
            option-label="label"
            emit-value
            map-options
            label="Category *"
            :rules="[(val) => !!val || 'Category is required']"
            outlined
            class="q-mb-md"
          />

          <div class="row q-col-gutter-md q-mb-md">
            <div class="col">
              <q-input
                v-model.number="formData.minStockLevel"
                label="Min Stock Level *"
                type="number"
                min="0"
                :rules="[(val) => val >= 0 || 'Must be positive']"
                outlined
              />
            </div>
            <div class="col">
              <q-input
                v-model.number="formData.currentStock"
                label="Current Stock *"
                type="number"
                min="0"
                :rules="[(val) => val >= 0 || 'Must be positive']"
                outlined
              />
            </div>
          </div>

          <div class="row q-gutter-sm justify-end">
            <q-btn label="Cancel" color="grey" flat @click="onClose" />
            <q-btn label="Save" type="submit" color="primary" />
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { Product, CreateProductDTO, UpdateProductDTO } from '../types';

interface CategoryOption {
  label: string;
  value: string;
}

interface Props {
  modelValue: boolean;
  product?: Product | undefined;
  categories: CategoryOption[];
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', data: CreateProductDTO | UpdateProductDTO): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const showDialog = ref(props.modelValue);
const isEdit = ref(!!props.product);

const formData = ref({
  name: '',
  description: '',
  sku: '',
  price: 0,
  categoryId: '',
  minStockLevel: 0,
  currentStock: 0,
});

const categoryOptions = ref<CategoryOption[]>(props.categories);

watch(
  () => props.modelValue,
  (val) => {
    showDialog.value = val;
    if (val) {
      isEdit.value = !!props.product;
      if (props.product) {
        formData.value = {
          name: props.product.name,
          description: props.product.description || '',
          sku: props.product.sku || '',
          price: props.product.price,
          categoryId: props.product.categoryId,
          minStockLevel: props.product.minStockLevel,
          currentStock: props.product.currentStock,
        };
      } else {
        formData.value = {
          name: '',
          description: '',
          sku: '',
          price: 0,
          categoryId: '',
          minStockLevel: 0,
          currentStock: 0,
        };
      }
    }
  }
);

watch(
  () => props.categories,
  (val) => {
    categoryOptions.value = val;
  }
);

const onSubmit = () => {
  emit('submit', formData.value);
  onClose();
};

const onClose = () => {
  emit('update:modelValue', false);
};
</script>
