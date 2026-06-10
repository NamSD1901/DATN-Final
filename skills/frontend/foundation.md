# 🟢 Level 1: Foundation (Nền Tảng) - Frontend Skills

Bao gồm các kiến thức nền tảng bắt buộc về HTML5, CSS3 Layouts, và cách áp dụng Hệ thống thiết kế (Design System) của dự án.

---

## 📋 Danh Sách Kỹ Kăng
1. [FE-F01: HTML5 & Semantic Elements](#fe-f01-html5--semantic-elements)
2. [FE-F02: CSS3 Layouts & CSS Variables](#fe-f02-css3-layouts--css-variables)
3. [FE-F03: JavaScript/TypeScript Basics](#fe-f03-javascripttypescript-basics)

---

### FE-F01: HTML5 & Semantic Elements

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Mastery |
| **Sprint** | Sprint 1 |
| **Tại sao cần?** | Đảm bảo trang web SEO tốt, dễ đọc đối với các công cụ đọc màn hình và cấu trúc DOM sạch sẽ. |

<details>
<summary><b>📚 Cách áp dụng (Click để mở rộng)</b></summary>

```html
<!-- Dành cho các khối giao diện của MyPetClinic -->
<header class="header">
  <nav class="navigation">
    <router-link to="/">Trang chủ</router-link>
    <router-link to="/services/tiem-phong">Tiêm phòng</router-link>
  </nav>
</header>

<main id="main-content">
  <section class="hero-section">
    <h1>Phòng khám thú y MyPetClinic</h1>
    <p>Chăm sóc thú cưng bằng cả trái tim.</p>
  </section>

  <section class="services-grid">
    <article class="service-card">
      <h2>Grooming & Spa</h2>
      <p>Dịch vụ cắt tỉa lông chuyên nghiệp cho chó mèo.</p>
    </article>
  </section>
</main>
```

</details>

---

### FE-F02: CSS3 Layouts & CSS Variables

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 1-2 |
| **Tại sao cần?** | MyPetClinic sử dụng hệ thống màu sắc Warm Gold và Glassmorphism tùy chỉnh. Nắm vững CSS variables giúp tái sử dụng và điều chỉnh giao diện nhanh chóng. |

<details>
<summary><b>📚 Chi tiết màu sắc & biến CSS (Click để mở rộng)</b></summary>

Nhà phát triển cần nắm vững các biến định nghĩa trong [style.css](file:///e:/DATN/MyPetClinic/frontend/src/style.css):

```css
/* Sử dụng biến màu sắc trong CSS */
.custom-card {
  background-color: var(--bg-pure);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  transition: var(--transition-smooth);
}

.custom-card:hover {
  border-color: var(--primary-gold);
  transform: translateY(-4px);
}

/* Áp dụng Glassmorphism có sẵn của dự án */
.glass-panel {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: var(--radius-md);
}
```

</details>

---

### FE-F03: JavaScript/TypeScript Basics

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 1 |

<details>
<summary><b>📚 Cú pháp cơ bản (Click để mở rộng)</b></summary>

* Sử dụng ES6 Destructuring, Spread Operator, và Arrow Functions.
* Kiểu dữ liệu TypeScript cơ bản: `string`, `number`, `boolean`, `Array<T>`, `interface`, và `type`.

```typescript
interface UserProfile {
  fullName: string;
  email: string;
  phone?: string;
  gender: number;
}

const showWelcomeMessage = (user: UserProfile): string => {
  return `Chào mừng ${user.fullName} đến với phòng khám!`;
};
```

</details>
