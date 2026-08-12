<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <div class="container py-5 mt-5">
      <div class="row justify-content-center">
        <div class="col-lg-8">
          <div class="mb-4">
            <button @click="goBack" class="btn-back text-decoration-none text-muted hover-text-warning">
              <ArrowLeft class="icon-back me-1" /> Quay lại bản tin
            </button>
          </div>

          <div v-if="loading" class="text-center py-5">
            <div class="spinner-border text-warning" role="status"></div>
            <p class="text-muted mt-2">Đang tải bài viết...</p>
          </div>
          <div v-else-if="article">
            <!-- Article Header -->
            <div class="mt-4 mb-3">
              <span class="badge" :class="article.badgeClass">{{ article.category }}</span>
              <span v-for="tag in article.tags" :key="tag" class="badge bg-light text-secondary border fw-normal ms-1">
                <Tag class="tag-icon me-1" />{{ tag }}
              </span>
            </div>
            
            <h1 class="fw-bold mb-3 article-title">{{ article.title }}</h1>
            
            <div class="d-flex align-items-center gap-2 mb-4 text-muted small border-bottom pb-3">
              <span><CalendarDays class="meta-icon" /> {{ article.date }}</span>
              <span>•</span>
              <span><User class="meta-icon" /> {{ article.author }}</span>
            </div>

            <!-- Article Image -->
            <div class="article-image-wrapper mb-5">
              <img :src="article.image" class="img-fluid article-hero-img" :alt="article.title" />
            </div>

            <!-- Article Content -->
            <div class="article-content text-muted lh-lg fs-5" v-html="article.content"></div>
          </div>

          <!-- Suggestion widget -->
          <div class="mt-5 pt-5 border-top">
            <h4 class="fw-bold mb-4"><Sparkles class="text-warning me-2" />Có thể bạn quan tâm</h4>
            <div class="row g-4">
              <div v-for="suggest in suggestions" :key="suggest.id" class="col-md-6">
                <router-link :to="'/article/' + suggest.id" class="text-decoration-none">
                  <div class="card border-0 shadow-sm h-100 hover-shadow transition-all suggestion-card">
                    <div class="row g-0 h-100">
                      <div class="col-4">
                        <img :src="suggest.image" class="img-fluid rounded-start h-100 suggest-img" style="object-fit: cover;" :alt="suggest.title">
                      </div>
                      <div class="col-8">
                        <div class="card-body py-2 px-3">
                          <h6 class="card-title fw-bold text-dark mb-1 lh-sm hover-text-warning suggestion-title">{{ suggest.shortTitle }}</h6>
                          <small class="text-muted"><Clock class="meta-icon" />3 phút đọc</small>
                        </div>
                      </div>
                    </div>
                  </div>
                </router-link>
              </div>
            </div>
          </div>

        </div>
      </div>
    </div>

    <Footer />

    <BookingModal 
      :show="showBookingModal" 
      @close="showBookingModal = false" 
      @success="handleBookingSuccess" 
      @error="handleBookingError" 
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
import { ArrowLeft, Tag, CalendarDays, User, Clock, Sparkles } from 'lucide-vue-next';
import api from '../services/api';

const route = useRoute();
const router = useRouter();
const showBookingModal = ref(false);
const dbArticle = ref<any>(null);
const loading = ref(false);

const articleId = computed(() => {
  const idVal = Number(route.params.id);
  return isNaN(idVal) ? 1 : idVal;
});

const goBack = () => {
  if (window.history.length > 1) {
    router.back();
  } else {
    router.push('/news');
  }
};

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};

