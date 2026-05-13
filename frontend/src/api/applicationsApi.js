import api from './axios';

export const submitApplication = (data) => api.post('/api/applications', data);
export const getApplications = () => api.get('/api/applications');
// status: 1 = Accepted, 2 = Rejected
export const updateApplicationStatus = (id, status) =>
  api.put(`/api/applications/${id}/status`, { status });
