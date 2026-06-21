import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import Login from '../views/Login.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { guest: true }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/Register.vue'),
    meta: { guest: true }
  },
  {
    path: '/activate',
    name: 'Activate',
    component: () => import('../views/Activate.vue'),
    meta: { guest: true }
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('../views/Profile.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/my-pets/:id',
    name: 'PetProfile',
    component: () => import('../views/PetProfile.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/qr-checkin/:id',
    name: 'QrCheckIn',
    component: () => import('../views/QrCheckIn.vue'),
    meta: { requiresAuth: true }
  },

  {
    path: '/history',
    name: 'History',
    component: () => import('../views/History.vue')
  },
  {
    path: '/team',
    name: 'Team',
    component: () => import('../views/Team.vue')
  },
  {
    path: '/contact',
    name: 'Contact',
    component: () => import('../views/Contact.vue')
  },
  {
    path: '/news',
    name: 'News',
    component: () => import('../views/TinTuc.vue')
  },
  {
    path: '/services/kham-dieu-tri',
    name: 'KhamDieuTri',
    component: () => import('../views/services/KhamDieuTri.vue')
  },
  {
    path: '/services/spa-grooming',
    name: 'SpaGrooming',
    component: () => import('../views/services/SpaGrooming.vue')
  },
  {
    path: '/services/suc-khoe',
    name: 'SucKhoe',
    component: () => import('../views/services/SucKhoe.vue')
  },
  {
    path: '/services/tiem-phong',
    name: 'TiemPhong',
    component: () => import('../views/services/TiemPhong.vue')
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue')
  },

  // Catch-all route to redirect back to login
  {
    path: '/:pathMatch(.*)*',
    redirect: '/login'
  }
];

import api from '../services/api';

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach(async (to, _from, next) => {
  if (to.matched.some(record => record.meta.requiresAuth)) {
    try {
      // Gọi API profile để xác minh cookie session
      await api.get('/profile');
      next();
    } catch (err) {
      // Nếu 401 hoặc lỗi kết nối, chuyển về trang login
      next('/login');
    }
  } else {
    next();
  }
});

export default router;