// Raw articles data matching Article.cshtml
const articlesList = [
  {
    id: 1,
    category: 'Sức khỏe',
    badgeClass: 'bg-danger text-white',
    tags: ['Nấm da', 'Lây nhiễm', 'Bệnh da liễu'],
    title: 'Nấm da ở chó mèo có lây sang người không?',
    shortTitle: 'Nấm da ở chó mèo lây sang người?',
    date: '25/05/2026',
    author: 'Bác sĩ MyPetClinic',
    image: 'https://images.unsplash.com/photo-1596492784531-6e6eb5ea9993?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Nấm da (Ringworm) là một trong những căn bệnh da liễu cực kỳ phổ biến ở chó mèo, đặc biệt là trong môi trường thời tiết nóng ẩm. Triệu chứng dễ nhận biết nhất là thú cưng bị rụng lông thành từng mảng tròn, da mẩn đỏ, có vảy sừng và ngứa ngáy liên tục.</p>
      <p>Điều đáng lo ngại là bệnh này không chỉ làm thú cưng khó chịu, mất thẩm mỹ mà khả năng lây lan trong môi trường sống là rất cao. Bào tử nấm có thể tồn tại trong nhà, trên thảm, ghế sofa và giường ngủ lên đến 18 tháng nếu không được tiêu diệt hoàn toàn.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Nấm da có lây sang người không?</h4>
      <p>Câu trả lời là <strong>CÓ</strong>. Nấm da là một bệnh truyền nhiễm có thể lây từ động vật sang người thông qua tiếp xúc trực tiếp hoặc gián tiếp qua đồ dùng (chăn nệm, lược chải lông). Trẻ em, người lớn tuổi, hoặc những người có hệ miễn dịch yếu là nhóm đối tượng có nguy cơ lây nhiễm cao nhất. Khi bị lây, người bệnh sẽ xuất hiện các đốm đỏ hình tròn, ngứa ngáy dữ dội.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Cách phòng ngừa và điều trị hiệu quả</h4>
      <ul>
        <li><strong>Cách ly ngay lập tức:</strong> Tách thú cưng bị bệnh khỏi trẻ em và các vật nuôi khác trong nhà.</li>
        <li><strong>Khám thú y:</strong> Đưa bé đến phòng khám thú y để được bác sĩ lấy mẫu xét nghiệm, soi đèn mộc nhĩ và kê đơn thuốc bôi/uống (thuốc kháng nấm) phù hợp.</li>
        <li><strong>Vệ sinh môi trường:</strong> Giặt giũ toàn bộ chăn, nệm, đồ chơi của thú cưng bằng nước nóng. Lau nhà bằng dung dịch sát khuẩn chuyên dụng.</li>
        <li><strong>Tắm rửa định kỳ:</strong> Sử dụng sữa tắm trị nấm chuyên dụng theo chỉ định (thường 1-2 lần/tuần).</li>
      </ul>
      <div class="bg-light p-4 rounded-3 border-start border-warning border-5 mt-4">
        <p class="mb-0"><i>Lưu ý quan trọng: Tuyệt đối không tự ý mua các loại thuốc bôi da của người hoặc các loại lá dân gian để bôi cho thú cưng nếu chưa có sự chỉ định của bác sĩ thú y để tránh tình trạng bệnh nặng hơn hoặc ngộ độc thuốc do thú cưng liếm phải.</i></p>
      </div>
    `
  },
  {
    id: 2,
    category: 'Kinh nghiệm',
    badgeClass: 'bg-warning text-dark',
    tags: ['Rụng lông', 'Kinh nghiệm', 'Chăm sóc chó'],
    title: 'Tại sao chó bị rụng lông và cách điều trị hiệu quả',
    shortTitle: 'Tại sao chó bị rụng lông nhiều?',
    date: '24/05/2026',
    author: 'Bác sĩ MyPetClinic',
    image: 'https://images.unsplash.com/photo-1581888227599-779811939961?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Rụng lông ở chó là một hiện tượng sinh lý bình thường diễn ra theo chu kỳ (thường là vào mùa xuân và mùa thu). Tuy nhiên, nếu lượng lông rụng quá nhiều, rụng thành từng mảng lộ cả da non, kèm theo các triệu chứng như ngứa ngáy, gãi liên tục, lở loét hoặc da có mùi hôi, thì đó có thể là dấu hiệu cảnh báo vấn đề sức khỏe.</p>
      <p>Ngoài hiện tượng sinh lý, tình trạng stress, mang thai hoặc thay đổi môi trường sống đột ngột cũng có thể làm cho chó bị căng thẳng, dẫn đến rụng lông trên diện rộng.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Các nguyên nhân y khoa phổ biến</h4>
      <ul>
        <li><strong>Thiếu hụt dinh dưỡng:</strong> Chế độ ăn nghèo nàn, thiếu protein, vitamin (đặc biệt là Biotin, Vitamin E) và khoáng chất thiết yếu.</li>
        <li><strong>Nhiễm ký sinh trùng:</strong> Ve, rận, bọ chét hoặc bệnh ghẻ Demodex/Sarcoptes phá hủy nang lông.</li>
        <li><strong>Dị ứng:</strong> Dị ứng với thành phần thức ăn (như gà, bò), thời tiết, hoặc các sản phẩm sữa tắm hóa chất mạnh không phù hợp.</li>
        <li><strong>Rối loạn nội tiết:</strong> Hội chứng Cushing hoặc suy giáp cũng là nguyên nhân gây rụng lông mất kiểm soát.</li>
      </ul>
      <h4 class="fw-bold text-dark mt-4 mb-3">Giải pháp khắc phục và chăm sóc</h4>
      <p>Bổ sung ngay các loại thực phẩm giàu Omega 3-6 (như dầu cá hồi, hạt lanh, dầu dừa) vào khẩu phần ăn hàng ngày để nuôi dưỡng da lông từ bên trong. Thường xuyên chải lông để loại bỏ lông rụng và massage kích thích nang lông phát triển.</p>
      <div class="bg-light p-4 rounded-3 border-start border-warning border-5 mt-4">
        <p class="mb-0 text-danger fw-bold">Nếu chó gãi nhiều, da ửng đỏ, có vảy hoặc chảy dịch, hãy đặt lịch khám tại MyPetClinic để bác sĩ kiểm tra chuyên sâu, soi da và có phác đồ điều trị kịp thời.</p>
      </div>
    `
  },
  {
    id: 3,
    category: 'Dinh dưỡng',
    badgeClass: 'bg-info text-white',
    tags: ['Táo bón', 'Tiêu hóa', 'Dinh dưỡng'],
    title: 'Chó bị táo bón: Biểu hiện và cách điều trị tại nhà',
    shortTitle: 'Chó bị táo bón: Biểu hiện và xử lý',
    date: '20/05/2026',
    author: 'Bác sĩ MyPetClinic',
    image: 'https://images.unsplash.com/photo-1544568100-847a948585b9?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Táo bón là một trong những vấn đề tiêu hóa thường gặp nhất ở chó. Mặc dù không phải là bệnh nan y, nhưng nếu để kéo dài có thể gây ra hiện tượng phình đại tràng, tổn thương niêm mạc ruột, hấp thu ngược độc tố và làm thú cưng suy nhược cơ thể nghiêm trọng.</p>
      <p>Nguyên nhân gây táo bón rất đa dạng: từ việc chế độ ăn quá nghèo nàn chất xơ, ăn quá nhiều xương cứng (đặc biệt là xương ống, xương cổ gà), thói quen uống ít nước, cho đến các vấn đề bệnh lý về tuyến hậu môn hoặc khối u trong đường ruột.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Dấu hiệu nhận biết rõ ràng</h4>
      <ul>
        <li>Chó đi vệ sinh rặn nhiều lần nhưng không ra phân, rên rỉ vì đau đớn.</li>
        <li>Phân rất cứng, nhỏ, khô và có thể lẫn một chút máu do nứt kẽ hậu môn.</li>
        <li>Bụng phình to, căng cứng, sờ vào thấy cứng và có biểu hiện cắn lại khi bị chạm vào.</li>
        <li>Chó lờ đờ, chán ăn, thỉnh thoảng nôn mửa do thức ăn và dịch dạ dày bị ứ đọng không tiêu thoát được.</li>
      </ul>
      <h4 class="fw-bold text-dark mt-4 mb-3">Chăm sóc và xử lý tại nhà an toàn</h4>
      <p>Điều đầu tiên cần làm là cung cấp đủ nước sạch, hoặc pha thêm chút mật ong/nước luộc gà để kích thích chó uống nhiều nước hơn. Bạn có thể trộn thêm một chút men tiêu hóa hoặc bí đỏ luộc nghiền nhuyễn (rất giàu chất xơ hòa tan) vào thức ăn mềm để giúp nhuận tràng.</p>
      <p>Nên ngừng hẳn việc cho chó ăn xương cứng trong giai đoạn này. Khuyến khích chó vận động chạy nhảy nhẹ nhàng từ 15-20 phút mỗi ngày để kích thích nhu động ruột hoạt động tự nhiên trở lại.</p>
      <div class="bg-light p-4 rounded-3 border-start border-danger border-5 mt-4">
        <p class="mb-0 text-danger fw-bold">Tuyệt đối không tự ý dùng thuốc thụt tháo của người cho chó. Nếu tình trạng táo bón kéo dài quá 2-3 ngày, vui lòng đưa cún cưng đến MyPetClinic ngay lập tức để bác sĩ can thiệp y tế (thụt tháo chuyên dụng hoặc siêu âm kiểm tra dị vật/khối u).</p>
      </div>
    `
  },
  {
    id: 4,
    category: 'Dinh dưỡng',
    badgeClass: 'bg-info text-white',
    tags: ['Mèo con', 'Taurine', 'Dinh dưỡng'],
    title: 'Chế độ dinh dưỡng khoa học cho mèo dưới 1 năm tuổi',
    shortTitle: 'Dinh dưỡng khoa học cho mèo con',
    date: '15/05/2026',
    author: 'Chuyên gia Dinh dưỡng MyPetClinic',
    image: 'https://images.unsplash.com/photo-1533738363-b7f9aef128ce?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Mèo dưới 1 năm tuổi (mèo con) đang ở trong giai đoạn tăng trưởng vàng. Ở độ tuổi này, tốc độ phát triển cơ xương khớp, hệ miễn dịch và trí não diễn ra vô cùng nhanh chóng. Do đó, nhu cầu năng lượng và hàm lượng dinh dưỡng của mèo con cao gấp 2-3 lần so với mèo trưởng thành.</p>
      <p>Một chế độ dinh dưỡng thiếu hụt trong giai đoạn này có thể dẫn đến tình trạng còi xương, suy dinh dưỡng, giảm sức đề kháng và dễ mắc các bệnh nhiễm trùng nguy hiểm.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Những dưỡng chất thiết yếu bắt buộc phải có</h4>
      <ul>
        <li><strong>Protein động vật chất lượng cao:</strong> Là thành phần cốt lõi xây dựng cơ bắp. Nên chọn các loại thịt như ức gà, cá phi lê, thịt bò, lòng đỏ trứng.</li>
        <li><strong>Taurine:</strong> Axit amin thiết yếu cực kỳ quan trọng đối với thị lực và sự phát triển cơ tim của mèo. Thiếu hụt Taurine có thể dẫn đến mù lòa.</li>
        <li><strong>Canxi và Phốt-pho:</strong> Giúp hình thành và làm chắc khỏe khung xương, răng của mèo con.</li>
        <li><strong>Axit béo Omega-3 (DHA/EPA):</strong> Hỗ trợ đắc lực cho sự phát triển trí não và võng mạc mắt, đồng thời nuôi dưỡng lông mềm mượt.</li>
      </ul>
      <h4 class="fw-bold text-dark mt-4 mb-3">Những thực phẩm tuyệt đối cấm kỵ</h4>
      <p>Tránh cho mèo con uống sữa bò (sữa đặc có đường của người) vì hệ tiêu hóa của mèo không dung nạp được Lactose, dễ gây tiêu chảy cấp tính nguy hiểm. Không cho ăn hành, tỏi, sô cô la, nho khô vì đây là những chất cực độc gây suy gan thận cấp.</p>
      <div class="bg-light p-4 rounded-3 border-start border-success border-5 mt-4">
        <p class="mb-0 text-success fw-bold">MyPetClinic khuyên dùng: Nên kết hợp giữa thức ăn hạt khô chuyên dụng cho mèo con (Kitten) và pate ướt để vừa bổ sung đủ nước chống sỏi thận, vừa cân bằng dinh dưỡng tuyệt đối cho sự phát triển toàn diện của bé.</p>
      </div>
    `
  },
  {
    id: 5,
    category: 'HOT EVENT',
    badgeClass: 'bg-danger text-white',
    tags: ['Tiêm phòng dại', 'Miễn phí', 'Cộng đồng'],
    title: 'Chiến Dịch Tiêm Vaccine Phòng Dại Miễn Phí Vì Cộng Đồng',
    shortTitle: 'Chiến dịch tiêm phòng dại miễn phí',
    date: '01/06/2026',
    author: 'Ban Tổ Chức MyPetClinic',
    image: 'https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Nhằm hưởng ứng ngày phòng chống bệnh dại toàn cầu và chung tay đẩy lùi nguy cơ lây nhiễm bệnh dại tại Việt Nam, hệ thống bệnh viện thú y MyPetClinic chính thức phát động chiến dịch <strong>"Tiêm Phòng Vaccine Dại Miễn Phí Vì Cộng Đồng"</strong>.</p>
      <p>Bệnh dại là bệnh truyền nhiễm virus cấp tính nguy hiểm, một khi đã khởi phát triệu chứng lâm sàng ở cả người và động vật thì tỷ lệ tử vong là gần như 100%. Tiêm phòng vaccine là biện pháp duy nhất và hiệu quả nhất để phòng tránh thảm kịch này.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Thông tin chi tiết chương trình</h4>
      <p>Thời gian diễn ra từ ngày 01/06/2026 đến hết ngày 15/06/2026 tại toàn bộ 3 chi nhánh của hệ thống bệnh viện MyPetClinic. Đối tượng là chó, mèo từ 3 tháng tuổi trở lên, có sức khỏe ổn định với số lượng giới hạn 1000 liều vaccine Rabisin (Pháp) chính hãng nhập khẩu.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Những lưu ý quan trọng trước khi tiêm phòng</h4>
      <p>Thú cưng mang đi tiêm cần có sức khỏe hoàn toàn bình thường, không sốt, không tiêu chảy, không mang thai cận ngày sinh. Sau khi tiêm, khách hàng sẽ được cấp Sổ tiêm phòng chuẩn có dấu đỏ xác nhận của bệnh viện và hướng dẫn theo dõi bé tại nhà trong 24 giờ đầu để đảm bảo an toàn tuyệt đối.</p>
      <div class="bg-light p-4 rounded-3 border-start border-danger border-5 mt-4">
        <p class="mb-0 text-danger fw-bold">Cách thức đăng ký: Khách hàng vui lòng đặt lịch trực tiếp qua hotline hoặc mang sổ khám bệnh cũ của bé tới chi nhánh MyPetClinic gần nhất để làm thủ tục check-in nhận vaccine miễn phí.</p>
      </div>
    `
  },
  {
    id: 6,
    category: 'Khuyến mãi',
    badgeClass: 'bg-warning text-dark',
    tags: ['Spa Grooming', 'Ưu đãi hè', 'Cắt lông'],
    title: 'Chào Hè Rực Rỡ - Ưu Đãi 20% Dịch Vụ Cắt Tỉa Lông (Grooming)',
    shortTitle: 'Chào hè rực rỡ - Ưu đãi 20% Grooming',
    date: '26/05/2026',
    author: 'Bộ phận Dịch vụ khách hàng',
    image: 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Mùa hè oi ả đã gõ cửa, thời tiết nóng bức cùng độ ẩm cao chính là "kẻ thù" số một của bộ lông dày dặn ở chó mèo. Lông quá dày không chỉ làm các bé khó giải nhiệt cơ thể dẫn đến sốc nhiệt (heat stroke) mà còn tạo môi trường lý tưởng cho ve, rận, bọ chét và nấm da phát triển mạnh mẽ.</p>
      <p>Để giúp bé yêu vượt qua mùa hè mát mẻ, sạch sẽ và thời thượng nhất, MyPetClinic mang đến chương trình siêu ưu đãi lớn nhất năm dành riêng cho dịch vụ làm đẹp.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Nội dung ưu đãi cực khủng</h4>
      <ul>
        <li><strong>Giảm ngay 20%</strong> trên tổng hóa đơn dịch vụ cắt lông tạo kiểu nghệ thuật.</li>
        <li><strong>Tặng gói tắm sấy khử mùi chuyên sâu</strong> bằng dầu gội dược liệu nhập khẩu từ Ý.</li>
        <li><strong>Miễn phí 100%</strong> các dịch vụ đi kèm: Vệ sinh tai, cắt mài móng chân, vắt tuyến hôi hậu môn.</li>
      </ul>
      <h4 class="fw-bold text-dark mt-4 mb-3">Đội ngũ Stylist chuyên nghiệp</h4>
      <p>Tại MyPetClinic, bé cưng của bạn sẽ được chăm sóc bởi đội ngũ Stylist đã có chứng chỉ quốc tế, am hiểu cấu trúc xương khớp và tâm lý thú cưng, giúp giảm thiểu tối đa sự sợ hãi của các bé khi tắm cắt. Phòng Grooming được thiết kế kính suốt hiện đại, giúp chủ nuôi dễ dàng theo dõi toàn bộ quá trình làm đẹp của bé cưng.</p>
      <div class="bg-light p-4 rounded-3 border-start border-warning border-5 mt-4">
        <p class="mb-0 text-dark"><i>Lưu ý: Chương trình chỉ áp dụng cho khách hàng đặt lịch hẹn trước tối thiểu 1 tiếng trên website hoặc ứng dụng. Đặt lịch ngay hôm nay để nhận được khung giờ vàng ưng ý nhất!</i></p>
      </div>
    `
  },
  {
    id: 7,
    category: 'Nội bộ',
    badgeClass: 'bg-info text-white',
    tags: ['Chứng nhận ISO', 'Chất lượng', 'Quốc tế'],
    title: 'MyPetClinic Đón Nhận Chứng Chỉ Y Khoa Quốc Tế ISO 9001:2015',
    shortTitle: 'MyPetClinic nhận chứng chỉ quốc tế ISO',
    date: '18/05/2026',
    author: 'Ban Quản Lý Chất Lượng',
    image: 'https://images.unsplash.com/photo-1583337130417-3346a1be7dee?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Hệ thống thú y MyPetClinic tự hào thông báo đã chính thức đón nhận Chứng chỉ Hệ thống Quản lý Chất lượng Tiêu chuẩn Quốc tế <strong>ISO 9001:2015</strong> cho toàn bộ quy trình chăm sóc sức khỏe thú y, phẫu thuật ngoại khoa và quy trình khử trùng chuẩn y khoa quốc tế.</p>
      <p>Đây là minh chứng vững chắc cho cam kết không ngừng nâng cao chất lượng điều trị và mang lại sự an tâm tuyệt đối cho khách hàng khi gửi gắm những người bạn bốn chân tại đây.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Chứng nhận ISO 9001 mang ý nghĩa gì với thú cưng của bạn?</h4>
      <p>Việc đạt tiêu chuẩn ISO đòi hỏi toàn bộ hệ thống phải tuân thủ nghiêm ngặt các tiêu chuẩn y tế khắt khe nhất:</p>
      <ul>
        <li><strong>Quy trình phẫu thuật vô trùng tuyệt đối:</strong> Giảm thiểu tối đa nguy cơ nhiễm trùng chéo sau phẫu thuật.</li>
        <li><strong>Hồ sơ bệnh án điện tử đồng bộ hóa:</strong> Lịch sử điều trị, xét nghiệm và dị ứng thuốc của thú cưng được lưu trữ an toàn, truy xuất nhanh chóng giúp chẩn đoán khẩn cấp cực nhanh.</li>
        <li><strong>Chất lượng dược phẩm chuẩn y khoa:</strong> 100% thuốc điều trị và vaccine được bảo quản trong hệ thống tủ lạnh chuyên dụng đạt chuẩn GSP quốc tế.</li>
      </ul>
      <div class="bg-light p-4 rounded-3 border-start border-primary border-5 mt-4">
        <p class="mb-0 text-primary fw-bold">MyPetClinic cam kết: Không dừng lại ở chứng chỉ này, chúng tôi sẽ liên tục cải tiến dịch vụ, nâng cấp trang thiết bị máy móc hiện đại và cử đội ngũ bác sĩ đi đào tạo chuyên sâu tại nước ngoài hàng năm nhằm duy trì tiêu chuẩn y khoa tốt nhất.</p>
      </div>
    `
  },
  {
    id: 8,
    category: 'Cộng đồng',
    badgeClass: 'bg-success text-white',
    tags: ['Nhận nuôi', 'Yêu thương', 'Cứu hộ chó mèo'],
    title: 'Ngày Hội Nhận Nuôi Thú Cưng Mồ Côi - Tìm Mái Ấm Yêu Thương',
    shortTitle: 'Ngày hội nhận nuôi thú cưng mồ côi',
    date: '10/05/2026',
    author: 'Ban Truyền Thông MyPetClinic',
    image: 'https://images.unsplash.com/photo-1518717758536-85ae29035b6d?q=80&w=1200&auto=format&fit=crop',
    content: `
      <p>Mỗi chú chó mèo mồ côi, bị bỏ rơi hay đi lạc đều xứng đáng có cơ hội thứ hai để cảm nhận được tình thương ấm áp của một gia dịch. Thấu hiểu điều đó, MyPetClinic phối hợp cùng các Trạm Cứu Hộ Động Vật tổ chức ngày hội kết nối ý nghĩa mang tên: <strong>"Tìm Mái Ấm Yêu Thương"</strong>.</p>
      <p>Sự kiện nhằm mục đích nâng cao nhận thức cộng đồng về việc bảo vệ động vật và khuyến khích việc nhận nuôi thay vì mua bán thương mại.</p>
      <h4 class="fw-bold text-dark mt-4 mb-3">Sự hỗ trợ đặc biệt từ MyPetClinic khi bạn nhận nuôi</h4>
      <ul>
        <li><strong>Gói kiểm tra sức khỏe tổng quát</strong> và soi da tầm soát nấm, ghẻ cho bé tại chỗ.</li>
        <li><strong>Tiêm phòng miễn phí</strong> mũi vaccine dại và tẩy giun sán định kỳ lần đầu.</li>
        <li><strong>Tặng 1 túi thức ăn dinh dưỡng</strong> hạt khô và bộ vòng cổ dây dắt xinh xắn làm quà chào mừng.</li>
      </ul>
      <h4 class="fw-bold text-dark mt-4 mb-3">Làm sao để đăng ký nhận nuôi?</h4>
      <p>Sự kiện mở cửa tự do. Tuy nhiên, để nhận nuôi một bé cưng, người nhận nuôi cần mang theo CCCD và chứng minh được điều kiện tài chính cơ bản cũng như không gian sống phù hợp. Các bạn tình nguyện viên sẽ phỏng vấn nhanh để đảm bảo bé cưng được gửi gắm vào một gia đình thực sự trách nhiệm.</p>
      <div class="bg-light p-4 rounded-3 border-start border-success border-5 mt-4">
        <p class="mb-0 text-success fw-bold">Hãy đến tham gia ngày hội cùng chúng tôi tại cơ sở 1 Quận 1 vào ngày Chủ nhật (14/06/2026) để cùng mang lại cơ hội đổi đời cho hàng chục mảnh đời bé nhỏ đáng yêu!</p>
      </div>
    `
  }
];

const isHardcoded = computed(() => articleId.value >= 1 && articleId.value <= 8);

const article = computed(() => {
  if (isHardcoded.value) {
    return articlesList.find(a => a.id === articleId.value) || articlesList[0];
  }
  return dbArticle.value;
});

const loadDbArticle = async () => {
  if (isHardcoded.value) {
    dbArticle.value = null;
    return;
  }
  loading.value = true;
  try {
    const res = await api.get('/posts');
    const posts = res.data;
    const found = posts.find((p: any) => p.id === articleId.value);
    if (found) {
      dbArticle.value = {
        id: found.id,
        category: 'Tin tức',
        badgeClass: 'bg-warning text-dark',
        tags: found.tags || ['Bản tin', 'Tin tức'],
        title: found.title,
        date: new Date(found.createdAt).toLocaleDateString('vi-VN'),
        author: found.authorName || 'Bác sĩ MyPetClinic',
        image: found.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=1200&auto=format&fit=crop',
        content: found.content
      };
    } else {
      dbArticle.value = articlesList[0];
    }
  } catch (err) {
    console.error('Lỗi tải bài viết từ DB:', err);
    dbArticle.value = articlesList[0];
  } finally {
    loading.value = false;
  }
};

onMounted(loadDbArticle);
watch(articleId, loadDbArticle);

const suggestions = computed(() => {
  const nextId1 = (articleId.value % 8) + 1;
  const nextId2 = ((articleId.value + 1) % 8) + 1;
  
  return [
    articlesList.find(a => a.id === nextId1) || articlesList[0],
    articlesList.find(a => a.id === nextId2) || articlesList[1]
  ];
});
</script>

<style scoped>
.page-wrapper {
  background-color: #ffffff;
  color: var(--text-dark);
  font-family: 'Plus Jakarta Sans', sans-serif;
}

.btn-back {
  background: none;
  border: none;
  font-family: 'Be Vietnam Pro', sans-serif;
  font-weight: 600;
  display: flex;
  align-items: center;
  padding: 0;
  cursor: pointer;
  transition: color var(--transition-speed);
}

.icon-back {
  width: 18px;
  height: 18px;
}

.article-title {
  font-family: 'Be Vietnam Pro', sans-serif;
  font-size: 2.5rem;
  line-height: 1.25;
  color: var(--text-dark);
}

.meta-icon {
  width: 14px;
  height: 14px;
  display: inline-block;
  vertical-align: middle;
  margin-right: 2px;
}

.tag-icon {
  width: 12px;
  height: 12px;
  display: inline-block;
  vertical-align: middle;
}

.article-image-wrapper {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  border-radius: 1.25rem;
  overflow: hidden;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.1);
  background-color: #f8fafc;
}

.article-hero-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center;
  transition: transform 0.7s ease;
}

.article-image-wrapper:hover .article-hero-img {
  transform: scale(1.03);
}

.article-content {
  color: var(--text-muted);
}

.article-content :deep(p) {
  margin-bottom: 1.5rem;
  font-size: 1.1rem;
}

.article-content :deep(h4) {
  font-family: 'Be Vietnam Pro', sans-serif;
  font-weight: 700;
  color: var(--text-dark);
}

.article-content :deep(ul) {
  margin-bottom: 1.5rem;
  padding-left: 1.5rem;
}

.article-content :deep(li) {
  margin-bottom: 0.5rem;
  font-size: 1.1rem;
}

.suggestion-card {
  transition: all 0.3s ease;
}

.suggestion-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-md) !important;
}

.suggestion-title {
  font-family: 'Be Vietnam Pro', sans-serif;
  font-size: 0.95rem;
  transition: color 0.3s ease;
}

.suggest-img {
  border-radius: var(--radius-sm) 0 0 var(--radius-sm);
  width: 100%;
}
</style>
