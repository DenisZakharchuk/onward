<template>
  <div id="app">
    <nav class="navbar">
      <div class="nav-brand">
        <h2>📦 Inventory System</h2>
      </div>
      <div class="nav-links">
        <router-link to="/products" active-class="active">Products</router-link>
        <router-link to="/categories" active-class="active">Categories</router-link>
        <router-link to="/stock" active-class="active">Stock Movements</router-link>
        <router-link to="/audit-log" active-class="active">Audit Log</router-link>
      </div>
    </nav>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  background: #f5f5f5;
}

#app {
  min-height: 100vh;
}

.navbar {
  background: white;
  padding: 16px 24px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-brand h2 {
  color: #333;
}

.nav-links {
  display: flex;
  gap: 16px;
}

.nav-links a {
  padding: 8px 16px;
  text-decoration: none;
  color: #666;
  border-radius: 4px;
  transition: all 0.2s;
}

.nav-links a:hover {
  background: #f5f5f5;
  color: #333;
}

.nav-links a.active {
  background: #4CAF50;
  color: white;
}

.main-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px;
}
</style>
