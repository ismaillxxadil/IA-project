import api from './axios';

export const requestVisit = (data) => api.post('/api/visits', data);
export const getVisits = () => api.get('/api/visits');
// status: 1 = Accepted, 2 = Rejected
export const updateVisitStatus = (id, status) =>
  api.put(`/api/visits/${id}/status`, { status });
