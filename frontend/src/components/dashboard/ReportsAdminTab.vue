<template>
  <div class="reports-admin-container p-4">
    <!-- Header Controls -->
    <div class="glass-card p-4 mb-4 d-flex flex-wrap justify-content-between align-items-center gap-3">
      <div>
        <h4 class="fw-bold text-dark mb-1"><i class="bi bi-graph-up-arrow text-warning me-2"></i>Báo Cáo & Thống Kê</h4>
        <p class="text-muted small mb-0">Theo dõi doanh thu phòng khám và hiệu suất làm việc của bác sĩ</p>
      </div>
      
      <div class="d-flex align-items-center gap-3">
        <div class="d-flex align-items-center gap-2">
          <label class="small text-muted fw-bold text-nowrap mb-0">Từ ngày</label>
          <input type="date" v-model="startDate" class="form-control form-control-sm rounded-3 shadow-sm border-light" />
        </div>
        <div class="d-flex align-items-center gap-2">
          <label class="small text-muted fw-bold text-nowrap mb-0">Đến ngày</label>
          <input type="date" v-model="endDate" class="form-control form-control-sm rounded-3 shadow-sm border-light" />
        </div>
        <button class="btn btn-warning btn-sm px-3 fw-bold rounded-pill shadow-sm text-dark d-flex align-items-center gap-1" @click="loadReport" :disabled="loading">
          <i v-if="loading" class="spinner-border spinner-border-sm me-1" role="status"></i>
          <i v-else class="bi bi-filter"></i> Lọc dữ liệu
        </button>
      </div>
    </div>

    <!-- Error state -->
    <div v-if="errorMsg" class="alert alert-danger rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Toast Notification (auto-dismiss) -->
    <Transition name="toast-slide">
      <div v-if="showToast" class="position-fixed top-0 end-0 p-3" style="z-index: 1055">
        <div class="toast align-items-center text-white border-0 show" :class="toastType === 'toast-success' ? 'bg-success' : 'bg-danger'" role="alert" aria-live="assertive" aria-atomic="true">
          <div class="d-flex">
            <div class="toast-body">
              <i :class="toastType === 'toast-success' ? 'bi-check-circle-fill' : 'bi-exclamation-circle-fill'" class="me-2"></i>
              {{ toastMessage }}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" @click="showToast = false" aria-label="Close"></button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Stats summary cards -->
    <div class="row g-4 mb-4">
      <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-4 p-4 text-white bg-emerald-gradient position-relative overflow-hidden">
          <div class="position-absolute end-0 bottom-0 p-3 opacity-25">
            <i class="bi bi-wallet2" style="font-size: 5rem;"></i>
          </div>
          <h6 class="text-white-50 text-uppercase fw-bold small mb-2">Tổng Doanh Thu</h6>
          <h2 class="fw-bold mb-1">{{ formatCurrency(reportData?.totalRevenue ?? 0) }}</h2>
          <span class="small text-white-50"><i class="bi bi-check-circle-fill me-1"></i>Từ các hóa đơn đã thanh toán</span>
        </div>
      </div>

      <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-4 p-4 text-white bg-blue-gradient position-relative overflow-hidden">
          <div class="position-absolute end-0 bottom-0 p-3 opacity-25">
            <i class="bi bi-file-earmark-ruled" style="font-size: 5rem;"></i>
          </div>
          <h6 class="text-white-50 text-uppercase fw-bold small mb-2">Số Hóa Đơn Đã Thanh Toán</h6>
          <h2 class="fw-bold mb-1">{{ reportData?.invoicesCount ?? 0 }} <span class="fs-6 fw-normal text-white-50">hóa đơn</span></h2>
          <span class="small text-white-50"><i class="bi bi-receipt me-1"></i>Hóa đơn đã xác nhận tại quầy</span>
        </div>
      </div>

      <div class="col-md-4">
        <div class="card border-0 shadow-sm rounded-4 p-4 text-white bg-gold-gradient position-relative overflow-hidden">
          <div class="position-absolute end-0 bottom-0 p-3 opacity-25">
            <i class="bi bi-journal-medical" style="font-size: 5rem;"></i>
          </div>
          <h6 class="text-white-50 text-uppercase fw-bold small mb-2">Số Ca Khám Hoàn Thành</h6>
          <h2 class="fw-bold mb-1">
            {{ totalCompletedAppointments }} <span class="fs-6 fw-normal text-white-50">ca khám</span>
          </h2>
          <span class="small text-white-50"><i class="bi bi-person-check-fill me-1"></i>Bác sĩ đã đóng bệnh án</span>
        </div>
      </div>
    </div>

    <!-- Charts and Tables -->
    <div class="row g-4">
      <!-- Chart section -->
      <div class="col-lg-8">
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100">
          <h5 class="fw-bold text-dark mb-4"><i class="bi bi-activity text-success me-2"></i>Biểu Đồ Doanh Thu Theo Ngày</h5>
          <div class="chart-wrapper" style="overflow-x: auto; overflow-y: hidden;">
            <div class="chart-container" :style="{ width: chartDynamicWidth, height: '350px', minWidth: '100%' }">
              <canvas ref="revenueChartCanvas"></canvas>
            </div>
          </div>
        </div>
      </div>

      <!-- Doctor performance ranking -->
      <div class="col-lg-4">
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100">
          <h5 class="fw-bold text-dark mb-3"><i class="bi bi-trophy text-warning me-2"></i>Hiệu Suất Bác Sĩ</h5>
          <p class="text-muted small mb-4">Danh sách số ca khám bệnh đã hoàn thành trong kỳ</p>
          <div class="table-responsive">
            <table class="table align-middle table-hover mb-0">
              <tbody>
                <tr v-for="(doc, idx) in reportData?.doctorPerformance" :key="idx">
                  <td width="40">
                    <span class="badge rounded-circle p-2 text-dark font-monospace d-flex align-items-center justify-content-center" 
                          :class="idx === 0 ? 'bg-warning' : (idx === 1 ? 'bg-light border border-warning' : 'bg-light')"
                          style="width: 28px; height: 28px; font-size: 0.85rem;">
                      {{ Number(idx) + 1 }}
                    </span>
                  </td>
                  <td>
                    <span class="fw-bold text-dark">{{ doc.doctorName }}</span>
                  </td>
                  <td class="text-end">
                    <span class="badge bg-success-subtle text-success px-3 py-1 rounded-pill fw-bold">
                      {{ doc.completedAppointments }} ca
                    </span>
                  </td>
                </tr>
                <tr v-if="!reportData?.doctorPerformance || reportData.doctorPerformance.length === 0">
                  <td colspan="3" class="text-center py-5 text-muted small">
                    <i class="bi bi-person-slash fs-2 d-block mb-2 text-black-50 opacity-25"></i>
                    Không có ca khám nào hoàn thành trong khoảng thời gian này
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Service & Product Breakdown -->
      <div class="col-12">
        <div class="row g-4">
          <!-- Services Table -->
          <div class="col-md-6">
            <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100">
              <h5 class="fw-bold text-dark mb-4"><i class="bi bi-heart-pulse-fill text-danger me-2"></i>Chi Tiết Doanh Thu Dịch Vụ</h5>
              <div class="table-responsive">
                <table class="table align-middle border-bottom mb-0">
                  <thead class="table-light">
                    <tr>
                      <th class="py-3 ps-4 border-0 text-muted small">Tên dịch vụ</th>
                      <th class="py-3 border-0 text-muted small text-center">Số lượng</th>
                      <th class="py-3 border-0 text-muted small text-end pe-4">Doanh thu</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(item, idx) in serviceOnlyRevenue" :key="'s'+idx">
                      <td class="py-3 ps-4 border-0 fw-bold text-dark">{{ item.serviceName }}</td>
                      <td class="py-3 border-0 text-center text-muted">{{ item.count }}</td>
                      <td class="py-3 border-0 text-end pe-4 text-emerald fw-bold">{{ formatCurrency(item.amount) }}</td>
                    </tr>
                    <tr v-if="serviceOnlyRevenue.length === 0">
                      <td colspan="3" class="text-center py-5 text-muted small">
                        <i class="bi bi-inbox fs-2 d-block mb-2 text-black-50 opacity-25"></i>
                        Chưa có doanh thu dịch vụ
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
          
          <!-- Medicines Table -->
          <div class="col-md-6">
            <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100">
              <h5 class="fw-bold text-dark mb-4"><i class="bi bi-capsule text-primary me-2"></i>Chi Tiết Doanh Thu Thuốc</h5>
              <div class="table-responsive">
                <table class="table align-middle border-bottom mb-0">
                  <thead class="table-light">
                    <tr>
                      <th class="py-3 ps-4 border-0 text-muted small">Tên thuốc</th>
                      <th class="py-3 border-0 text-muted small text-center">Số lượng</th>
                      <th class="py-3 border-0 text-muted small text-end pe-4">Doanh thu</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(item, idx) in medicineOnlyRevenue" :key="'m'+idx">
                      <td class="py-3 ps-4 border-0 fw-bold text-dark">{{ item.serviceName }}</td>
                      <td class="py-3 border-0 text-center text-muted">{{ item.count }}</td>
                      <td class="py-3 border-0 text-end pe-4 text-emerald fw-bold">{{ formatCurrency(item.amount) }}</td>
                    </tr>
                    <tr v-if="medicineOnlyRevenue.length === 0">
                      <td colspan="3" class="text-center py-5 text-muted small">
                        <i class="bi bi-inbox fs-2 d-block mb-2 text-black-50 opacity-25"></i>
                        Chưa có doanh thu thuốc kê đơn
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

