<template>
  <div class="news-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <div class="news-container">
      <!-- Back button -->
      <div class="action-bar">
        <router-link to="/" class="btn-back">
          <ArrowLeft class="icon-btn" />
          <span>Quay lại Trang chủ</span>
        </router-link>
      </div>

      <!-- Header Section -->
      <div class="news-header">
        <span class="section-tag">Cẩm nang nuôi pet</span>
        <h1 class="gradient-text">Tin Tức & Kinh Nghiệm</h1>
        <p class="news-subtitle">
          Tổng hợp những bài viết hữu ích từ đội ngũ bác sĩ chuyên khoa giúp bạn có thêm kiến thức khoa học để nuôi dạy và bảo vệ sức khỏe cho thú cưng yêu quý.
        </p>
      </div>

      <!-- Filter Tabs -->
      <div class="filter-tabs">
        <button 
          v-for="tab in filterTabs" 
          :key="tab" 
          @click="selectedTag = tab" 
          :class="['filter-btn', { active: selectedTag === tab }]"
        >
          {{ tab }}
        </button>
      </div>

      <!-- Articles Grid -->
      <div class="news-grid">
        <div v-for="article in filteredArticles" :key="article.id" class="news-card">
          <div class="news-img-wrapper">
            <img :src="article.image" :alt="article.title" class="news-img" />
            <span class="news-tag" :style="{ background: getTagColor(article.tag) }">
              {{ article.tag }}
            </span>
          </div>
          <div class="news-info">
            <span class="news-date">{{ article.date }}</span>
            <h3 class="news-title" @click="openArticle(article)">{{ article.title }}</h3>
            <p class="news-excerpt">{{ article.excerpt }}</p>
            <button @click="openArticle(article)" class="btn-read-more">
              Đọc chi tiết <ChevronRight class="icon-right" />
            </button>
          </div>
        </div>
      </div>

    </div>

    <!-- Article Detail Modal -->
    <div v-if="activeArticle" class="modal-overlay" @click.self="activeArticle = null">
      <div class="modal-card">
        <button class="modal-close" @click="activeArticle = null">
          <X />
        </button>
        <div class="modal-img-wrapper">
          <img :src="activeArticle.image" alt="Article image" class="modal-img" />
          <span class="modal-tag" :style="{ background: getTagColor(activeArticle.tag) }">
            {{ activeArticle.tag }}
          </span>
        </div>
        <span class="modal-date">{{ activeArticle.date }}</span>
        <h2 class="modal-title">{{ activeArticle.title }}</h2>
        
        <div class="modal-body">
          <p v-for="(pText, idx) in activeArticle.paragraphs" :key="idx">
            {{ pText }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { ArrowLeft, ChevronRight, X } from '@lucide/vue';

const selectedTag = ref('Tất cả');
const activeArticle = ref<any | null>(null);

const filterTabs = ['Tất cả', 'Sức khỏe', 'Cẩm nang', 'Dinh dưỡng'];

const articles = ref([
  {
    id: 1,
    title: 'Lịch tiêm phòng dại định kỳ cho chó mèo bạn cần biết',
    tag: 'Sức khỏe',
    date: '05 Tháng 6, 2026',
    image: 'https://images.unsplash.com/photo-1581888227599-779811939961?auto=format&fit=crop&w=400&h=250',
    excerpt: 'Bệnh dại là căn bệnh vô cùng nguy hiểm và có khả năng lây sang người. Tìm hiểu lịch tiêm phòng chuẩn xác nhất để bảo vệ pet cưng của bạn...',
    paragraphs: [
      'Bệnh dại (Rabies) là bệnh truyền nhiễm virus cấp tính của hệ thần kinh trung ương, lây từ động vật sang người thông qua vết cắn, vết cào. Đây là căn bệnh cực kỳ nguy hiểm, một khi đã lên cơn dại thì tỷ lệ tử vong là 100%.',
      'Để bảo vệ thú cưng cũng như bản thân và gia đình, chủ nuôi bắt buộc phải cho chó mèo đi tiêm vaccine phòng dại định kỳ. Mũi tiêm đầu tiên nên thực hiện khi thú cưng đạt 3 tháng tuổi. Sau đó, cần tiêm nhắc lại đều đặn mỗi năm một lần.',
      'Lưu ý: Chỉ thực hiện tiêm vaccine khi thú cưng hoàn toàn khỏe mạnh, không bị sốt hay đang điều trị bệnh lý nào khác. Sau khi tiêm nên theo dõi tại phòng khám khoảng 15-30 phút để đề phòng sốc phản vệ.'
    ]
  },
  {
    id: 2,
    title: 'Cách chăm sóc thú cưng vào mùa hè nắng nóng tránh sốc nhiệt',
    tag: 'Cẩm nang',
    date: '28 Tháng 5, 2026',
    image: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=400&h=250',
    excerpt: 'Thời tiết nắng nóng của mùa hè rất dễ khiến chó mèo bị mất nước và sốc nhiệt dẫn đến đột quỵ. Hãy áp dụng các bí quyết chăm sóc sau...',
    paragraphs: [
      'Sốc nhiệt là tình trạng khẩn cấp xảy ra khi nhiệt độ cơ thể thú cưng tăng cao vượt ngưỡng an toàn (thường trên 40 độ C), khiến cơ thể không kịp tản nhiệt. Điều này rất dễ xảy ra trong những ngày hè oi bức tại Việt Nam.',
      'Các dấu hiệu sốc nhiệt dễ nhận biết bao gồm: thở gấp gáp, chảy nhiều nước dãi, nướu đỏ sẫm hoặc xanh tím, đi đứng lảo đảo và lờ đờ. Nếu không sơ cứu kịp thời có thể dẫn đến suy đa tạng và tử vong.',
      'Biện pháp phòng ngừa hiệu quả: Luôn cung cấp đủ nước sạch mát, giữ pet trong không gian thoáng gió hoặc phòng điều hòa vào khung giờ nắng nóng cao điểm. Tuyệt đối không để thú cưng một mình trong xe ô tô đóng kín cửa.'
    ]
  },
  {
    id: 3,
    title: 'Chế độ dinh dưỡng khoa học giúp mèo cưng bóng mượt lông',
    tag: 'Dinh dưỡng',
    date: '15 Tháng 5, 2026',
    image: 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=400&h=250',
    excerpt: 'Một bộ lông mèo bóng mượt phản ánh thể trạng sức khỏe tốt. Các vitamin, axit béo Omega-3 và thực đơn lý tưởng cho mèo...',
    paragraphs: [
      'Bộ lông của mèo là tấm gương phản chiếu trực tiếp tình trạng sức khỏe bên trong của chúng. Một chế độ ăn thiếu hụt dinh dưỡng sẽ khiến lông mèo bị xơ rối, dễ gãy rụng và da khô ráp.',
      'Để giúp lông mèo luôn mềm mại và bóng mượt, bạn cần bổ sung các thực phẩm giàu Axit béo Omega-3 và Omega-6 có nhiều trong cá hồi, dầu cá và các loại hạt. Ngoài ra, Protein chất lượng cao chiếm tỷ lệ lớn trong chế độ ăn hàng ngày là cốt lõi xây dựng chất sừng cho lông.',
      'Đừng quên cung cấp đầy đủ nước sạch để mèo cưng giải độc cơ thể và duy trì độ ẩm cho da. Chải lông hàng ngày cũng là phương pháp tốt kích thích tuần hoàn máu nuôi dưỡng nang lông.'
    ]
  }
]);

const filteredArticles = computed(() => {
  if (selectedTag.value === 'Tất cả') return articles.value;
  return articles.value.filter(art => art.tag === selectedTag.value);
});

const getTagColor = (tag: string) => {
  if (tag === 'Sức khỏe') return '#0d9488';
  if (tag === 'Cẩm nang') return '#6366f1';
  if (tag === 'Dinh dưỡng') return '#f59e0b';
  return '#14b8a6';
};

const openArticle = (article: any) => {
  activeArticle.value = article;
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.news-wrapper {
  position: relative;
  min-height: 100vh;
  background: radial-gradient(circle at top right, #1e293b, #0f172a, #0b0f19);
  font-family: 'Outfit', sans-serif;
  color: #f8fafc;
  overflow: hidden;
  padding: 2.5rem 1.5rem;
}

.bg-glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.12;
  z-index: 0;
  pointer-events: none;
}

.bg-glow-1 {
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, #0d9488, transparent);
  top: -100px;
  right: -100px;
}

.bg-glow-2 {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, #6366f1, transparent);
  bottom: -50px;
  left: -50px;
}

.news-container {
  position: relative;
  z-index: 1;
  max-width: 1100px;
  margin: 0 auto;
}

.action-bar {
  margin-bottom: 2.5rem;
}

.btn-back {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 0.6rem 1.2rem;
  border-radius: 10px;
  color: #cbd5e1;
  font-weight: 500;
  cursor: pointer;
  text-decoration: none;
  transition: all 0.3s;
}

.btn-back:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
  transform: translateX(-4px);
}

.icon-btn {
  width: 18px;
  height: 18px;
}

.news-header {
  text-align: center;
  margin-bottom: 3rem;
}

.section-tag {
  color: #14b8a6;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 2px;
  background: rgba(20, 184, 166, 0.1);
  padding: 0.4rem 1rem;
  border-radius: 20px;
  margin-bottom: 1rem;
  display: inline-block;
}

.gradient-text {
  font-size: 2.8rem;
  font-weight: 800;
  letter-spacing: -1px;
  margin-bottom: 1.2rem;
  background: linear-gradient(135deg, #14b8a6, #6366f1);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.news-subtitle {
  color: #94a3b8;
  font-size: 1.05rem;
  line-height: 1.7;
  max-width: 700px;
  margin: 0 auto;
}

/* Filter tabs styling */
.filter-tabs {
  display: flex;
  justify-content: center;
  gap: 12px;
  margin-bottom: 3.5rem;
  flex-wrap: wrap;
}

.filter-btn {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  color: #cbd5e1;
  padding: 0.6rem 1.4rem;
  border-radius: 12px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.filter-btn:hover {
  background: rgba(255, 255, 255, 0.08);
  color: white;
}

.filter-btn.active {
  background: #14b8a6;
  color: white;
  border-color: #14b8a6;
  box-shadow: 0 4px 12px rgba(20, 184, 166, 0.3);
}

/* Articles grid */
.news-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 2.5rem;
  margin-bottom: 4rem;
}

.news-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 20px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.2);
}

