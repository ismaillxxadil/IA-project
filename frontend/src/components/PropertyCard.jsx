import React from "react";
import { Link } from "react-router-dom";
import { FaMapMarkerAlt, FaDollarSign, FaBuilding } from "react-icons/fa";

const TYPE_LABELS = ["Apartment", "Villa", "Shared Room"];
const STATUS_LABELS = ["Pending", "Available", "Rented"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function PropertyCard({ property, actions }) {
  const {
    id,
    title,
    price,
    location,
    type,
    status,
    landlordName,
    hasParking,
    hasElevator,
    isFurnished,
    imageUrls,
  } = property;

  return (
    <div className="card property-card">
      <div className="property-img-placeholder">
        {imageUrls && imageUrls.length > 0 ? (
          <img
            src={imageUrls[0]}
            alt={title}
            style={{ width: "100%", height: "160px", objectFit: "cover" }}
          />
        ) : (
          <FaBuilding size={48} color="#aaa" />
        )}
      </div>
      <div className="card-body">
        <div className="property-card-header">
          <h3 className="property-title">{title}</h3>
          <span className={`badge ${STATUS_CLASSES[status]}`}>
            {STATUS_LABELS[status]}
          </span>
        </div>
        <p className="property-meta">
          <FaMapMarkerAlt /> {location}
        </p>
        <p className="property-meta">
          <FaDollarSign /> {price?.toLocaleString()} / month
        </p>
        <p className="property-meta">
          Type: <strong>{TYPE_LABELS[type]}</strong>
        </p>
        <p className="property-meta">Landlord: {landlordName}</p>
        <div className="amenity-badges">
          {hasParking && <span className="badge badge-info">🅿️ Parking</span>}
          {hasElevator && <span className="badge badge-info">🛗 Elevator</span>}
          {isFurnished && (
            <span className="badge badge-info">🛋️ Furnished</span>
          )}
        </div>
        <div className="property-card-actions">
          <Link to={`/properties/${id}`} className="btn btn-primary btn-sm">
            View Details
          </Link>
          {actions}
        </div>
      </div>
    </div>
  );
}
