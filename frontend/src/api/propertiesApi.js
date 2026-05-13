import api from './axios';

export const getProperties = (filters = {}) => api.get('/api/properties', { params: filters });
export const getPropertyById = (id) => api.get(`/api/properties/${id}`);
export const getMyProperties = () => api.get('/api/properties/my-properties');
export const createProperty = (data) => api.post('/api/properties', data);
export const updateProperty = (id, data) => api.put(`/api/properties/${id}`, data);
export const deleteProperty = (id) => api.delete(`/api/properties/${id}`);
