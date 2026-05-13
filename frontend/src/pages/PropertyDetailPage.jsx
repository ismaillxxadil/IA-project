import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getPropertyById } from "../api/propertiesApi";
import { getPropertyReviews } from "../api/reviewsApi";
import { addFavorite, removeFavorite, getFavorites } from "../api/favoritesApi";
import { requestVisit } from "../api/visitsApi";
import { submitApplication } from "../api/applicationsApi";
import { useAuth } from "../context/AuthContext";
import toast from "react-hot-toast";
import { FaMapMarkerAlt, FaBuilding, FaStar } from "react-icons/fa";

const TYPE_LABELS = ["Apartment", "Villa", "Shared Room"];
const STATUS_LABELS = ["Pending", "Available", "Rented"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function PropertyDetailPage() {
  const { id } = useParams();
  const { user, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [property, setProperty] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [isFavorite, setIsFavorite] = useState(false);
  const [loading, setLoading] = useState(true);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  // Visit modal state
  const [showVisitModal, setShowVisitModal] = useState(false);
  const [visitDate, setVisitDate] = useState("");
  const [visitLoading, setVisitLoading] = useState(false);

  // Application modal state
  const [showAppModal, setShowAppModal] = useState(false);
  const [docUrl, setDocUrl] = useState("");
  const [appLoading, setAppLoading] = useState(false);

  useEffect(() => {
    const load = async () => {
      try {
        const [propRes, revRes] = await Promise.all([
          getPropertyById(id),
          getPropertyReviews(id),
        ]);
        setProperty(propRes.data);
        setReviews(revRes.data);
        setSelectedImageIndex(0);

        if (isAuthenticated && user?.role === "Tenant") {
          const favRes = await getFavorites();
          setIsFavorite(favRes.data.some((f) => f.propertyId === parseInt(id)));
        }
      } catch {
        toast.error("Property not found.");
        navigate("/");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [id, isAuthenticated, user, navigate]);

  const toggleFavorite = async () => {
    if (!isAuthenticated) {
      navigate("/login");
      return;
    }
    try {
      if (isFavorite) {
        await removeFavorite(parseInt(id));
        setIsFavorite(false);
        toast.success("Removed from favorites.");
      } else {
        await addFavorite(parseInt(id));
        setIsFavorite(true);
        toast.success("Added to favorites!");
      }
    } catch (err) {
      toast.error(err.response?.data || "Failed.");
    }
  };

  const handleVisitSubmit = async (e) => {
    e.preventDefault();
    setVisitLoading(true);
    try {
      await requestVisit({
        propertyId: parseInt(id),
        scheduledDate: new Date(visitDate).toISOString(),
      });
      toast.success("Visit request submitted!");
      setShowVisitModal(false);
      setVisitDate("");
      navigate("/tenant/visits");
    } catch (err) {
      toast.error(err.response?.data || "Failed to submit visit.");
    } finally {
      setVisitLoading(false);
    }
  };

  const handleApplicationSubmit = async (e) => {
    e.preventDefault();
    setAppLoading(true);
    try {
      await submitApplication({
        propertyId: parseInt(id),
        documentUrl: docUrl,
      });
      toast.success("Application submitted!");
      setShowAppModal(false);
      setDocUrl("");
    } catch (err) {
      toast.error(err.response?.data || "Failed to submit application.");
    } finally {
      setAppLoading(false);
    }
  };

  const avgRating = reviews.length
    ? (reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length).toFixed(
        1,
      )
    : null;

  if (loading) return <div className="loading">Loading...</div>;
  if (!property) return null;

  const isTenant = isAuthenticated && user?.role === "Tenant";
  const isAvailable = property.status === 1;
  const imageUrls = property.imageUrls || [];
  const selectedImage = imageUrls[selectedImageIndex] || imageUrls[0];

  return (
    <div className="page">
      <div className="detail-container">
        {/* Header */}
        <div className="detail-header">
          <div>
            <h1>{property.title}</h1>
            <p className="property-meta">
              <FaMapMarkerAlt /> {property.location}
            </p>
          </div>
          <div className="detail-header-right">
            <span className={`badge ${STATUS_CLASSES[property.status]}`}>
              {STATUS_LABELS[property.status]}
            </span>
            {isTenant && (
              <button
                className={`btn ${isFavorite ? "btn-danger" : "btn-outline"}`}
                onClick={toggleFavorite}
              >
                {isFavorite ? "❤️ Saved" : "🤍 Save"}
              </button>
            )}
          </div>
        </div>

        {/* Images */}
        <div className="detail-gallery card">
          {imageUrls.length > 0 ? (
            <>
              <div className="detail-gallery-main">
                <img
                  src={selectedImage}
                  alt={property.title}
                  className="detail-gallery-image"
                />
                <div className="detail-gallery-count">
                  {selectedImageIndex + 1} / {imageUrls.length}
                </div>
              </div>
              <div className="detail-gallery-thumbs">
                {imageUrls.map((url, idx) => (
                  <button
                    key={`${url}-${idx}`}
                    type="button"
                    className={`detail-gallery-thumb ${idx === selectedImageIndex ? "is-active" : ""}`}
                    onClick={() => setSelectedImageIndex(idx)}
                    aria-label={`View image ${idx + 1}`}
                  >
                    <img
                      src={url}
                      alt={`${property.title} thumbnail ${idx + 1}`}
                    />
                  </button>
                ))}
              </div>
            </>
          ) : (
            <div className="detail-gallery-empty">
              <FaBuilding size={72} color="#94a3b8" />
              <p>No images provided for this property yet.</p>
            </div>
          )}
        </div>

        {/* Info grid */}
        <div className="detail-grid">
          <div className="detail-section">
            <h2>Details</h2>
            <p>
              <strong>Price:</strong> ${property.price?.toLocaleString()} /
              month
            </p>
            <p>
              <strong>Type:</strong> {TYPE_LABELS[property.type]}
            </p>
            <p>
              <strong>Landlord:</strong> {property.landlordName}
            </p>
            <p>
              <strong>Posted:</strong>{" "}
              {new Date(property.createdAt).toLocaleDateString()}
            </p>

            <h3>Amenities</h3>
            <div className="amenity-badges">
              {property.hasParking && (
                <span className="badge badge-info">🅿️ Parking</span>
              )}
              {property.hasElevator && (
                <span className="badge badge-info">🛗 Elevator</span>
              )}
              {property.isFurnished && (
                <span className="badge badge-info">🛋️ Furnished</span>
              )}
              {!property.hasParking &&
                !property.hasElevator &&
                !property.isFurnished && (
                  <span className="text-muted">
                    No special amenities listed
                  </span>
                )}
            </div>
          </div>

          <div className="detail-section">
            <h2>Description</h2>
            <p>{property.description}</p>
          </div>
        </div>

        {/* Tenant Actions */}
        {isTenant && isAvailable && (
          <div className="detail-actions">
            <button
              className="btn btn-primary"
              onClick={() => setShowVisitModal(true)}
            >
              📅 Schedule a Visit
            </button>
            <button
              className="btn btn-success"
              onClick={() => setShowAppModal(true)}
            >
              📄 Apply to Rent
            </button>
          </div>
        )}

        {/* Reviews */}
        <div className="detail-section" style={{ marginTop: "2rem" }}>
          <h2>
            Reviews{" "}
            {avgRating && (
              <span className="rating-avg">
                <FaStar color="#f5a623" /> {avgRating}
              </span>
            )}
          </h2>
          {reviews.length === 0 ? (
            <p className="text-muted">No reviews yet.</p>
          ) : (
            reviews.map((r) => (
              <div key={r.id} className="review-card">
                <div className="review-header">
                  <strong>{r.tenantName}</strong>
                  <span>{"⭐".repeat(r.rating)}</span>
                </div>
                <p>{r.comment}</p>
                <small className="text-muted">
                  {new Date(r.createdAt).toLocaleDateString()}
                </small>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Visit Modal */}
      {showVisitModal && (
        <div className="modal-overlay" onClick={() => setShowVisitModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Schedule a Visit</h2>
            <form onSubmit={handleVisitSubmit} className="form">
              <div className="form-group">
                <label>Preferred Date & Time</label>
                <input
                  type="datetime-local"
                  value={visitDate}
                  onChange={(e) => setVisitDate(e.target.value)}
                  required
                  min={new Date().toISOString().slice(0, 16)}
                />
              </div>
              <div className="modal-actions">
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={visitLoading}
                >
                  {visitLoading ? "Submitting..." : "Submit Request"}
                </button>
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setShowVisitModal(false)}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Application Modal */}
      {showAppModal && (
        <div className="modal-overlay" onClick={() => setShowAppModal(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Rental Application</h2>
            <form onSubmit={handleApplicationSubmit} className="form">
              <div className="form-group">
                <label>Document URL (ID / Proof of income)</label>
                <input
                  type="url"
                  value={docUrl}
                  onChange={(e) => setDocUrl(e.target.value)}
                  required
                  placeholder="https://example.com/my-document.pdf"
                />
              </div>
              <div className="modal-actions">
                <button
                  type="submit"
                  className="btn btn-success"
                  disabled={appLoading}
                >
                  {appLoading ? "Submitting..." : "Submit Application"}
                </button>
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setShowAppModal(false)}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
