import { createRouter, createWebHistory } from 'vue-router';
import ProductsView from '../components/ProductsView.vue';
import CategoriesView from '../components/CategoriesView.vue';
import StockView from '../components/StockView.vue';
import AuditLogView from '../components/AuditLogView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/products',
    },
    {
      path: '/products',
      name: 'products',
      component: ProductsView,
    },
    {
      path: '/categories',
      name: 'categories',
      component: CategoriesView,
    },
    {
      path: '/stock',
      name: 'stock',
      component: StockView,
    },
    {
      path: '/audit-log',
      name: 'audit-log',
      component: AuditLogView,
    },
  ],
});

export default router;