.news-img-wrapper {
  position: relative;
  height: 200px;
  overflow: hidden;
}

.news-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s;
}

.news-card:hover .news-img {
  transform: scale(1.05);
}

.news-tag {
  position: absolute;
  top: 15px;
  left: 15px;
  color: white;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.3rem 0.8rem;
  border-radius: 20px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
}

.news-info {
  padding: 1.8rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.news-date {
  font-size: 0.8rem;
  color: #64748b;
  margin-bottom: 0.6rem;
}

.news-title {
  font-size: 1.15rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.8rem;
  line-height: 1.4;
  cursor: pointer;
  transition: color 0.2s;
}

.news-title:hover {
  color: #14b8a6;
}

.news-excerpt {
  color: #94a3b8;
  font-size: 0.85rem;
  line-height: 1.6;
  margin-bottom: 1.5rem;
  flex-grow: 1;
}

.btn-read-more {
  background: none;
  border: none;
  color: #14b8a6;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
  align-self: flex-start;
  padding: 0;
  transition: color 0.2s;
}

.btn-read-more:hover {
  color: #2dd4bf;
}

.icon-right {
  width: 16px;
  height: 16px;
  transition: transform 0.2s;
}

.btn-read-more:hover .icon-right {
  transform: translateX(4px);
}

/* Modal detail styling */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(8px);
  z-index: 10000;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
}

