# 🟡 Level 2: Core (Cốt Lõi) - Frontend Skills

Bao gồm các kỹ năng cốt lõi về Vue 3 Composition API và Vue Router 4 được áp dụng trong dự án.

---

## 📋 Danh Sách Kỹ Năng
1. [FE-C01: Vue 3 Composition API với `<script setup>`](#fe-c01-vue-3-composition-api-với-script-setup)
2. [FE-C02: Vue Router 4 & Router Guards](#fe-c02-vue-router-4--router-guards)
3. [FE-C03: Pinia State Management Cơ Bản](#fe-c03-pinia-state-management-cơ-bản)

---

### FE-C01: Vue 3 Composition API với `<script setup>`

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 1-3 |
| **Tại sao cần?** | Tất cả component mới của MyPetClinic đều được viết bằng cú pháp `<script setup>` của Vue 3 giúp tối ưu hiệu năng và code gọn hơn. |

<details>
<summary><b>📚 Code mẫu Vue 3 Component (Click để mở rộng)</b></summary>

```vue
<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';

// 1. Định nghĩa Props & Emits bằng TypeScript compiler-macro
interface Props {
  title: string;
  isActive?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
  isActive: false
});

interface Emits {
  (e: 'change', status: boolean): void;
}
const emit = defineEmits<Emits>();

// 2. Reactivity State
const counter = ref(0);
const status = ref(props.isActive);

// Computed property
const doubleCounter = computed(() => counter.value * 2);

// Watcher
watch(status, (newVal) => {
  emit('change', newVal);
});

// Lifecycle Hook
onMounted(() => {
  console.log('Component Mounted!');
});

function increment() {
  counter.value++;
}
</script>

<template>
  <div class="custom-card glass-card">
    <h3>{{ title }}</h3>
    <p>Số lượng: {{ counter }} (Nhân đôi: {{ doubleCounter }})</p>
    <button class="btn-premium" @click="increment">Tăng số lượng</button>
  </div>
</template>
```

</details>

---

### FE-C02: Vue Router 4 & Router Guards

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 1-2 |
| **Tại sao cần?** | Kiểm soát quyền truy cập trang (Yêu cầu đăng nhập hoặc trang chỉ dành cho khách) thông qua Cookie Session Verification. |

<details>
<summary><b>📚 Cấu hình Guards & Verification (Click để mở rộng)</b></summary>

Nhà phát triển cần nắm cấu trúc định nghĩa Router và Navigation Guard như tại [router/index.ts](file:///e:/DATN/MyPetClinic/frontend/src/router/index.ts):

```typescript
import { createRouter, createWebHistory } from 'vue-router';
import api from '../services/api';

const routes = [
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { guest: true }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

// Kiểm tra quyền đăng nhập trước mỗi route chuyển trang
router.beforeEach(async (to, from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    try {
      // Gọi API lấy profile để xác minh cookie session còn hiệu lực hay không
      await api.get('/profile');
      next();
    } catch (err) {
      // Cookie hết hạn hoặc không có cookie session -> chuyển hướng về Login
      next('/login');
    }
  } else {
    next();
  }
});
```

</details>

---

### FE-C03: Pinia State Management Cơ Bản

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Intermediate |
| **Sprint** | Sprint 1-3 |
| **Tại sao cần?** | Quản lý và chia sẻ trạng thái dùng chung (ví dụ: thông tin user, giỏ hàng, hoặc cấu hình UI) một cách tập trung, dễ bảo trì hơn so với truyền props sâu. |

<details>
<summary><b>📚 Code mẫu định nghĩa và sử dụng Pinia Store (Click để mở rộng)</b></summary>

```typescript
// stores/counter.ts
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useCounterStore = defineStore('counter', () => {
  // 1. State (ref)
  const count = ref(0);

  // 2. Getters (computed)
  const doubleCount = computed(() => count.value * 2);

  // 3. Actions (function)
  function increment() {
    count.value++;
  }

  function decrement() {
    count.value--;
  }

  return { count, doubleCount, increment, decrement };
});
```

Sử dụng store trong Vue component:
```vue
<script setup lang="ts">
import { useCounterStore } from '../stores/counter';

const counterStore = useCounterStore();
</script>

<template>
  <div class="glass-card">
    <p>Giá trị count từ store: {{ counterStore.count }}</p>
    <p>Double count: {{ counterStore.doubleCount }}</p>
    <button class="btn-premium" @click="counterStore.increment()">Tăng</button>
  </div>
</template>
```

</details>
