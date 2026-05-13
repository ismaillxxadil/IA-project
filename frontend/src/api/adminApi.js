import api from './axios';

export const getPendingLandlords = () => api.get('/api/admin/landlords/pending');
export const getPendingProperties = () => api.get('/api/admin/properties/pending');

// status: 1 = Approved, 2 = Rejected
export const updateLandlordStatus = (id, status) =>
  api.put(`/api/admin/landlords/${id}/status`, { status });

// approve: true/false
export const updatePropertyStatus = (id, approve) =>
  api.put(`/api/admin/properties/${id}/status`, { approve });
