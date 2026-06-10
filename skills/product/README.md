# 🐾 MyPetClinic - Product Skills Framework

Chào mừng bạn đến với **Khung Năng Lực Product (Product Management, Business Analysis & Quality Assurance)** cho dự án **MyPetClinic**. Tài liệu này được thiết kế nhằm chuẩn hóa các kỹ năng cốt lõi cần có cho vai trò Product Owner (PO), Business Analyst (BA), và QA Tester trong bối cảnh phát triển một hệ thống quản lý phòng khám thú y thông minh sử dụng **.NET 8 Clean Architecture**, **Vue 3 (Vite + TypeScript)**, **PostgreSQL** và **Gemini AI API**.

---

## 📋 Mục Lục Chi Tiết

1. [Tổng Quan Vai Trò Product](#1-tổng-quan-vai-trò-product)
2. [Cây Kỹ Năng (Skill Tree)](#2-cây-kỹ-năng-skill-tree)
3. [Chi Tiết Từng Cấp Độ (Detailed Framework Files)](#3-chi-tiết-từng-cấp-độ-detailed-framework-files)
   - 🟢 [Level 1: Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/product/foundation.md)
   - 🟡 [Level 2: Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/product/core.md)
   - 🟠 [Level 3: Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/product/advanced.md)
   - 🔴 [Level 4: Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/product/expert.md)
4. [Lộ Trình & Đánh Giá](file:///e:/DATN/MyPetClinic/skills/product/roadmap.md)

---

## 1. Tổng Quan Vai Trò Product

```
┌─────────────────────────────────────────────────────────────────┐
│              PRODUCT ROLE IN MYPETCLINIC                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │                  PRODUCT OWNER (PO)                        │    │
│  │  • Định hình tầm nhìn sản phẩm (Vision & Value Prop)      │    │
│  │  • Quyết định mức độ ưu tiên backlog (MoSCoW & RICE)      │    │
│  │  • Định nghĩa & duyệt tiêu chí hoàn thành (DoR & DoD)      │    │
│  └──────────────────────────────────────────────────────────┘    │
│                            │                                       │
│  ┌─────────────────────────┴────────────────────────────────┐    │
│  │              BUSINESS ANALYST (BA)                         │    │
│  │  • Phân tích nghiệp vụ phòng khám (Domain Operations)      │    │
│  │  • Vẽ sơ đồ quy trình & phân làn chức năng (BPMN)           │    │
│  │  • Viết User Stories & Acceptance Criteria (BDD/Gherkin)   │    │
│  └─────────────────────────┬────────────────────────────────┘    │
│                            │                                       │
│  ┌─────────────────────────┴────────────────────────────────┐    │
│  │              QA TESTER / QUALITY ASSURANCE                 │    │
│  │  • Thiết kế kịch bản & Test Cases (Equivalence, Boundary) │    │
│  │  • Bug Reporting, kiểm thử API & truy vấn DB (SQL)        │    │
│  │  • Viết kịch bản kiểm thử tự động E2E (Playwright)         │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                     │
│  👤 NGƯỜI ĐẢM NHẬN CHÍNH: Lâm                                     │
│  👥 HỖ TRỢ & REVIEW: Nam (Technical review), Phương (UX review)    │
│                                                                     │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Cây Kỹ Năng (Skill Tree)

```
Product Manager / BA / QA (MyPetClinic)
│
├── 🟢 LEVEL 1: FOUNDATION (Nền Tảng - Tuần 1-2)
│   ├── PRD-01: Product Mindset & User Empathy (Tầm nhìn & Persona)
│   ├── PRD-02: Agile & Scrum Mastery (Scrum Events & DoR/DoD)
│   ├── PRD-03: User Story Writing (Định dạng chuẩn Agile)
│   ├── PRD-04: Acceptance Criteria (BDD / Gherkin Syntax)
│   └── PRD-05: Basic Manual Testing (Functional & UI Checklists)
│
├── 🟡 LEVEL 2: CORE (Cốt Lõi - Tuần 3-6)
│   ├── PRD-06: Business Process Modeling (BPMN / Swimlanes)
│   ├── PRD-07: Domain Knowledge (Nghiệp vụ thú y & Thuật ngữ)
│   ├── PRD-08: Test Case Design Techniques (Phân tích giá trị biên, Trạng thái)
│   ├── PRD-09: Bug Reporting & Defect Management (Jira/GitHub Issues)
│   ├── PRD-10: Backlog Refinement & Prioritization (MoSCoW & RICE)
│   └── PRD-11: Sprint Planning & Sizing (Story Points & Velocity)
│
├── 🟠 LEVEL 3: ADVANCED (Nâng Cao - Tuần 7-10)
│   ├── PRD-12: UAT Planning & Execution (Nghiệm thu thực tế)
│   ├── PRD-13: API Testing (Postman / Swagger - .NET API Web endpoints)
│   ├── PRD-14: SQL for Verification (Truy vấn PostgreSQL database)
│   ├── PRD-15: Product Metrics & KPI Tracking (Doanh thu, Tỷ lệ đặt lịch)
│   ├── PRD-16: Risk Management (Kiểm soát rủi ro tiến độ & Nghiệp vụ)
│   └── PRD-17: Stakeholder Communication (Phối hợp DEV-UX & Chủ phòng khám)
│
└── 🔴 LEVEL 4: EXPERT (Chuyên Gia - Tuần 11-12)
    ├── PRD-18: Automation Testing Basics (Playwright E2E UI Flow)
    ├── PRD-19: Release Management & Go-Live (Quản lý phiên bản)
    ├── PRD-20: User Documentation (Sách hướng dẫn sử dụng cho Lễ tân/Bác sĩ)
    └── PRD-21: Product Roadmap & Strategy (Tầm nhìn phát triển AI Chatbot)
```

---

## 3. Chi Tiết Từng Cấp Độ

*   🟢 **[Level 1: Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/product/foundation.md)**: Định hình Product Mindset, viết User Stories và Acceptance Criteria chuẩn Gherkin, thực hành Manual Test cơ bản.
*   🟡 **[Level 2: Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/product/core.md)**: Thiết kế quy trình BPMN, đào sâu kiến thức nghiệp vụ thú y, áp dụng các kỹ thuật thiết kế Test Case (Phân tích giá trị biên, Biểu đồ trạng thái) và ưu tiên backlog (RICE, MoSCoW).
*   🟠 **[Level 3: Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/product/advanced.md)**: Test API bằng Swagger/Postman, kiểm tra dữ liệu bằng SQL trong PostgreSQL, lập kế hoạch UAT và quản trị rủi ro dự án.
*   🔴 **[Level 4: Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/product/expert.md)**: Tự động hóa kiểm thử bằng Playwright, điều phối Release, viết hướng dẫn sử dụng, xây dựng chiến lược tích hợp AI Chatbot.
