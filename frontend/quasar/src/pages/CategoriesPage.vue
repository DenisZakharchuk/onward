<template>
  <q-page padding>
    <div class="row items-center q-mb-md">
      <div class="text-h4">Categories</div>
      <q-space />
      <q-btn
        color="primary"
        icon="add"
        label="Add Category"
        @click="openAddDialog"
      />
    </div>

    <q-table
      :rows="categories"
      :columns="columns"
      row-key="id"
      :loading="loading"
      flat
      bordered
      :rows-per-page-options="[10, 25, 50]"
    >
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

    <category-form
      v-model="showDialog"
      :category="selectedCategory"
      @submit="handleSubmit"
    />
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import CategoryForm from '../components/CategoryForm.vue';
import { categoryService } from '../services/categoryService';
import type {
  Category,
  CreateCategoryDTO,
  UpdateCategoryDTO,
} from '../types';

const $q = useQuasar();

const categories = ref<Category[]>([]);
const loading = ref(false);
const showDialog = ref(false);
const selectedCategory = ref<Category | undefined>(undefined);

const columns = [
  {
    name: 'name',
    label: 'Name',
    field: 'name',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'description',
    label: 'Description',
    field: 'description',
    align: 'left' as const,
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
  loadCategories();
});

const loadCategories = async () => {
  loading.value = true;
  try {
    categories.value = await categoryService.getAll();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load categories',
    });
  } finally {
    loading.value = false;
  }
};

const openAddDialog = () => {
  selectedCategory.value = undefined;
  showDialog.value = true;
};

const openEditDialog = (category: Category) => {
  selectedCategory.value = category;
  showDialog.value = true;
};

const handleSubmit = async (
  data: CreateCategoryDTO | UpdateCategoryDTO
) => {
  $q.loading.show();
  try {
    if (selectedCategory.value) {
      await categoryService.update(selectedCategory.value.id, data);
      $q.notify({
        type: 'positive',
        message: 'Category updated successfully',
      });
    } else {
      await categoryService.create(data as CreateCategoryDTO);
      $q.notify({
        type: 'positive',
        message: 'Category created successfully',
      });
    }
    await loadCategories();
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to save category',
    });
  } finally {
    $q.loading.hide();
  }
};

const confirmDelete = (category: Category) => {
  $q.dialog({
    title: 'Confirm Delete',
    message: `Are you sure you want to delete "${category.name}"?`,
    cancel: true,
    persistent: true,
  }).onOk(async () => {
    $q.loading.show();
    try {
      await categoryService.delete(category.id);
      $q.notify({
        type: 'positive',
        message: 'Category deleted successfully',
      });
      await loadCategories();
    } catch (error) {
      $q.notify({
        type: 'negative',
        message: 'Failed to delete category',
      });
    } finally {
      $q.loading.hide();
    }
  });
};
</script>
