<template>
  <q-page padding>
    <div class="text-h4 q-mb-md">Audit Log</div>

    <q-card class="q-mb-md">
      <q-card-section>
        <div class="text-h6 q-mb-md">Filters</div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-3">
            <q-input
              v-model="filters.fromDate"
              label="From Date"
              outlined
              dense
            >
              <template v-slot:append>
                <q-icon name="event" class="cursor-pointer">
                  <q-popup-proxy
                    cover
                    transition-show="scale"
                    transition-hide="scale"
                  >
                    <q-date v-model="filters.fromDate" mask="YYYY-MM-DD">
                      <div class="row items-center justify-end">
                        <q-btn v-close-popup label="Close" color="primary" flat />
                      </div>
                    </q-date>
                  </q-popup-proxy>
                </q-icon>
              </template>
            </q-input>
          </div>

          <div class="col-12 col-md-3">
            <q-input v-model="filters.toDate" label="To Date" outlined dense>
              <template v-slot:append>
                <q-icon name="event" class="cursor-pointer">
                  <q-popup-proxy
                    cover
                    transition-show="scale"
                    transition-hide="scale"
                  >
                    <q-date v-model="filters.toDate" mask="YYYY-MM-DD">
                      <div class="row items-center justify-end">
                        <q-btn v-close-popup label="Close" color="primary" flat />
                      </div>
                    </q-date>
                  </q-popup-proxy>
                </q-icon>
              </template>
            </q-input>
          </div>

          <div class="col-12 col-md-2">
            <q-select
              v-model="filters.entityType"
              :options="entityTypeOptions"
              label="Entity Type"
              outlined
              dense
              clearable
            />
          </div>

          <div class="col-12 col-md-2">
            <q-select
              v-model="filters.action"
              :options="actionOptions"
              label="Action"
              outlined
              dense
              clearable
            />
          </div>

          <div class="col-12 col-md-2">
            <q-input
              v-model="filters.userId"
              label="User ID"
              outlined
              dense
              clearable
            />
          </div>
        </div>

        <div class="row justify-end q-mt-md">
          <q-btn
            color="primary"
            label="Apply Filters"
            icon="search"
            @click="loadAuditLogs"
          />
        </div>
      </q-card-section>
    </q-card>

    <q-table
      :rows="auditLogs"
      :columns="columns"
      row-key="id"
      :loading="loading"
      flat
      bordered
      :rows-per-page-options="[10, 25, 50]"
    >
      <template v-slot:body-cell-action="props">
        <q-td :props="props">
          <q-badge :color="getActionColor(props.row.action)" text-color="white">
            {{ props.row.action }}
          </q-badge>
        </q-td>
      </template>

      <template v-slot:body-cell-timestamp="props">
        <q-td :props="props">
          {{ formatDate(props.row.timestamp) }}
        </q-td>
      </template>

      <template v-slot:body-cell-changes="props">
        <q-td :props="props">
          <q-btn
            flat
            dense
            round
            color="primary"
            icon="visibility"
            @click="showChanges(props.row)"
          >
            <q-tooltip>View Changes</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>

    <q-dialog v-model="showChangesDialog">
      <q-card style="min-width: 500px">
        <q-card-section>
          <div class="text-h6">Audit Log Details</div>
        </q-card-section>

        <q-card-section v-if="selectedLog">
          <div class="q-mb-sm">
            <strong>Action:</strong>
            <q-badge
              :color="getActionColor(selectedLog.action)"
              text-color="white"
              class="q-ml-sm"
            >
              {{ selectedLog.action }}
            </q-badge>
          </div>
          <div class="q-mb-sm">
            <strong>Entity Type:</strong> {{ selectedLog.entityType }}
          </div>
          <div class="q-mb-sm">
            <strong>Entity ID:</strong> {{ selectedLog.entityId }}
          </div>
          <div class="q-mb-sm">
            <strong>User ID:</strong> {{ selectedLog.userId }}
          </div>
          <div class="q-mb-sm">
            <strong>Timestamp:</strong> {{ formatDate(selectedLog.timestamp) }}
          </div>

          <q-separator class="q-my-md" />

          <div class="text-subtitle1 q-mb-sm">Changes:</div>
          <pre class="bg-grey-2 q-pa-sm" style="overflow: auto">{{
            JSON.stringify(selectedLog.changes, null, 2)
          }}</pre>

          <div v-if="selectedLog.metadata" class="q-mt-md">
            <div class="text-subtitle1 q-mb-sm">Metadata:</div>
            <pre class="bg-grey-2 q-pa-sm" style="overflow: auto">{{
              JSON.stringify(selectedLog.metadata, null, 2)
            }}</pre>
          </div>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn label="Close" color="primary" flat v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { auditService, type AuditLog } from '../services/auditService';

