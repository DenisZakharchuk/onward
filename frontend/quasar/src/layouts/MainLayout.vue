<template>
  <q-layout view="hHh lpR fFf">
    <q-header elevated class="bg-primary text-white">
      <q-toolbar>
        <q-toolbar-title>
          <q-icon name="inventory_2" size="sm" class="q-mr-sm" />
          Inventory Dashboard
        </q-toolbar-title>

        <q-tabs v-model="activeTab" align="right">
          <q-route-tab
            name="products"
            to="/products"
            label="Products"
            icon="inventory"
          />
          <q-route-tab
            name="categories"
            to="/categories"
            label="Categories"
            icon="category"
          />
          <q-route-tab
            name="stock"
            to="/stock"
            label="Stock Movements"
            icon="swap_vert"
          />
          <q-route-tab
            name="audit"
            to="/audit-log"
            label="Audit Log"
            icon="history"
          />
        </q-tabs>
      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const activeTab = ref('products');

watch(
  () => route.path,
  (path) => {
    if (path.includes('products')) activeTab.value = 'products';
    else if (path.includes('categories')) activeTab.value = 'categories';
    else if (path.includes('stock')) activeTab.value = 'stock';
    else if (path.includes('audit-log')) activeTab.value = 'audit';
  },
  { immediate: true }
);
</script>
