import api from './axios';

export const addReview = (data) => api.post('/api/reviews', data);
export const getPropertyReviews = (propertyId) => api.get(`/api/reviews/property/${propertyId}`);
