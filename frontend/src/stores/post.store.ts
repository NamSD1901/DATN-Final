import { defineStore } from 'pinia';
import { postService, postCategoryService } from '../services/post.service';

export const usePostStore = defineStore('post', {
  state: () => ({
    posts: [] as any[],
    totalCount: 0,
    pageIndex: 1,
    pageSize: 10,
    currentPost: null as any,
    categories: [] as any[],
    loading: false,
    error: null as string | null
  }),
  actions: {
    // ---- Posts ----
    async fetchPublicPosts(page = 1, limit = 10, search?: string, categorySlug?: string) {
      this.loading = true;
      try {
        const res = await postService.getPublicPosts(page, limit, search, categorySlug);
        this.posts = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.pageIndex = res.pageIndex || 1;
        this.pageSize = res.pageSize || 10;
      } catch (err: any) {
        this.error = err.response?.data?.message || err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchAdminPosts(page = 1, limit = 10, search?: string, status?: string, categoryId?: number) {
      this.loading = true;
      try {
        const res = await postService.getAdminPosts(page, limit, search, status, categoryId);
        this.posts = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.pageIndex = res.pageIndex || 1;
        this.pageSize = res.pageSize || 10;
      } catch (err: any) {
        this.error = err.response?.data?.message || err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchPostBySlug(slug: string) {
      this.loading = true;
      try {
        this.currentPost = await postService.getPostBySlug(slug);
        return this.currentPost;
      } catch (err: any) {
        this.error = err.response?.data?.message || err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },
    async createPost(data: any) {
      this.loading = true;
      try {
        await postService.createPost(data);
      } catch (err: any) {
        throw new Error(err.response?.data?.message || err.message);
      } finally {
        this.loading = false;
      }
    },
    async updatePost(id: number, data: any) {
      this.loading = true;
      try {
        await postService.updatePost(id, data);
      } catch (err: any) {
        throw new Error(err.response?.data?.message || err.message);
      } finally {
        this.loading = false;
      }
    },
    async deletePost(id: number) {
      this.loading = true;
      try {
        await postService.deletePost(id);
      } catch (err: any) {
        throw new Error(err.response?.data?.message || err.message);
      } finally {
        this.loading = false;
      }
    },
    async incrementView(id: number) {
      try {
        await postService.incrementViewCount(id);
      } catch (err) {
        console.error("Lỗi tăng lượt xem", err);
      }
    },

    // ---- Categories ----
    async fetchCategories() {
      this.loading = true;
      try {
        this.categories = await postCategoryService.getAll();
      } catch (err: any) {
        this.error = err.response?.data?.message || err.message;
      } finally {
        this.loading = false;
      }
    },
    async createCategory(data: any) {
      try {
        await postCategoryService.create(data);
        await this.fetchCategories();
      } catch (err: any) {
        throw new Error(err.response?.data?.message || err.message);
      }
    },
    async updateCategory(id: number, data: any) {
      try {
        await postCategoryService.update(id, data);
        await this.fetchCategories();
      } catch (err: any) {
        throw new Error(err.response?.data?.message || err.message);
      }
    },
    async deleteCategory(id: number) {
      try {
        await postCategoryService.delete(id);
        await this.fetchCategories();
      } catch (err: any) {
        throw new Error(err.response?.data?.message || err.message);
      }
    }
  }
});

