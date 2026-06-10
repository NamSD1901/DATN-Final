# 🐾 CS-05: Vue 3, TS & Premium CSS Basics (Phát triển Giao diện)

Giao diện Frontend của **MyPetClinic** được xây dựng trên bộ công nghệ hiện đại: **Vue 3 (Composition API)**, **TypeScript**, công cụ build **Vite**, bộ icon **Lucide Vue Next**, và đặc biệt là hệ thống phong cách thiết kế **Premium Gold Design System** tự phát triển thay vì dùng TailwindCSS.

Tài liệu này hướng dẫn cách đọc hiểu mã nguồn Frontend, cách viết component Vue 3 chuẩn hóa và cách áp dụng chuẩn Design System của dự án.

---

## 1. Cấu trúc Thư mục Frontend chính

Mã nguồn Frontend nằm tại thư mục [frontend/src](file:///e:/DATN/MyPetClinic/frontend/src):
- `assets/`: Chứa các tài nguyên hình ảnh tĩnh.
- `components/`: Các UI Component tái sử dụng (như Nút bấm, Ô nhập liệu, Card thông tin, Hộp thoại Popup).
- `views/`: Các màn hình trang chính tương ứng với các route (như Dashboard, Lịch khám, Hồ sơ thú cưng).
- `router/`: Cấu hình định tuyến các trang.
- `services/`: Nơi thực hiện gọi API (sử dụng **Axios** kết nối đến Backend .NET).
- `style.css`: File chứa toàn bộ mã nguồn của hệ thống thiết kế Premium Gold.

---

## 2. Viết Vue 3 Single File Component (SFC) bằng TypeScript

Chúng ta tuân thủ viết Vue component theo chuẩn **Composition API** kết hợp với thẻ `<script setup lang="ts">`. Dưới đây là cấu trúc mẫu chuẩn:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Calendar, User } from 'lucide-vue-next'; // Sử dụng Lucide Icons

// Định nghĩa kiểu dữ liệu (Props) nhận vào từ bên ngoài
interface BookingProps {
  doctorName: string;
  timeSlot: string;
}

const props = defineProps<BookingProps>();

// Khai báo State bằng ref
const isConfirmed = ref(false);

const toggleConfirm = () => {
  isConfirmed.value = !isConfirmed.value;
};
</script>

<template>
  <div class="glass-card p-5 max-w-sm">
    <div class="flex items-center gap-2 mb-3">
      <Calendar :size="18" class="text-amber-500" />
      <span class="text-sm font-semibold">Khung giờ: {{ props.timeSlot }}</span>
    </div>
    
    <div class="flex items-center gap-2 mb-4">
      <User :size="18" class="text-stone-500" />
      <span class="text-sm">Bác sĩ: {{ props.doctorName }}</span>
    </div>
    
    <!-- Áp dụng Premium Buttons và CSS Variables -->
    <button 
      @click="toggleConfirm" 
      :class="isConfirmed ? 'btn-premium' : 'btn-premium-outline'"
      class="w-full"
    >
      {{ isConfirmed ? 'Đã xác nhận' : 'Xác nhận lịch khám' }}
    </button>
  </div>
</template>

<style scoped>
/* Chỉ viết các style đặc thù không có sẵn trong style.css chung */
</style>
```

---

## 3. Đồng bộ hóa Premium Gold Design System ( style.css )

Dự án có hệ thống CSS thiết kế cao cấp nằm trong file [style.css](file:///e:/DATN/MyPetClinic/frontend/src/style.css). **Tuyệt đối hạn chế tự đặt màu sắc tùy ý (inline styles) hoặc các màu mặc định.**

### Sử dụng CSS Custom Properties (Variables):
Khi viết thuộc tính CSS trong component, hãy dùng các biến màu đại diện:
- Màu vàng kim chủ đạo: `var(--primary-gold)`
- Màu vàng kim đậm (dành cho hover): `var(--primary-dark)`
- Màu kem nhạt: `var(--primary-cream)`
- Nền trang sáng: `var(--bg-light)`
- Chữ tối: `var(--text-dark)`
- Chữ xám mờ: `var(--text-muted)`

### Các Class CSS tiện ích có sẵn:
Hãy áp dụng trực tiếp các class sau vào thẻ HTML để tạo nên giao diện cao cấp:
- **Card kính mờ (Glassmorphism):** class `.glass-card` (có sẵn hiệu ứng hover nổi bóng mượt mà).
- **Nút bấm cao cấp:** class `.btn-premium` (nút màu vàng gradient) hoặc `.btn-premium-outline` (nút viền vàng, nền trong suốt).
- **Ô nhập liệu:** class `.input-premium` (có viền mảnh, khi focus tự động sáng vàng mờ và đổ bóng nhẹ).
- **Chữ Gradient vàng tối:** class `.gradient-text-gold` (rất thích hợp cho tiêu đề trang lớn).
- **Hiệu ứng Scroll Reveal (Cuộn trang hiển thị):** class `.reveal`, `.reveal-left`, `.reveal-right` kết hợp với class `.active` khi component được mount.

---

## 4. Bài tập Thực Hành Đạt Yêu Cầu CS-05

- [ ] Chạy thành công ứng dụng Frontend dưới local (`npm run dev`) và truy cập được trên trình duyệt.
- [ ] Tạo mới thành công 1 component Vue 3 sử dụng đúng TypeScript (`lang="ts"`) và `<script setup>`.
- [ ] Thay đổi thành công màu nền hoặc viền của một khối giao diện sử dụng đúng biến CSS Custom Properties từ `style.css`.
- [ ] Thực hiện cấu hình định tuyến (Route) mới và liên kết nó vào thanh Menu điều hướng.
