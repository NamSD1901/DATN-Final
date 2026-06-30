# TÀI LIỆU ĐÀO TẠO NỘI BỘ: QUẢN LÝ DANH MỤC VÀ BÀI VIẾT (ADMIN VIEW)

> [!NOTE]
> Đây là tài liệu Đào tạo số 20, dành riêng cho **Quản trị viên (Admin)**.
> Mô-đun này phụ trách việc tạo lập hệ thống Tin tức / Kiến thức y khoa cho phòng khám. 
> Trọng tâm tài liệu này sẽ đi sâu vào **Nghiệp vụ Quản lý (CRUD)** bao gồm: Cấu trúc Cây danh mục vô hạn cấp (Adjacency List), Logic tự động sinh Slug, và Cơ chế Xóa Mềm (Soft Delete) đối với Bài viết.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Danh mục Bài viết & Quản lý Bài viết.
- **Mục đích:** Thêm/sửa/xóa các thư mục phân loại và xuất bản nội dung bài viết.
- **Điểm nổi bật (Kỹ thuật):** Danh mục áp dụng kiến trúc đệ quy (Parent-Children). Bài viết áp dụng cơ chế Soft-delete bằng máy trạng thái (Status) để tránh đứt gãy khóa ngoại.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - QUẢN LÝ DANH MỤC (POST CATEGORY)

Danh mục bài viết không nằm ngang hàng nhau, mà có thể lồng vào nhau (Ví dụ: `Chăm sóc thú cưng -> Chăm sóc chó -> Dinh dưỡng chó`). Để làm được điều này, Entity sử dụng mẫu thiết kế **Adjacency List**.

**Tệp:** `MyPetClinic.Domain/Entities/PostCategory.cs` & `MyPetClinic.Application/DTOs/PostCategoryDtos.cs`

```csharp
    // --- 1. ENTITY: BẢNG DANH MỤC TRONG DATABASE ---
    public class PostCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; // Đường dẫn chuẩn (Vd: cham-soc-cho)
        
        // CẤU TRÚC ĐỆ QUY (TỰ TRỎ VỀ CHÍNH NÓ)
        public long? ParentId { get; set; } // Nếu = NULL thì đây là Danh mục gốc
        public PostCategory? Parent { get; set; } // Trỏ lên Danh mục cha
        
        public ICollection<PostCategory> Children { get; set; } = new List<PostCategory>(); // Danh sách các danh mục con
    }

    // --- 2. DTO: GÓI DỮ LIỆU ĐỂ ADMIN TẠO MỚI ---
    public class CreatePostCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; } // Không bắt buộc nhập
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        
        public long? ParentId { get; set; } // Truyền ID của danh mục cha (nếu có)
    }
```

**Logic tự động sinh Slug (Controller / Service):**
Khi Admin tạo danh mục mới, họ thường lười gõ trường `Slug`. Hệ thống sẽ bắt lỗi này và tự sinh ra.
Ví dụ: Admin nhập Name = "Dinh Dưỡng Chó", hệ thống tự động đổi `Slug` thành `dinh-duong-cho`.

---

### PHẦN 2.2 - QUẢN LÝ BÀI VIẾT (POST) VÀ AUTO-SLUG

Bài viết lưu trữ nội dung bằng HTML (trường `Content`), đồng thời quản lý theo Máy Trạng Thái (Status: `draft`, `published`, `archived`).

**Tệp:** `MyPetClinic.Domain/Entities/Post.cs` & `MyPetClinic.Application/DTOs/PostDtos.cs`

```csharp
    // --- 1. ENTITY BÀI VIẾT ---
    public class Post
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; 
        public string? Content { get; set; } // Chứa mã HTML từ bộ soạn thảo (Rich Text Editor)
        
        // MÁY TRẠNG THÁI (STATE MACHINE)
        public string Status { get; set; } = "draft"; 
        
        public long? CategoryId { get; set; } // Thuộc danh mục nào?
        public Guid? AuthorId { get; set; }   // Admin/Bác sĩ nào viết?
    }

    // --- 2. DTO TẠO MỚI BÀI VIẾT ---
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "draft"; // Mặc định là Nháp
        
        public long? CategoryId { get; set; }
        public List<string> Tags { get; set; } = new List<string>(); // Danh sách thẻ (Ví dụ: ["Chó Corgi", "Mùa hè"])
    }
```

**Tệp:** `MyPetClinic.Application/Services/PostService.cs` *(Logic Thêm mới)*

```csharp
        public async Task<PostDto> CreatePostAsync(CreatePostDto dto, Guid? authorId)
        {
            // 1. AUTO-SLUG: Nếu Admin không nhập Slug, tự động lấy Title, hạ chữ thường và thay khoảng trắng bằng dấu gạch ngang
            var slug = dto.Slug ?? dto.Title.ToLower().Replace(" ", "-");
            
            // 2. CHỐNG TRÙNG LẶP URL
            var existing = await _unitOfWork.Posts.FindAsync(p => p.Slug == slug);
            if (existing.Any())
                throw new Exception("Slug already exists"); // Đường dẫn này đã có bài khác xài rồi!

            var post = new Post
            {
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                Status = dto.Status,
                CategoryId = dto.CategoryId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Gọi hàm tự động tạo Thẻ (Nếu thẻ chưa có trong DB thì tự thêm)
            await SyncTags(post, dto.Tags);

            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();

            return await GetPostBySlugAsync(post.Slug);
        }
```

---

### PHẦN 2.3 - CƠ CHẾ XÓA MỀM (SOFT DELETE) BÀI VIẾT

Trong hệ thống lớn, khi một bài viết đã được public và có người xem (lưu log, lưu bình luận), ta TUYỆT ĐỐI KHÔNG ĐƯỢC chạy lệnh `Remove()` để xóa cứng khỏi Database. Điều đó sẽ gây lỗi văng khóa ngoại (Foreign Key Constraint) hoặc làm mất dữ liệu thống kê. 

Giải pháp là **Xóa Mềm (Soft Delete)** thông qua chuyển đổi Trạng thái.

**Tệp:** `MyPetClinic.Application/Services/PostService.cs` *(Logic Xóa)*

```csharp
        public async Task DeletePostAsync(long id)
        {
            // 1. Tìm bài viết
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null) throw new Exception("Post not found");

            // 2. SOFT DELETE (Thay đổi trạng thái thay vì xóa)
            post.Status = "archived"; // Chuyển thành trạng thái Lưu Trữ / Đã xóa
            
            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();
        }
```

**Kết quả:** 
Khi gọi API Get Danh sách Bài viết ở trang chủ Khách hàng, lệnh lọc của ta là `p => p.Status == "published"`. Do bài viết vừa xóa đã bị đổi thành `"archived"`, nó lập tức tàng hình khỏi Website mà không cần phải xóa một dòng dữ liệu nào trong SQL!

---
*(Hết tài liệu đào tạo chuyên sâu Admin: Quản lý Bài viết & Danh mục)*
