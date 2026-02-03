<template>
  <q-dialog v-model="showDialog" @hide="onClose">
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-h6">
          {{ isEdit ? 'Edit Category' : 'Add Category' }}
        </div>
      </q-card-section>

      <q-card-section>
        <q-form @submit="onSubmit">
          <q-input
            v-model="formData.name"
            label="Category Name *"
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
import type { Category, CreateCategoryDTO, UpdateCategoryDTO } from '../types';

interface Props {
  modelValue: boolean;
  category?: Category | undefined;
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void;
  (e: 'submit', data: CreateCategoryDTO | UpdateCategoryDTO): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const showDialog = ref(props.modelValue);
const isEdit = ref(!!props.category);

const formData = ref({
  name: '',
  description: '',
});

watch(
  () => props.modelValue,
  (val) => {
    showDialog.value = val;
    if (val) {
      isEdit.value = !!props.category;
      if (props.category) {
        formData.value = {
          name: props.category.name,
          description: props.category.description || '',
        };
      } else {
        formData.value = {
          name: '',
          description: '',
        };
      }
    }
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