const $q = useQuasar();

const auditLogs = ref<AuditLog[]>([]);
const loading = ref(false);
const showChangesDialog = ref(false);
const selectedLog = ref<AuditLog | null>(null);

const filters = ref({
  fromDate: '',
  toDate: '',
  entityType: '',
  action: '',
  userId: '',
});

const entityTypeOptions = ['Product', 'Category', 'StockMovement'];
const actionOptions = [
  'ProductCreated',
  'ProductUpdated',
  'ProductDeleted',
  'CategoryCreated',
  'CategoryUpdated',
  'CategoryDeleted',
  'StockMovementCreated',
];

const columns = [
  {
    name: 'action',
    label: 'Action',
    field: 'action',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'entityType',
    label: 'Entity Type',
    field: 'entityType',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'entityId',
    label: 'Entity ID',
    field: 'entityId',
    align: 'left' as const,
  },
  {
    name: 'userId',
    label: 'User ID',
    field: 'userId',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'timestamp',
    label: 'Timestamp',
    field: 'timestamp',
    align: 'left' as const,
    sortable: true,
  },
  {
    name: 'changes',
    label: 'Details',
    field: 'changes',
    align: 'center' as const,
  },
];

onMounted(() => {
  // Set default date range (last 30 days)
  const today = new Date();
  const thirtyDaysAgo = new Date(today);
  thirtyDaysAgo.setDate(today.getDate() - 30);

  filters.value.fromDate = thirtyDaysAgo.toISOString().split('T')[0] ?? '';
  filters.value.toDate = today.toISOString().split('T')[0] ?? '';

  loadAuditLogs();
});

const loadAuditLogs = async () => {
  loading.value = true;
  try {
    const filterParams: {
      fromDate?: string;
      toDate?: string;
      entityType?: string;
      action?: string;
      userId?: string;
    } = {};

    if (filters.value.fromDate) {
      filterParams.fromDate = new Date(filters.value.fromDate).toISOString();
    }
    if (filters.value.toDate) {
      filterParams.toDate = new Date(filters.value.toDate).toISOString();
    }
    if (filters.value.entityType) {
      filterParams.entityType = filters.value.entityType;
    }
    if (filters.value.action) {
      filterParams.action = filters.value.action;
    }
    if (filters.value.userId) {
      filterParams.userId = filters.value.userId;
    }

    auditLogs.value = await auditService.getAuditLogs(filterParams);
  } catch (error) {
    $q.notify({
      type: 'negative',
      message: 'Failed to load audit logs',
    });
  } finally {
    loading.value = false;
  }
};

const showChanges = (log: AuditLog) => {
  selectedLog.value = log;
  showChangesDialog.value = true;
};

const formatDate = (date: string) => {
  return new Date(date).toLocaleString();
};

const getActionColor = (action: string) => {
  if (action.includes('Created')) return 'positive';
  if (action.includes('Updated')) return 'warning';
  if (action.includes('Deleted')) return 'negative';
  return 'info';
};
</script>
