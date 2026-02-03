import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      { path: '', redirect: '/products' },
      { path: 'products', component: () => import('pages/ProductsPage.vue') },
      {
        path: 'categories',
        component: () => import('pages/CategoriesPage.vue'),
      },
      { path: 'stock', component: () => import('pages/StockPage.vue') },
      {
        path: 'audit-log',
        component: () => import('pages/AuditLogPage.vue'),
      },
    ],
  },

  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
