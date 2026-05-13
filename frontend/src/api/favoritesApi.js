import api from './axios';

export const addFavorite = (propertyId) => api.post('/api/favorites', { propertyId });
export const getFavorites = () => api.get('/api/favorites');
export const removeFavorite = (propertyId) => api.delete(`/api/favorites/${propertyId}`);
