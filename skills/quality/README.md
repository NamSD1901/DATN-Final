# 🧪 MyPetClinic - Quality Assurance & Testing Skills Framework

Chào mừng bạn đến với **Khung Năng Lực Quality Assurance & Testing** cho dự án **MyPetClinic**. Tài liệu này được thiết kế dựa trên các công nghệ kiểm thử hiện đại bao gồm **xUnit** (.NET 8), **Vitest/Playwright** (Vue 3/TypeScript), **Postman**, và **PostgreSQL**, phục vụ việc đảm bảo chất lượng toàn diện của dự án.

---

## 📋 Mục Lục Chi Tiết

1. [Tổng Quan Chiến Lược QA](#1-tổng-quan-chiến-lược-qa)
2. [Cây Kỹ Năng (Skill Tree)](#2-cây-kỹ-năng-qa)
3. [Chi Tiết Từng Cấp Độ (Detailed Framework Files)](#3-chi-tiết-từng-cấp-độ-detailed-framework-files)
   - 🟢 [Level 1: QA Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/quality/foundation.md)
   - 🟡 [Level 2: QA Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/quality/core.md)
   - 🟠 [Level 3: QA Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/quality/advanced.md)
   - 🔴 [Level 4: QA Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/quality/expert.md)
4. [Lộ Trình & Đánh Giá](file:///e:/DATN/MyPetClinic/skills/quality/roadmap.md)

---

## 1. Tổng Quan Chiến Lược QA

```
┌─────────────────────────────────────────────────────────────────────┐
│                    QUALITY ASSURANCE STRATEGY                         │
│                       MY PET CLINIC                                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │                    TESTING PYRAMID                             │    │
│  │                                                                │    │
│  │                         ╱ E2E ╲                              │    │
│  │                        ╱  10%   ╲                            │    │
│  │                       ╱──────────╲                           │    │
│  │                      ╱ INTEGRATION ╲                         │    │
│  │                     ╱     30%       ╲                        │    │
│  │                    ╱──────────────────╲                      │    │
│  │                   ╱    UNIT TESTS      ╲                     │    │
│  │                  ╱        60%           ╲                    │    │
│  │                 ╱────────────────────────╲                   │    │
│  │                                                                │    │
│  └──────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │                    QA RESPONSIBILITIES                        │    │
│  ├──────────────────────────────────────────────────────────────┤    │
│  │                                                                │    │
│  │  👤 QA LEAD (Lâm)                                              │    │
│  │  • Test Strategy & Planning                                    │    │
│  │  • Manual Test Case Design & Execution                        │    │
│  │  • Bug Management, Verification & Reporting                    │    │
│  │  • UAT Coordination & Quality Metrics                          │    │
│  │                                                                │    │
│  │  👥 DEV TEAM (Nam, Phương, Hạnh)                               │    │
│  │  • Unit Testing (xUnit cho .NET, Vitest cho Vue 3)             │    │
│  │  • Code Review & Integration Testing                           │    │
│  │  • Performance Tuning & Bug Fixing                             │    │
│  │                                                                │    │
│  └──────────────────────────────────────────────────────────────┘    │
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │                    TESTING PHASES                              │    │
│  │                                                                │    │
│  │  Sprint 1-2: Manual Testing + Basic API Testing               │    │
│  │  Sprint 3-4: Integration Testing + Regression Testing         │    │
│  │  Sprint 5-6: UAT + Performance + Security Testing             │    │
│  │                                                                │    │
│  └──────────────────────────────────────────────────────────────┘    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Cây Kỹ Năng QA (QA Skill Tree)

```
Quality Assurance Engineer (MyPetClinic)
│
├── 🟢 LEVEL 1: QA FOUNDATION (Sprint 1-2)
│   ├── QA-01: Testing Fundamentals & Mindset (Tư duy kiểm thử & Kế hoạch Master)
│   ├── QA-02: Manual Testing Techniques (Khảo sát tính năng, Smoke/Sanity)
│   ├── QA-03: Test Case Design (Black-box: Phân vùng tương đương, Phân tích biên)
│   ├── QA-04: Bug Reporting Best Practices (Vòng đời lỗi & Template chuẩn)
│   └── QA-05: Browser DevTools for Testing (Elements, Console, Network)
│
├── 🟡 LEVEL 2: QA CORE (Sprint 2-4)
│   ├── QA-06: API Testing with Postman (Collections, Environments, Test Scripts)
│   ├── QA-07: Database Testing with SQL (Data Integrity, Verify & Cleanup)
│   ├── QA-08: UI/UX Testing & Responsive Testing (Checklists & Responsive Matrix)
│   ├── QA-09: Test Data Management (Dữ liệu kiểm thử ảo & Môi trường)
│   ├── QA-10: Regression Testing Strategy (Kiểm thử hồi quy sau mỗi Sprint)
│   └── QA-11: Cross-browser Testing (Chrome, Safari Mobile, Firefox, Edge)
│
├── 🟠 LEVEL 3: QA ADVANCED (Sprint 4-6)
│   ├── QA-12: Integration Testing Coordination (Kiểm thử tích hợp các luồng E2E)
│   ├── QA-13: UAT Planning & Facilitation (Nghiệm thu phối hợp với người dùng)
│   ├── QA-14: Performance Testing (Kiểm thử hiệu năng bằng k6/JMeter)
│   ├── QA-15: Security Testing Basics (Bảo mật JWT, OWASP Top 10 cơ bản)
│   ├── QA-16: Accessibility Testing (a11y - Kiểm thử khả năng tiếp cận)
│   └── QA-17: Test Metrics & Reporting (Sprint Quality Report)
│
└── 🔴 LEVEL 4: QA EXPERT (Sprint 6+)
    ├── QA-18: Automation Testing (Playwright UI Automation & E2E)
    ├── QA-19: CI/CD Test Integration (Tích hợp kiểm thử tự động vào Github Actions)
    ├── QA-20: Chaos Engineering Basics (Kiểm thử độ chịu tải & Fault injection)
    └── QA-21: Quality Process Improvement (Cải tiến quy trình kiểm thử)
```

---

## 3. Chi Tiết Từng Cấp Độ

*   🟢 **[Level 1: QA Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/quality/foundation.md)**: Master Test Plan, Phân loại Severity, Kế hoạch quản lý rủi ro (Risk-based Testing) và kỹ thuật thiết kế Test Case.
*   🟡 **[Level 2: QA Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/quality/core.md)**: API testing chi tiết bằng Postman, SQL Queries xác minh cơ sở dữ liệu PostgreSQL, UI/UX Responsive Checklist.
*   🟠 **[Level 3: QA Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/quality/advanced.md)**: Performance testing bằng k6, Bug Lifecycle & Reporting chi tiết, và Sprint Quality Report Metrics.
*   🔴 **[Level 4: QA Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/quality/expert.md)**: Viết mã Playwright tự động hóa, CI/CD pipeline integration, và tối ưu hóa quy trình kiểm thử.
