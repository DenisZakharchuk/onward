<template>
  <q-dialog v-model="showDialog" @hide="onClose">
    <q-card style="min-width: 500px">
      <q-card-section>
        <div class="text-h6">Add Stock Movement</div>
      </q-card-section>

      <q-card-section>
        <q-form @submit="onSubmit">
          <q-select
            v-model="formData.productId"
            :options="productOptions"
            option-value="value"
            option-label="label"
            emit-value
            map-options
            label="Product *"
            :rules="[(val) => !!val || 'Product is required']"
            outlined
            class="q-mb-md"
          >
            <template v-slot:option="scope">
              <q-item v-bind="scope.itemProps">
                <q-item-section>
                  <q-item-label>{{ scope.opt.label }}</q-item-label>
                  <q-item-label caption
                    >Current Stock: {{ scope.opt.stock }}</q-item-label
                  >
                </q-item-section>
              </q-item>
            </template>
          </q-select>

          <q-select
            v-model="formData.movementType"
            :options="movementTypeOptions"
            option-value="value"
            option-label="label"
            emit-value
            map-options
            label="Movement Type *"
            :rules="[(val) => val !== null || 'Type is required']"
            outlined
            class="q-mb-md"
          />

          <q-input
            v-model.number="formData.quantity"
            label="Quantity *"
            type="number"
            min="1"
            :rules="[(val) => val > 0 || 'Quantity must be positive']"
            outlined
            class="q-mb-md"
          />

          <q-input
            v-model="formData.notes"
            label="Notes"
            type="textarea"
            outlined
            rows="3"
            class="q-mb-md"
          />

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
import type { CreateStockMovementDTO } from '../types';

interface ProductOption {
  label: string;
  value: string;
  stock: number;
}

interface Props {
  modelValue: boolean;
  products: ProductOption[];
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', data: CreateStockMovementDTO): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const showDialog = ref(props.modelValue);

const formData = ref({
  productId: '',
  movementType: 0,
  quantity: 1,
  notes: '',
});

const productOptions = ref<ProductOption[]>(props.products);

const movementTypeOptions = [
  { label: 'Stock In', value: 0 },
  { label: 'Stock Out', value: 1 },
  { label: 'Adjustment', value: 2 },
];

watch(
  () => props.modelValue,
  (val) => {
    showDialog.value = val;
    if (val) {
      formData.value = {
        productId: '',
        movementType: 0,
        quantity: 1,
        notes: '',
      };
    }
  }
);

watch(
  () => props.products,
  (val) => {
    productOptions.value = val;
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
