using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using Xunit;

namespace MyPetClinic.Tests
{
    public class PostServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _postService = new PostService(_unitOfWork);
        }

        [Fact]
        public async Task CreatePost_ValidData_ShouldCreateSuccessfully()
        {
            var adminId = Guid.NewGuid();
            var category = new PostCategory { Id = 1, Name = "Sức khỏe", Slug = "suc-khoe", IsActive = true };
            _context.PostCategories.Add(category);
            await _context.SaveChangesAsync();

            var dto = new CreatePostDto
            {
                Title = "Chăm sóc chó mùa đông",
                Slug = "cham-soc-cho-mua-dong",
                Summary = "Tóm tắt...",
                Content = "<p>Nội dung</p>",
                Thumbnail = "http://example.com/img.jpg",
                CategoryId = 1,
                Status = "published",
                Tags = new List<string> { "Cho", "Mua Dong" }
            };

            var result = await _postService.CreatePostAsync(dto, adminId);

            Assert.NotNull(result);
            Assert.Equal("Chăm sóc chó mùa đông", result.Title);
            
            var savedPost = await _context.Posts.Include(p => p.PostTags).FirstOrDefaultAsync(p => p.Id == result.Id);
            Assert.NotNull(savedPost);
            Assert.Equal(adminId, savedPost.AuthorId);
            Assert.Equal("published", savedPost.Status);
            Assert.Equal(2, savedPost.PostTags.Count);
        }

        [Fact]
        public async Task CreatePost_DuplicateSlug_ShouldThrowException()
        {
            var adminId = Guid.NewGuid();
            var category = new PostCategory { Id = 1, Name = "Sức khỏe", Slug = "suc-khoe", IsActive = true };
            _context.PostCategories.Add(category);
            _context.Posts.Add(new Post 
            { 
                Title = "Bài cũ", 
                Slug = "test-slug", 
                Summary = "Sum", 
                Content = "Content", 
                CategoryId = 1, 
                AuthorId = adminId,
                RowVersion = Array.Empty<byte>()
            });
            await _context.SaveChangesAsync();

            var dto = new CreatePostDto
            {
                Title = "Bài mới",
                Slug = "test-slug",
                Summary = "Sum",
                Content = "Content",
                CategoryId = 1,
                Status = "draft"
            };

            await Assert.ThrowsAsync<Exception>(() => _postService.CreatePostAsync(dto, adminId));
        }

        [Fact]
        public async Task GetPublicPosts_ShouldOnlyReturnPublishedNotArchived()
        {
            var adminId = Guid.NewGuid();
            _context.Posts.AddRange(
                new Post { Title = "Post 1", Slug = "p1", Summary = "S", Content = "C", AuthorId = adminId, Status = "published", PublishedAt = DateTime.UtcNow, RowVersion = Array.Empty<byte>() },
                new Post { Title = "Post 2", Slug = "p2", Summary = "S", Content = "C", AuthorId = adminId, Status = "draft", RowVersion = Array.Empty<byte>() },
                new Post { Title = "Post 3", Slug = "p3", Summary = "S", Content = "C", AuthorId = adminId, Status = "archived", PublishedAt = DateTime.UtcNow, RowVersion = Array.Empty<byte>() }
            );
            await _context.SaveChangesAsync();

            var result = await _postService.GetPublicPostsAsync(1, 10, null, null, null);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Post 1", result.Items.First().Title);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
