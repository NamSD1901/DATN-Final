<template>
  <div class="dashboard-container">
    <div class="card">
      <h1>Dashboard (Chào mừng bạn đến với MyPetClinic)</h1>
      <p>Bạn đã đăng nhập thành công!</p>
      <button @click="logout" class="btn-logout">Đăng xuất</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import api from '../services/api';
import { useRouter } from 'vue-router';

const router = useRouter();

const logout = async () => {
  try {
    await api.post('/account/logout');
    router.push('/login');
  } catch (error) {
    console.error('Lỗi khi đăng xuất:', error);
  }
};
</script>

<style scoped>
.dashboard-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: radial-gradient(circle at top right, #1e293b, #0f172a);
  color: #f8fafc;
  font-family: 'Outfit', 'Inter', sans-serif;
}

.card {
  background: rgba(30, 41, 59, 0.7);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 2.5rem;
  border-radius: 16px;
  text-align: center;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
  max-width: 500px;
  width: 90%;
}

h1 {
  font-size: 1.8rem;
  margin-bottom: 1rem;
  background: linear-gradient(135deg, #38bdf8, #818cf8);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

p {
  color: #94a3b8;
  margin-bottom: 2rem;
}

.btn-logout {
  background: linear-gradient(135deg, #ef4444, #b91c1c);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-logout:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
}
</style>
