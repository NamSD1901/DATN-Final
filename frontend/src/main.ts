import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import router from './router'
import { createPinia } from 'pinia'
import { autoAnimatePlugin } from '@formkit/auto-animate/vue'
import i18n from './i18n'
import { initTheme } from './utils/useTheme'

// Khởi tạo theme ngay lập tức trước khi mount
// để tránh FOUC (Flash of Unstyled Content)
initTheme();

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.use(autoAnimatePlugin)
app.use(i18n)
app.mount('#app')
