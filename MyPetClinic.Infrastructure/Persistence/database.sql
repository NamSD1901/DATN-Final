-- ============================================================
-- CƠ SỞ DỮ LIỆU HỆ THỐNG QUẢN LÝ PHÒNG KHÁM THÚ CƯNG
-- Dành cho Supabase (PostgreSQL)
-- ============================================================

-- Xóa bảng nếu đã tồn tại (theo thứ tự phụ thuộc FK ngược lại)
DROP TABLE IF EXISTS posts CASCADE;
DROP TABLE IF EXISTS reviews CASCADE;
DROP TABLE IF EXISTS invoice_items CASCADE;
DROP TABLE IF EXISTS invoices CASCADE;
DROP TABLE IF EXISTS vaccination_records CASCADE;
DROP TABLE IF EXISTS vaccines CASCADE;
DROP TABLE IF EXISTS prescription_items CASCADE;
DROP TABLE IF EXISTS prescriptions CASCADE;
DROP TABLE IF EXISTS medicines CASCADE;
DROP TABLE IF EXISTS medical_records CASCADE;
DROP TABLE IF EXISTS appointments CASCADE;
DROP TABLE IF EXISTS doctor_schedules CASCADE;
DROP TABLE IF EXISTS services CASCADE;
DROP TABLE IF EXISTS service_categories CASCADE;
DROP TABLE IF EXISTS pets CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS roles CASCADE;


-- ============================================================
-- 1. roles
-- ============================================================
CREATE TABLE roles (
    id   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) NOT NULL
);

COMMENT ON TABLE  roles      IS 'Danh sách vai trò trong hệ thống';
COMMENT ON COLUMN roles.id   IS 'Mã vai trò';
COMMENT ON COLUMN roles.name IS 'Tên vai trò';

-- Dữ liệu mặc định
INSERT INTO roles (name) VALUES
    ('admin'),
    ('receptionist'),
    ('veterinarian'),
    ('customer');


