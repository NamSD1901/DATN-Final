import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7284/api',
  withCredentials: true, // Quan trọng để gửi nhận Cookie Session
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  }
});

// Response Interceptor để xử lý lỗi tập trung
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      console.warn('Phiên đăng nhập hết hạn hoặc chưa đăng nhập.');
    }
    return Promise.reject(error);
  }
);

export default api;
