/**
 * Service API Client untuk request ke backend
 */

async function request(endpoint, options = {}) {
  const url = endpoint.startsWith('http') ? endpoint : endpoint;
  const defaultHeaders = {
    'Content-Type': 'application/json',
    Accept: 'application/json'
  };

  const config = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...(options.headers || {})
    }
  };

  if (config.body && typeof config.body === 'object') {
    config.body = JSON.stringify(config.body);
  }

  const response = await fetch(url, config);
  const data = await response.json().catch(() => ({}));

  if (!response.ok) {
    const errorMsg = data.message || `Request failed with status ${response.status}`;
    throw new Error(errorMsg);
  }

  return data;
}

export const api = {
  // Products
  getProducts(params = {}) {
    const searchParams = new URLSearchParams();
    if (params.page) searchParams.append('page', params.page);
    if (params.limit) searchParams.append('limit', params.limit);
    if (params.search) searchParams.append('search', params.search);
    if (params.isavailable !== undefined && params.isavailable !== '') {
      searchParams.append('isavailable', params.isavailable);
    }
    const query = searchParams.toString();
    return request(`/api/products${query ? `?${query}` : ''}`);
  },

  getProduct(id) {
    return request(`/api/products/${id}`);
  },

  createProduct(data) {
    return request('/api/products', {
      method: 'POST',
      body: data
    });
  },

  updateProduct(id, data) {
    return request(`/api/products/${id}`, {
      method: 'PUT',
      body: data
    });
  },

  deleteProduct(id) {
    return request(`/api/products/${id}`, {
      method: 'DELETE'
    });
  },

  // Plan Downtime
  getPlanDowntimes(params = {}) {
    const searchParams = new URLSearchParams();
    if (params.page) searchParams.append('page', params.page);
    if (params.limit) searchParams.append('limit', params.limit);
    if (params.search) searchParams.append('search', params.search);
    if (params.department) searchParams.append('department', params.department);
    const query = searchParams.toString();
    return request(`/api/plan-downtimes${query ? `?${query}` : ''}`);
  },

  createPlanDowntime(data) {
    return request('/api/plan-downtimes', {
      method: 'POST',
      body: data
    });
  },

  updatePlanDowntime(id, data) {
    return request(`/api/plan-downtimes/${id}`, {
      method: 'PUT',
      body: data
    });
  },

  deletePlanDowntime(id) {
    return request(`/api/plan-downtimes/${id}`, {
      method: 'DELETE'
    });
  },

  // Unplan Downtime
  getUnplanDowntimes(params = {}) {
    const searchParams = new URLSearchParams();
    if (params.page) searchParams.append('page', params.page);
    if (params.limit) searchParams.append('limit', params.limit);
    if (params.search) searchParams.append('search', params.search);
    if (params.department) searchParams.append('department', params.department);
    const query = searchParams.toString();
    return request(`/api/unplan-downtimes${query ? `?${query}` : ''}`);
  },

  createUnplanDowntime(data) {
    return request('/api/unplan-downtimes', {
      method: 'POST',
      body: data
    });
  },

  updateUnplanDowntime(id, data) {
    return request(`/api/unplan-downtimes/${id}`, {
      method: 'PUT',
      body: data
    });
  },

  deleteUnplanDowntime(id) {
    return request(`/api/unplan-downtimes/${id}`, {
      method: 'DELETE'
    });
  },

  // OpenAPI Specification
  getOpenApiSpec() {
    return request('/api/openapi.json');
  }
};

export default api;