// Default dates: last 30 days
const today = new Date();
const pastDate = new Date(new Date().setDate(today.getDate() - 30));

const startDate = ref<string>(pastDate.toISOString().split('T')[0]);
const endDate = ref<string>(today.toISOString().split('T')[0]);

const loading = ref(false);
const errorMsg = ref('');
const reportData = ref<any>(null);

const revenueChartCanvas = ref<HTMLCanvasElement | null>(null);
let chartInstance: Chart | null = null;
const chartDynamicWidth = ref('100%');

// Toast Notification State
const showToast = ref(false);
const toastMessage = ref('');
const toastType = ref<'toast-success' | 'toast-error'>('toast-success');
let toastTimer: ReturnType<typeof setTimeout> | null = null;

const triggerToast = (message: string, type: 'toast-success' | 'toast-error' = 'toast-success') => {
  if (toastTimer) clearTimeout(toastTimer);
  toastMessage.value = message;
  toastType.value = type;
  showToast.value = true;
  toastTimer = setTimeout(() => {
    showToast.value = false;
  }, 3000);
};

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

const totalCompletedAppointments = computed(() => {
  if (!reportData.value || !reportData.value.doctorPerformance) return 0;
  return reportData.value.doctorPerformance.reduce((acc: number, cur: any) => acc + Number(cur.completedAppointments), 0);
});