.modal-card {
  background: #1e293b;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 24px;
  width: 100%;
  max-width: 680px;
  position: relative;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
  animation: modal-enter 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

@keyframes modal-enter {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.modal-close {
  position: absolute;
  top: 20px;
  right: 20px;
  background: rgba(15, 23, 42, 0.6);
  border: none;
  color: white;
  cursor: pointer;
  padding: 5px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  z-index: 10;
}

.modal-close:hover {
  background: rgba(15, 23, 42, 0.9);
}

.modal-img-wrapper {
  position: relative;
  height: 280px;
  flex-shrink: 0;
}

.modal-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.modal-tag {
  position: absolute;
  bottom: 20px;
  left: 20px;
  color: white;
  font-weight: 600;
  padding: 0.4rem 1rem;
  border-radius: 20px;
  font-size: 0.8rem;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.25);
}

.modal-date {
  color: #64748b;
  font-size: 0.85rem;
  margin: 1.5rem 2rem 0.5rem 2rem;
  display: block;
}

.modal-card .modal-title {
  margin: 0 2rem 1.2rem 2rem;
  font-size: 1.5rem;
  font-weight: 700;
  line-height: 1.4;
  color: white;
}

.modal-body {
  padding: 0 2rem 2.5rem 2rem;
  overflow-y: auto;
  color: #cbd5e1;
  font-size: 0.95rem;
  line-height: 1.7;
}

.modal-body p {
  margin-bottom: 1rem;
}
</style>
