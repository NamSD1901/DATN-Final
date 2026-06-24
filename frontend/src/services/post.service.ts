import api from './api';

export const postService = {
  // Public Endpoints
  getPublicPosts: async (pageIndex = 1, pageSize = 10, search?: string, categorySlug?: string, tagSlug?: string) => {
    const params = new URLSearchParams({
      pageIndex: pageIndex.toString(),
      pageSize: pageSize.toString(),
    });
    if (search) params.append('search', search);
    if (categorySlug) params.append('categorySlug', categorySlug);
    if (tagSlug) params.append('tagSlug', tagSlug);
    
    const response = await api.get(`/posts?${params.toString()}`);
    return response.data;
  },

  getPostBySlug: async (slug: string) => {
    const response = await api.get(`/posts/${slug}`);
    return response.data;
  },

  incrementViewCount: async (id: number) => {
    const response = await api.post(`/posts/${id}/view`);
    return response.data;
  },

  getRelatedPosts: async (id: number, count = 3) => {
    const response = await api.get(`/posts/${id}/related?count=${count}`);
    return response.data;
  },

  // Admin Endpoints
  getAdminPosts: async (pageIndex = 1, pageSize = 10, search?: string, status?: string, categoryId?: number) => {
    const params = new URLSearchParams({
      pageIndex: pageIndex.toString(),
      pageSize: pageSize.toString(),
    });
    if (search) params.append('search', search);
    if (status) params.append('status', status);
    if (categoryId) params.append('categoryId', categoryId.toString());

    const response = await api.get(`/admin/posts?${params.toString()}`);
    return response.data;
  },

  createPost: async (data: any) => {
    const response = await api.post('/admin/posts', data);
    return response.data;
  },

  updatePost: async (id: number, data: any) => {
    const response = await api.put(`/admin/posts/${id}`, data);
    return response.data;
  },

  deletePost: async (id: number) => {
    const response = await api.delete(`/admin/posts/${id}`);
    return response.data;
  }
};

export const postCategoryService = {
  getAll: async () => {
    const response = await api.get('/PostCategories');
    return response.data;
  },
  getById: async (id: number) => {
    const response = await api.get(`/PostCategories/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/PostCategories', data);
    return response.data;
  },
  update: async (id: number, data: any) => {
    const response = await api.put(`/PostCategories/${id}`, data);
    return response.data;
  },
  delete: async (id: number) => {
    const response = await api.delete(`/PostCategories/${id}`);
    return response.data;
  }
};

export const bannerService = {
  getAll: async (onlyActive: boolean = false) => {
    const response = await api.get(`/Banners?onlyActive=${onlyActive}`);
    return response.data;
  },
  getById: async (id: number) => {
    const response = await api.get(`/Banners/${id}`);
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/Banners', data);
    return response.data;
  },
  update: async (id: number, data: any) => {
    const response = await api.put(`/Banners/${id}`, data);
    return response.data;
  },
  delete: async (id: number) => {
    const response = await api.delete(`/Banners/${id}`);
    return response.data;
  }
};

export const tagService = {
  getAll: async () => {
    const response = await api.get('/Tags');
    return response.data;
  },
  create: async (data: any) => {
    const response = await api.post('/Tags', data);
    return response.data;
  },
  update: async (id: number, data: any) => {
    const response = await api.put(`/Tags/${id}`, data);
    return response.data;
  },
  delete: async (id: number) => {
    const response = await api.delete(`/Tags/${id}`);
    return response.data;
  }
};
