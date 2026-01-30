<template>
  <div class="audit-log-container">
    <div class="header">
      <h1>Audit Log</h1>
      <p class="subtitle">Track all changes to your inventory system</p>
    </div>

    <div class="filters-card">
      <h2>Filters</h2>
      <div class="filters-grid">
        <div class="filter-group">
          <label>From Date</label>
          <input type="datetime-local" v-model="filters.fromDate" />
        </div>
        <div class="filter-group">
          <label>To Date</label>
          <input type="datetime-local" v-model="filters.toDate" />
        </div>
        <div class="filter-group">
          <label>Entity Type</label>
          <select v-model="filters.entityType">
            <option value="">All</option>
            <option value="Product">Product</option>
            <option value="Category">Category</option>
            <option value="StockMovement">Stock Movement</option>
          </select>
        </div>
        <div class="filter-group">
          <label>Action</label>
          <select v-model="filters.action">
            <option value="">All</option>
            <option value="ProductCreated">Product Created</option>
            <option value="ProductUpdated">Product Updated</option>
            <option value="ProductDeleted">Product Deleted</option>
            <option value="CategoryCreated">Category Created</option>
            <option value="CategoryUpdated">Category Updated</option>
            <option value="CategoryDeleted">Category Deleted</option>
            <option value="StockMovementCreated">Stock Movement Created</option>
          </select>
        </div>
        <div class="filter-group">
          <label>User ID</label>
          <input type="text" v-model="filters.userId" placeholder="e.g., system" />
        </div>
        <div class="filter-actions">
          <button @click="loadAuditLogs" class="btn-primary">Apply Filters</button>
          <button @click="clearFilters" class="btn-secondary">Clear</button>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading">Loading audit logs...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="auditLogs.length === 0" class="no-data">No audit logs found</div>

    <div v-else class="audit-list">
      <div class="audit-card" v-for="log in auditLogs" :key="log.id">
        <div class="audit-header">
          <span class="action-badge" :class="getActionClass(log.action)">
            {{ log.action }}
          </span>
          <span class="entity-type">{{ log.entityType }}</span>
          <span class="timestamp">{{ formatDate(log.timestamp) }}</span>
        </div>
        <div class="audit-body">
          <div class="audit-info">
            <div class="info-item">
              <strong>Entity ID:</strong> {{ log.entityId }}
            </div>
            <div class="info-item">
              <strong>User:</strong> {{ log.userId }}
            </div>
            <div v-if="log.ipAddress" class="info-item">
              <strong>IP Address:</strong> {{ log.ipAddress }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { auditService, type AuditLog, type AuditLogFilters } from '../services/auditService';

const auditLogs = ref<AuditLog[]>([]);
const loading = ref(false);
const error = ref('');

const filters = ref<AuditLogFilters>({
  fromDate: '',
  toDate: '',
  entityType: '',
  action: '',
  userId: '',
});

const loadAuditLogs = async () => {
  loading.value = true;
  error.value = '';
  try {
    const filterParams: AuditLogFilters = {};
    
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
  } catch (err) {
    error.value = 'Failed to load audit logs. Please try again.';
    console.error(err);
  } finally {
    loading.value = false;
  }
};

const clearFilters = () => {
  filters.value = {
    fromDate: '',
    toDate: '',
    entityType: '',
    action: '',
    userId: '',
  };
  loadAuditLogs();
};

const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return date.toLocaleString();
};

const getActionClass = (action: string): string => {
  if (action.includes('Created')) return 'badge-success';
  if (action.includes('Updated')) return 'badge-warning';
  if (action.includes('Deleted')) return 'badge-danger';
  return 'badge-info';
};

onMounted(() => {
  loadAuditLogs();
});
</script>

<style scoped>
.audit-log-container {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.header {
  margin-bottom: 30px;
}

.header h1 {
  margin: 0;
  color: #333;
  font-size: 2rem;
}

.subtitle {
  margin: 5px 0 0 0;
  color: #666;
  font-size: 1rem;
}

.filters-card {
  background: white;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.filters-card h2 {
  margin: 0 0 15px 0;
  color: #333;
  font-size: 1.2rem;
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
  align-items: end;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.filter-group label {
  font-weight: 500;
  color: #333;
  font-size: 0.9rem;
}

.filter-group input,
.filter-group select {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.9rem;
  color: #333;
}

.filter-actions {
  display: flex;
  gap: 10px;
}

.btn-primary,
.btn-secondary {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.2s;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-primary:hover {
  background: #0056b3;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #545b62;
}

.loading,
.error,
.no-data {
  text-align: center;
  padding: 40px;
  color: #666;
  font-size: 1.1rem;
}

.error {
  color: #dc3545;
}

.audit-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.audit-card {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.audit-header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 15px;
  flex-wrap: wrap;
}

.action-badge {
  padding: 4px 12px;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: 600;
  color: white;
}

.badge-success {
  background: #28a745;
}

.badge-warning {
  background: #ffc107;
  color: #333;
}

.badge-danger {
  background: #dc3545;
}

.badge-info {
  background: #17a2b8;
}

.entity-type {
  font-weight: 500;
  color: #333;
}

.timestamp {
  color: #666;
  font-size: 0.9rem;
  margin-left: auto;
}

.audit-body {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.audit-info {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.info-item {
  color: #333;
  font-size: 0.9rem;
}

.info-item strong {
  color: #666;
  margin-right: 5px;
}
</style>
