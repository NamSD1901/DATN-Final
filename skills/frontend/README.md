# 🎨 MyPetClinic - Frontend Skills Framework

Chào mừng bạn đến với **Khung Năng Lực Phát Triển Frontend** cho dự án **MyPetClinic**. Tài liệu này được thiết kế dựa trên cấu trúc Vue 3 (Vite + TypeScript) và hệ thống giao diện Premium Gold Design System hiện tại của dự án.

---

## 📋 Mục Lục Chi Tiết

1. [Tổng Quan Kiến Trúc Frontend](#1-tổng-quan-kiến-trúc-frontend)
2. [Cây Kỹ Năng (Skill Tree)](#2-cây-kỹ-năng-skill-tree)
3. [Chi Tiết Từng Cấp Độ (Detailed Framework Files)](#3-chi-tiết-từng-cấp-độ-detailed-framework-files)
   - 🟢 [Level 1: Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/frontend/foundation.md)
   - 🟡 [Level 2: Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/frontend/core.md)
   - 🟠 [Level 3: Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/frontend/advanced.md)
   - 🔴 [Level 4: Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/frontend/expert.md)
4. [Lộ Trình & Đánh Giá](file:///e:/DATN/MyPetClinic/skills/frontend/roadmap.md)

---

## 1. Tổng Quan Kiến Trúc Frontend

```
┌─────────────────────────────────────────────────────────────┐
│                 MyPetClinic Frontend (Vue 3 SPA)            │
├─────────────────────────────────────────────────────────────┤
│  Views Layer (Home, Login, Register, Profile, Dashboard...) │
├─────────────────────────────────────────────────────────────┤
│  Components Layer (Layout, Shared, Dashboard Components)   │
├─────────────────────────────────────────────────────────────┤
│  Routing (Vue Router 4, Auth Guards via Cookie validation) │
├─────────────────────────────────────────────────────────────┤
│  Services Layer (Axios configured with withCredentials: true)│
├─────────────────────────────────────────────────────────────┘
```

**Hệ sinh thái công nghệ:**
* **Framework:** Vue 3 (Composition API + `<script setup>`)
* **Build Tool & Router:** Vite + Vue Router 4
* **Language:** TypeScript
* **HTTP Client:** Axios (xác thực dựa trên HttpOnly Cookie Session)
* **Styling:** Premium Gold Design System (CSS Custom Properties, Glassmorphism, Transition animations)
* **Icons:** Lucide Vue Next & Bootstrap Icons

---

## 2. Cây Kỹ Năng (Skill Tree)

```
Frontend Developer (MyPetClinic)
│
├── 🟢 Foundation (Nền Tảng) - Sprint 1
│   ├── HTML5 & Semantic Elements
│   ├── CSS3 Layouts (Flexbox, Grid)
│   ├── CSS variables & Design System Integration
│   └── JS/TS Basics (Primitive types, Array methods)
│
├── 🟡 Core (Cốt Lõi) - Sprint 1-2
│   ├── Vue 3 Composition API
│   │   ├── State Reactivity (ref, reactive, computed, watch)
│   │   └── Lifecycle hooks & Options API migration
│   ├── Component Communication
│   │   └── Props, Emits, and template refs
│   ├── SPA Routing (Vue Router 4)
│   └── Pinia State Management (Cơ bản)
│       └── Định nghĩa store, State, Actions & Getters
│
├── 🟠 Advanced (Nâng Cao) - Sprint 3-5
│   ├── HTTP Session & Cookie Client (Axios)
│   │   ├── Axios client creation & response interceptors (401 Redirect)
│   │   └── withCredentials config
│   ├── TypeScript & Centralized Models
│   │   └── Định nghĩa types tập trung tại src/shared/types
│   ├── Pinia Store Integration
│   │   └── Quản lý auth state toàn cục & đồng bộ Route Guards
│   └── Premium UI/UX Implementation (.glass-card, Scroll Reveal)
│
└── 🔴 Expert (Chuyên Gia) - Sprint 5-6
    ├── Advanced Composables & Custom Hooks
    ├── Form Validation nâng cao (Client-side validation)
    ├── Frontend Testing (Unit & E2E Testing)
    │   └── Vitest (Unit Test) & Cypress/Playwright (E2E Test)
    └── Performance Optimization & Vite Analyzer
```

---

## 3. Chi Tiết Từng Cấp Độ

*   🟢 **[Level 1: Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/frontend/foundation.md)**: HTML5 Semantic, Flexbox/Grid và CSS Custom Variables.
*   🟡 **[Level 2: Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/frontend/core.md)**: Composition API với `<script setup>`, Reactive States, props/emits, Vue Router, và Pinia cơ bản.
*   🟠 **[Level 3: Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/frontend/advanced.md)**: Axios với Global Interceptors, TypeScript Centralized Models, Pinia Auth Integration, và Premium Custom CSS.
*   🔴 **[Level 4: Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/frontend/expert.md)**: Custom Composables, Form handling, Performance Optimization, và Frontend Testing (Vitest & Cypress/Playwright).
