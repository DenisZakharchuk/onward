<template>
  <q-badge v-if="isLowStock" color="warning" text-color="dark">
    <q-icon name="warning" class="q-mr-xs" />
    Low Stock
  </q-badge>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  currentStock: number;
  minStockLevel: number;
}

const props = defineProps<Props>();

const isLowStock = computed(
  () => props.currentStock <= props.minStockLevel
);
</script>