-- ============================================================
-- 2. users
-- ============================================================
CREATE TABLE users (
    id            UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    role_id       BIGINT      NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    full_name     VARCHAR(255),
    email         VARCHAR(255) UNIQUE,
    phone         VARCHAR(20),
    password_hash TEXT,
    avatar        TEXT,
    gender        SMALLINT,                  -- 0: không rõ, 1: nam, 2: nữ
    date_of_birth DATE,
    address       TEXT,
    is_active     BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  users               IS 'Thông tin tài khoản người dùng';
COMMENT ON COLUMN users.id            IS 'Mã người dùng';
COMMENT ON COLUMN users.role_id       IS 'Vai trò (FK -> roles.id)';
COMMENT ON COLUMN users.gender        IS '0: không rõ, 1: nam, 2: nữ';
COMMENT ON COLUMN users.is_active     IS 'Trạng thái hoạt động';
COMMENT ON COLUMN users.created_at    IS 'Ngày tạo';

CREATE INDEX idx_users_role_id ON users(role_id);
CREATE INDEX idx_users_email   ON users(email);


-- ============================================================
-- 3. pets
-- ============================================================
CREATE TABLE pets (
    id             BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    owner_id       UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name           VARCHAR(255),
    species        VARCHAR(100),
    breed          VARCHAR(100),
    gender         SMALLINT,
    birth_date     DATE,
    weight         NUMERIC(5,2),
    color          VARCHAR(100),
    blood_type     VARCHAR(20),
    sterilized     BOOLEAN     DEFAULT FALSE,
    microchip_code VARCHAR(100),
    allergy_note   TEXT,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  pets               IS 'Hồ sơ thú cưng';
COMMENT ON COLUMN pets.id            IS 'Mã thú cưng';
COMMENT ON COLUMN pets.owner_id      IS 'Chủ sở hữu (FK -> users.id)';
COMMENT ON COLUMN pets.sterilized    IS 'Đã triệt sản';
COMMENT ON COLUMN pets.microchip_code IS 'Mã microchip';
COMMENT ON COLUMN pets.allergy_note  IS 'Ghi chú dị ứng';

CREATE INDEX idx_pets_owner_id ON pets(owner_id);


-- ============================================================
-- 4. service_categories
-- ============================================================
CREATE TABLE service_categories (
    id   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

COMMENT ON TABLE  service_categories      IS 'Danh mục dịch vụ';
COMMENT ON COLUMN service_categories.id   IS 'Mã danh mục';
COMMENT ON COLUMN service_categories.name IS 'Tên danh mục';


-- ============================================================
-- 5. services
-- ============================================================
CREATE TABLE services (
    id                BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    category_id       BIGINT      NOT NULL REFERENCES service_categories(id) ON DELETE RESTRICT,
    name              VARCHAR(255) NOT NULL,
    price             NUMERIC(12,2),
    duration_minutes  INT,
    description       TEXT,
    is_active         BOOLEAN     NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE  services                  IS 'Danh sách dịch vụ phòng khám';
COMMENT ON COLUMN services.id               IS 'Mã dịch vụ';
COMMENT ON COLUMN services.category_id      IS 'Danh mục (FK -> service_categories.id)';
COMMENT ON COLUMN services.duration_minutes IS 'Thời lượng (phút)';
COMMENT ON COLUMN services.is_active        IS 'Trạng thái hoạt động';

CREATE INDEX idx_services_category_id ON services(category_id);


-- ============================================================
-- 6. doctor_schedules
-- ============================================================
CREATE TABLE doctor_schedules (
    id               BIGINT  GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    doctor_id        UUID    NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    work_date        DATE    NOT NULL,
    start_time       TIME    NOT NULL,
    end_time         TIME    NOT NULL,
    max_appointments INT     DEFAULT 10,
    is_available     BOOLEAN NOT NULL DEFAULT TRUE
);

COMMENT ON TABLE  doctor_schedules                  IS 'Lịch làm việc của bác sĩ';
COMMENT ON COLUMN doctor_schedules.id               IS 'Mã lịch';
COMMENT ON COLUMN doctor_schedules.doctor_id        IS 'Bác sĩ (FK -> users.id)';
COMMENT ON COLUMN doctor_schedules.max_appointments IS 'Số ca tối đa';
COMMENT ON COLUMN doctor_schedules.is_available     IS 'Trạng thái';

CREATE INDEX idx_doctor_schedules_doctor_id  ON doctor_schedules(doctor_id);
CREATE INDEX idx_doctor_schedules_work_date  ON doctor_schedules(work_date);


-- ============================================================
-- 7. appointments
-- ============================================================
CREATE TABLE appointments (
    id               BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    pet_id           BIGINT      NOT NULL REFERENCES pets(id) ON DELETE RESTRICT,
    customer_id      UUID        NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    doctor_id        UUID        NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    service_id       BIGINT      NOT NULL REFERENCES services(id) ON DELETE RESTRICT,
    appointment_date DATE        NOT NULL,
    start_time       TIME        NOT NULL,
    end_time         TIME,
    status           VARCHAR(50) NOT NULL DEFAULT 'pending',
                                         -- pending | confirmed | in_progress | done | cancelled
    symptom          TEXT,
    note             TEXT,
    created_by       UUID        REFERENCES users(id) ON DELETE SET NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  appointments                  IS 'Lịch hẹn khám';
COMMENT ON COLUMN appointments.id               IS 'Mã lịch hẹn';
COMMENT ON COLUMN appointments.pet_id           IS 'Thú cưng (FK -> pets.id)';
COMMENT ON COLUMN appointments.customer_id      IS 'Khách hàng (FK -> users.id)';
COMMENT ON COLUMN appointments.doctor_id        IS 'Bác sĩ (FK -> users.id)';
COMMENT ON COLUMN appointments.service_id       IS 'Dịch vụ (FK -> services.id)';
COMMENT ON COLUMN appointments.status           IS 'pending | confirmed | in_progress | done | cancelled';
COMMENT ON COLUMN appointments.symptom          IS 'Triệu chứng';
COMMENT ON COLUMN appointments.created_by       IS 'Người tạo lịch (FK -> users.id)';

CREATE INDEX idx_appointments_pet_id      ON appointments(pet_id);
CREATE INDEX idx_appointments_customer_id ON appointments(customer_id);
CREATE INDEX idx_appointments_doctor_id   ON appointments(doctor_id);
CREATE INDEX idx_appointments_date        ON appointments(appointment_date);


-- ============================================================
-- 8. medical_records
-- ============================================================
CREATE TABLE medical_records (
    id             BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointment_id BIGINT      NOT NULL UNIQUE REFERENCES appointments(id) ON DELETE RESTRICT,
    doctor_id      UUID        NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    weight         NUMERIC(5,2),
    temperature    NUMERIC(4,1),
    heart_rate     INT,
    symptoms       TEXT,
    diagnosis      TEXT,
    treatment_plan TEXT,
    note           TEXT,
    follow_up_date DATE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  medical_records                  IS 'Bệnh án';
COMMENT ON COLUMN medical_records.id               IS 'Mã bệnh án';
COMMENT ON COLUMN medical_records.appointment_id   IS 'Lịch hẹn (FK -> appointments.id)';
COMMENT ON COLUMN medical_records.doctor_id        IS 'Bác sĩ (FK -> users.id)';
COMMENT ON COLUMN medical_records.temperature      IS 'Nhiệt độ (°C)';
COMMENT ON COLUMN medical_records.heart_rate       IS 'Nhịp tim';
COMMENT ON COLUMN medical_records.treatment_plan   IS 'Hướng điều trị';
COMMENT ON COLUMN medical_records.follow_up_date   IS 'Ngày tái khám';

CREATE INDEX idx_medical_records_doctor_id ON medical_records(doctor_id);


-- ============================================================
-- 9. medicines
-- ============================================================
CREATE TABLE medicines (
    id             BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name           VARCHAR(255) NOT NULL,
    unit           VARCHAR(50),
    stock_quantity INT         NOT NULL DEFAULT 0,
    import_price   NUMERIC(12,2),
    sell_price     NUMERIC(12,2),
    expiry_date    DATE,
    description    TEXT
);

COMMENT ON TABLE  medicines               IS 'Danh sách thuốc';
COMMENT ON COLUMN medicines.id            IS 'Mã thuốc';
COMMENT ON COLUMN medicines.unit          IS 'Đơn vị (viên, chai, lọ,...)';
COMMENT ON COLUMN medicines.stock_quantity IS 'Tồn kho';
COMMENT ON COLUMN medicines.import_price  IS 'Giá nhập';
COMMENT ON COLUMN medicines.sell_price    IS 'Giá bán';
COMMENT ON COLUMN medicines.expiry_date   IS 'Hạn sử dụng';


-- ============================================================
-- 10. prescriptions
-- ============================================================
CREATE TABLE prescriptions (
    id                BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    medical_record_id BIGINT      NOT NULL REFERENCES medical_records(id) ON DELETE CASCADE,
    doctor_id         UUID        NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    note              TEXT,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  prescriptions                   IS 'Toa thuốc';
COMMENT ON COLUMN prescriptions.id                IS 'Mã toa thuốc';
COMMENT ON COLUMN prescriptions.medical_record_id IS 'Bệnh án (FK -> medical_records.id)';
COMMENT ON COLUMN prescriptions.doctor_id         IS 'Bác sĩ (FK -> users.id)';

CREATE INDEX idx_prescriptions_medical_record_id ON prescriptions(medical_record_id);
CREATE INDEX idx_prescriptions_doctor_id         ON prescriptions(doctor_id);


-- ============================================================
-- 11. prescription_items
-- ============================================================
CREATE TABLE prescription_items (
    id              BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    prescription_id BIGINT NOT NULL REFERENCES prescriptions(id) ON DELETE CASCADE,
    medicine_id     BIGINT NOT NULL REFERENCES medicines(id) ON DELETE RESTRICT,
    dosage          VARCHAR(100),
    frequency       VARCHAR(100),
    duration_days   INT,
    quantity        INT,
    instruction     TEXT
);

COMMENT ON TABLE  prescription_items                  IS 'Chi tiết thuốc trong toa';
COMMENT ON COLUMN prescription_items.id               IS 'Mã chi tiết';
COMMENT ON COLUMN prescription_items.prescription_id  IS 'Toa thuốc (FK -> prescriptions.id)';
COMMENT ON COLUMN prescription_items.medicine_id      IS 'Thuốc (FK -> medicines.id)';
COMMENT ON COLUMN prescription_items.dosage           IS 'Liều dùng';
COMMENT ON COLUMN prescription_items.frequency        IS 'Tần suất';
COMMENT ON COLUMN prescription_items.duration_days    IS 'Số ngày';
COMMENT ON COLUMN prescription_items.instruction      IS 'Hướng dẫn sử dụng';

CREATE INDEX idx_prescription_items_prescription_id ON prescription_items(prescription_id);
CREATE INDEX idx_prescription_items_medicine_id     ON prescription_items(medicine_id);


-- ============================================================
-- 12. vaccines
-- ============================================================
CREATE TABLE vaccines (
    id           BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name         VARCHAR(255) NOT NULL,
    manufacturer VARCHAR(255),
    description  TEXT
);

COMMENT ON TABLE  vaccines              IS 'Danh sách vaccine';
COMMENT ON COLUMN vaccines.id           IS 'Mã vaccine';
COMMENT ON COLUMN vaccines.manufacturer IS 'Nhà sản xuất';


-- ============================================================
-- 13. vaccination_records
-- ============================================================
CREATE TABLE vaccination_records (
    id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    pet_id         BIGINT NOT NULL REFERENCES pets(id) ON DELETE CASCADE,
    vaccine_id     BIGINT NOT NULL REFERENCES vaccines(id) ON DELETE RESTRICT,
    appointment_id BIGINT REFERENCES appointments(id) ON DELETE SET NULL,
    doctor_id      UUID   NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    injection_date DATE   NOT NULL,
    next_due_date  DATE,
    reaction_note  TEXT
);

COMMENT ON TABLE  vaccination_records                  IS 'Lịch sử tiêm chủng';
COMMENT ON COLUMN vaccination_records.id               IS 'Mã tiêm chủng';
COMMENT ON COLUMN vaccination_records.pet_id           IS 'Thú cưng (FK -> pets.id)';
COMMENT ON COLUMN vaccination_records.vaccine_id       IS 'Vaccine (FK -> vaccines.id)';
COMMENT ON COLUMN vaccination_records.appointment_id   IS 'Lịch hẹn (FK -> appointments.id)';
COMMENT ON COLUMN vaccination_records.doctor_id        IS 'Bác sĩ (FK -> users.id)';
COMMENT ON COLUMN vaccination_records.next_due_date    IS 'Ngày nhắc tiêm lại';
COMMENT ON COLUMN vaccination_records.reaction_note    IS 'Phản ứng sau tiêm';

CREATE INDEX idx_vaccination_records_pet_id     ON vaccination_records(pet_id);
CREATE INDEX idx_vaccination_records_vaccine_id ON vaccination_records(vaccine_id);


-- ============================================================
-- 14. invoices
-- ============================================================
CREATE TABLE invoices (
    id             BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointment_id BIGINT      NOT NULL UNIQUE REFERENCES appointments(id) ON DELETE RESTRICT,
    subtotal       NUMERIC(12,2) NOT NULL DEFAULT 0,
    discount_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_amount   NUMERIC(12,2) NOT NULL DEFAULT 0,
    payment_status VARCHAR(50) NOT NULL DEFAULT 'unpaid',
                                        -- unpaid | paid | refunded
    payment_method VARCHAR(50),         -- cash | card | transfer | ...
    paid_at        TIMESTAMPTZ,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  invoices                  IS 'Hóa đơn thanh toán';
COMMENT ON COLUMN invoices.id               IS 'Mã hóa đơn';
COMMENT ON COLUMN invoices.appointment_id   IS 'Lịch hẹn (FK -> appointments.id)';
COMMENT ON COLUMN invoices.subtotal         IS 'Tạm tính';
COMMENT ON COLUMN invoices.discount_amount  IS 'Giảm giá';
COMMENT ON COLUMN invoices.total_amount     IS 'Tổng tiền';
COMMENT ON COLUMN invoices.payment_status   IS 'unpaid | paid | refunded';
COMMENT ON COLUMN invoices.payment_method   IS 'Phương thức thanh toán';
COMMENT ON COLUMN invoices.paid_at          IS 'Ngày thanh toán';

CREATE INDEX idx_invoices_appointment_id ON invoices(appointment_id);


-- ============================================================
-- 15. invoice_items
-- ============================================================
CREATE TABLE invoice_items (
    id          BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    invoice_id  BIGINT      NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    item_type   VARCHAR(50),            -- 'service' | 'medicine' | 'vaccine' | ...
    item_id     BIGINT,
    item_name   VARCHAR(255),
    quantity    INT         NOT NULL DEFAULT 1,
    unit_price  NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_price NUMERIC(12,2) NOT NULL DEFAULT 0
);

COMMENT ON TABLE  invoice_items              IS 'Chi tiết từng dòng trong hóa đơn';
COMMENT ON COLUMN invoice_items.id           IS 'Mã chi tiết';
COMMENT ON COLUMN invoice_items.invoice_id   IS 'Hóa đơn (FK -> invoices.id)';
COMMENT ON COLUMN invoice_items.item_type    IS 'Loại mục: service | medicine | vaccine';
COMMENT ON COLUMN invoice_items.item_id      IS 'ID của mục (tham chiếu mềm)';
COMMENT ON COLUMN invoice_items.item_name    IS 'Tên mục (snapshot tại thời điểm lập hóa đơn)';
COMMENT ON COLUMN invoice_items.unit_price   IS 'Đơn giá';
COMMENT ON COLUMN invoice_items.total_price  IS 'Thành tiền';

CREATE INDEX idx_invoice_items_invoice_id ON invoice_items(invoice_id);


-- ============================================================
-- 16. reviews
-- ============================================================
CREATE TABLE reviews (
    id             BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_id    UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    appointment_id BIGINT      NOT NULL UNIQUE REFERENCES appointments(id) ON DELETE CASCADE,
    rating         SMALLINT    NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment        TEXT,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  reviews                IS 'Đánh giá dịch vụ';
COMMENT ON COLUMN reviews.id             IS 'Mã đánh giá';
COMMENT ON COLUMN reviews.customer_id    IS 'Khách hàng (FK -> users.id)';
COMMENT ON COLUMN reviews.appointment_id IS 'Lịch hẹn (FK -> appointments.id)';
COMMENT ON COLUMN reviews.rating         IS 'Số sao (1-5)';

CREATE INDEX idx_reviews_customer_id    ON reviews(customer_id);
CREATE INDEX idx_reviews_appointment_id ON reviews(appointment_id);


-- ============================================================
-- 17. posts
-- ============================================================
CREATE TABLE posts (
    id         BIGINT      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title      VARCHAR(255) NOT NULL,
    slug       VARCHAR(255) UNIQUE,
    thumbnail  TEXT,
    content    TEXT,
    author_id  UUID        REFERENCES users(id) ON DELETE SET NULL,
    status     VARCHAR(50) NOT NULL DEFAULT 'draft',  -- draft | published | archived
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE  posts           IS 'Bài viết blog/tin tức';
COMMENT ON COLUMN posts.id        IS 'Mã bài viết';
COMMENT ON COLUMN posts.slug      IS 'Slug SEO';
COMMENT ON COLUMN posts.thumbnail IS 'Ảnh thumbnail';
COMMENT ON COLUMN posts.author_id IS 'Tác giả (FK -> users.id)';
COMMENT ON COLUMN posts.status    IS 'draft | published | archived';

CREATE INDEX idx_posts_author_id ON posts(author_id);
CREATE INDEX idx_posts_status    ON posts(status);
CREATE INDEX idx_posts_slug      ON posts(slug);


-- ============================================================
-- ROW LEVEL SECURITY (RLS) – bật cho các bảng nhạy cảm
-- Bỏ comment các dòng bên dưới sau khi thiết lập Auth trong Supabase
-- ============================================================

-- ALTER TABLE users              ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE pets               ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE appointments       ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE medical_records    ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE prescriptions      ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE prescription_items ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE vaccination_records ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE invoices           ENABLE ROW LEVEL SECURITY;
-- ALTER TABLE reviews            ENABLE ROW LEVEL SECURITY;

-- ============================================================
-- KẾT THÚC MIGRATION
-- ============================================================
