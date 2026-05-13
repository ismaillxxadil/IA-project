import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './context/AuthContext';
import { NotificationProvider } from './context/NotificationContext';
import ProtectedRoute from './components/ProtectedRoute';
import Navbar from './components/Navbar';

// Public pages
import HomePage from './pages/HomePage';
import PropertyDetailPage from './pages/PropertyDetailPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';

// Tenant pages
import FavoritesPage from './pages/tenant/FavoritesPage';
import MyVisitsPage from './pages/tenant/MyVisitsPage';
import MyApplicationsPage from './pages/tenant/MyApplicationsPage';
import ReviewPage from './pages/tenant/ReviewPage';

// Landlord pages
import MyPropertiesPage from './pages/landlord/MyPropertiesPage';
import PropertyFormPage from './pages/landlord/PropertyFormPage';
import VisitRequestsPage from './pages/landlord/VisitRequestsPage';
import ApplicationsPage from './pages/landlord/ApplicationsPage';

// Admin pages
import AdminLandlordsPage from './pages/admin/AdminLandlordsPage';
import AdminPropertiesPage from './pages/admin/AdminPropertiesPage';

function App() {
  return (
    <AuthProvider>
      <NotificationProvider>
        <BrowserRouter>
          <Navbar />
          <Toaster position="top-right" toastOptions={{ duration: 4000 }} />
          <Routes>
            {/* Public */}
            <Route path="/" element={<HomePage />} />
            <Route path="/properties/:id" element={<PropertyDetailPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Tenant */}
            <Route path="/tenant/favorites" element={
              <ProtectedRoute role="Tenant"><FavoritesPage /></ProtectedRoute>
            } />
            <Route path="/tenant/visits" element={
              <ProtectedRoute role="Tenant"><MyVisitsPage /></ProtectedRoute>
            } />
            <Route path="/tenant/applications" element={
              <ProtectedRoute role="Tenant"><MyApplicationsPage /></ProtectedRoute>
            } />
            <Route path="/tenant/review/:propertyId" element={
              <ProtectedRoute role="Tenant"><ReviewPage /></ProtectedRoute>
            } />

            {/* Landlord */}
            <Route path="/landlord/properties" element={
              <ProtectedRoute role="Landlord"><MyPropertiesPage /></ProtectedRoute>
            } />
            <Route path="/landlord/properties/new" element={
              <ProtectedRoute role="Landlord"><PropertyFormPage /></ProtectedRoute>
            } />
            <Route path="/landlord/properties/:id/edit" element={
              <ProtectedRoute role="Landlord"><PropertyFormPage /></ProtectedRoute>
            } />
            <Route path="/landlord/visits" element={
              <ProtectedRoute role="Landlord"><VisitRequestsPage /></ProtectedRoute>
            } />
            <Route path="/landlord/applications" element={
              <ProtectedRoute role="Landlord"><ApplicationsPage /></ProtectedRoute>
            } />

            {/* Admin */}
            <Route path="/admin/landlords" element={
              <ProtectedRoute role="Admin"><AdminLandlordsPage /></ProtectedRoute>
            } />
            <Route path="/admin/properties" element={
              <ProtectedRoute role="Admin"><AdminPropertiesPage /></ProtectedRoute>
            } />

            {/* Fallback */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </NotificationProvider>
    </AuthProvider>
  );
}

export default App;