const serviceOnlyRevenue = computed(() => {
  if (!reportData.value?.serviceRevenue) return [];
  return reportData.value.serviceRevenue.filter((s: any) => s.itemType?.toLowerCase() === 'service');
});

const medicineOnlyRevenue = computed(() => {
  if (!reportData.value?.serviceRevenue) return [];
  return reportData.value.serviceRevenue.filter((s: any) => s.itemType?.toLowerCase() === 'medicine');
});

const loadReport = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/admin/reports/revenue', {
      params: {
        startDate: startDate.value,
        endDate: endDate.value
      }
    });
    reportData.value = res.data;
    renderChart();
    triggerToast('Đã cập nhật dữ liệu báo cáo thành công!', 'toast-success');
  } catch (err: any) {
    console.error('Không thể tải báo cáo doanh thu:', err);
    errorMsg.value = err.response?.data?.message || 'Lỗi hệ thống khi tải báo cáo.';
  } finally {
    loading.value = false;
  }
};

const renderChart = () => {
  if (!revenueChartCanvas.value || !reportData.value) return;

  if (chartInstance) {
    chartInstance.destroy();
  }

  const labels: string[] = [];
  const data: number[] = [];

  if (startDate.value && endDate.value) {
    const start = new Date(startDate.value + 'T00:00:00');
    const end = new Date(endDate.value + 'T00:00:00');
    
    const revenueMap = new Map();
    if (reportData.value?.dailyRevenue) {
      reportData.value.dailyRevenue.forEach((d: any) => {
        const dateKey = new Date(d.date).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
        revenueMap.set(dateKey, d.amount);
      });
    }

    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
      const fullDateKey = d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
      const label = d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
      labels.push(label);
      data.push(revenueMap.get(fullDateKey) || 0);
    }
  }
  
  // Calculate dynamic width based on number of bars (approx 60px per bar)
  const minRequiredWidth = labels.length * 60;
  chartDynamicWidth.value = minRequiredWidth > 800 ? `${minRequiredWidth}px` : '100%';

  chartInstance = new Chart(revenueChartCanvas.value, {
    type: 'bar',
    data: {
      labels,
      datasets: [
        {
          label: 'Doanh thu hàng ngày (VND)',
          data,
          backgroundColor: 'rgba(16, 185, 129, 0.85)',
          hoverBackgroundColor: '#10b981',
          borderRadius: 6,
          barPercentage: 0.7,
          categoryPercentage: 0.9
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: '#1f2937',
          titleFont: { size: 13, weight: 'bold' },
          bodyFont: { size: 14 },
          padding: 12,
          cornerRadius: 8,
          callbacks: {
            label: function(context: any) {
              return ' Doanh thu: ' + formatCurrency(context.raw);
            }
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          grid: {
            color: 'rgba(0, 0, 0, 0.05)'
          },
          ticks: {
            callback: function(value: any) {
              if (value >= 1000000) return (value / 1000000) + ' Tr';
              if (value >= 1000) return (value / 1000) + ' K';
              return value;
            }
          }
        },
        x: {
          grid: {
            display: false
          },
          ticks: {
            maxRotation: 0,
            minRotation: 0,
            autoSkip: false
          }
        }
      }
    }
  });
};

onMounted(() => {
  loadReport();
});
</script>

<style scoped>
.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 16px;
}

.bg-emerald-gradient {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
}

.bg-blue-gradient {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
}

.bg-gold-gradient {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.chart-wrapper {
  scrollbar-width: thin;
  scrollbar-color: #cbd5e1 transparent;
}
.chart-wrapper::-webkit-scrollbar {
  height: 8px;
}
.chart-wrapper::-webkit-scrollbar-track {
  background: transparent;
}
.chart-wrapper::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 10px;
}
.chart-container {
  position: relative;
}
</style>
