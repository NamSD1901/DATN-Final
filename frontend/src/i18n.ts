import { createI18n } from 'vue-i18n';
import vi from './locales/vi.json';
import en from './locales/en.json';

const savedLocale = localStorage.getItem('user_locale') || 'vi';

const i18n = createI18n({
  legacy: false, // use Composition API
  locale: savedLocale,
  fallbackLocale: 'en',
  messages: {
    vi,
    en
  }
});

export default i18n;
