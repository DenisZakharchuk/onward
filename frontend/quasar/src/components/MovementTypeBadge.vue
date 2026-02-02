<template>
  <q-chip :color="chipColor" text-color="white" dense>
    {{ chipLabel }}
  </q-chip>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  movementType: number;
}

const props = defineProps<Props>();

const chipColor = computed(() => {
  switch (props.movementType) {
    case 0:
      return 'positive';
    case 1:
      return 'negative';
    case 2:
      return 'info';
    default:
      return 'grey';
  }
});

const chipLabel = computed(() => {
  switch (props.movementType) {
    case 0:
      return 'Stock In';
    case 1:
      return 'Stock Out';
    case 2:
      return 'Adjustment';
    default:
      return 'Unknown';
  }
});
</script>
