# 🐾 MyPetClinic - Backend Skills Framework

Chào mừng bạn đến với **Khung Năng Lực Phát Triển Backend** cho dự án **MyPetClinic**. Tài liệu này được thiết kế chi tiết theo từng cấp độ kỹ năng, lộ trình học tập và tiêu chí đánh giá cụ thể giúp các thành viên phát triển toàn diện.

---

## 📋 Mục Lục Chi Tiết

1. [Tổng Quan Kiến Trúc Backend](#1-tổng-quan-kiến-trúc-backend)
2. [Cây Kỹ Năng (Skill Tree)](#2-cây-kỹ-năng-skill-tree)
3. [Chi Tiết Từng Cấp Độ (Detailed Framework Files)](#3-chi-tiết-từng-cấp-độ-detailed-framework-files)
   - 🟢 [Level 1: Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/backend/foundation.md)
   - 🟡 [Level 2: Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/backend/core.md)
   - 🟠 [Level 3: Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/backend/advanced.md)
   - 🔴 [Level 4: Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/backend/expert.md)
4. [Lộ Trình & Đánh Giá](#4-lộ-trình--đánh-giá)
   - 📅 [Lộ Trình Học Tập & Tiêu Chí Đánh Giá](file:///e:/DATN/MyPetClinic/skills/backend/roadmap.md)

---

## 1. Tổng Quan Kiến Trúc Backend

```
┌─────────────────────────────────────────────────────────┐
│                     MyPetClinic Backend                    │
├─────────────────────────────────────────────────────────┤
│  WebApi Layer (Controllers, Middlewares, Filters)        │
├─────────────────────────────────────────────────────────┤
│  Application Layer (Use Cases, DTOs, Interfaces)         │
├─────────────────────────────────────────────────────────┤
│  Domain Layer (Entities, Value Objects, Domain Events)   │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Layer (EF Core, Email, AI, File Storage) │
└─────────────────────────────────────────────────────────┘
```

---

## 2. Cây Kỹ Năng (Skill Tree)

```
Backend Developer (MyPetClinic)
│
├── 🟢 Foundation (Nền Tảng) - Sprint 1
│   ├── C# Fundamentals
│   ├── .NET 8 SDK & Runtime
│   ├── OOP & SOLID Principles
│   ├── LINQ & Lambda Expressions
│   └── Async/Await & Task Parallel Library
│
├── 🟡 Core (Cốt Lõi) - Sprint 1-2
│   ├── ASP.NET Core Web API
│   │   ├── Controllers & Action Methods
│   │   ├── Routing & Model Binding
│   │   ├── Middleware Pipeline
│   │   └── Dependency Injection
│   ├── Entity Framework Core
│   │   ├── Code First Migrations
│   │   ├── Fluent API Configuration
│   │   ├── Relationships (1-1, 1-N, N-N)
│   │   └── Query Optimization
│   └── RESTful API Design
│       ├── HTTP Methods & Status Codes
│       ├── Versioning & Pagination
│       └── OpenAPI/Swagger Documentation
│
├── 🟠 Advanced (Nâng Cao) - Sprint 3-5
│   ├── Clean Architecture
│   │   ├── Domain Layer Design
│   │   ├── Repository Pattern
│   │   ├── Unit of Work
│   │   └── CQRS (Command Query Responsibility Segregation)
│   ├── Authentication & Authorization
│   │   ├── Cookie Auth & Google OAuth
│   │   ├── BCrypt Password Hashing
│   │   ├── Role-Based Authorization
│   │   └── Policy-Based Authorization
│   ├── Validation & Error Handling
│   │   ├── FluentValidation
│   │   ├── Global Exception Handling
│   │   └── ProblemDetails (RFC 7807)
│   └── Database Advanced
│       ├── Transactions & Concurrency
│       ├── Stored Procedures
│       └── Indexing Strategy
│
└── 🔴 Expert (Chuyên Gia) - Sprint 5-6
    ├── Background Services
    │   ├── Hangfire / Quartz.NET
    │   └── Scheduled Jobs (Nhắc lịch, Cảnh báo)
    ├── External Services Integration
    │   ├── Email (SMTP/SendGrid)
    │   ├── Cloud Storage (Cloudinary/Azure Blob)
    │   └── AI (Google Gemini API)
    ├── Testing
    │   ├── Unit Testing (xUnit + Moq)
    │   ├── Integration Testing
    │   └── Test Coverage > 70%
    └── Performance & Security
        ├── Response Caching
        ├── Rate Limiting
        └── CORS & HTTPS Enforcement
```

---

## 3. Chi Tiết Từng Cấp Độ (Detailed Framework Files)

Vui lòng nhấp vào các liên kết dưới đây để xem chi tiết lý thuyết, ví dụ code thực tế trong dự án và bài tập áp dụng cho từng cấp độ kỹ năng:

*   🟢 **[Level 1: Foundation (Nền Tảng)](file:///e:/DATN/MyPetClinic/skills/backend/foundation.md)**: C# cơ bản, OOP/SOLID, và xử lý bất tuần tự (Async/Await).
*   🟡 **[Level 2: Core (Cốt Lõi)](file:///e:/DATN/MyPetClinic/skills/backend/core.md)**: Xây dựng API và tương tác cơ sở dữ liệu với Entity Framework Core.
*   🟠 **[Level 3: Advanced (Nâng Cao)](file:///e:/DATN/MyPetClinic/skills/backend/advanced.md)**: Triển khai Clean Architecture, Repository Pattern, Unit of Work, và bảo mật với Cookie & Google OAuth.
*   🔴 **[Level 4: Expert (Chuyên Gia)](file:///e:/DATN/MyPetClinic/skills/backend/expert.md)**: Tác vụ chạy nền (Hangfire) và Unit Testing nâng cao với xUnit/Moq.

---

## 4. Lộ Trình & Đánh Giá

*   📅 **[Lộ Trình Học Tập & Tiêu Chí Đánh Giá](file:///e:/DATN/MyPetClinic/skills/backend/roadmap.md)**: Kế hoạch phân bổ thời gian học tập, các bài tập lớn thực tế và bảng tiêu chí nghiệm thu hoàn thành của các thành viên.
