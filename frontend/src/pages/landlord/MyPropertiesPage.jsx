import React, { useState, useEffect } from "react";
import { getMyProperties, deleteProperty } from "../../api/propertiesApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";

const TYPE_LABELS = ["Apartment", "Villa", "Shared Room"];
const STATUS_LABELS = ["Pending", "Available", "Rented"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function MyPropertiesPage() {
  const [properties, setProperties] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      const res = await getMyProperties();
      setProperties(res.data);
    } catch (err) {
      toast.error(getApiErrorMessage(err, "Failed to load properties."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleDelete = async (id, title) => {
    if (!window.confirm(`Delete "${title}"?`)) return;
    try {
      await deleteProperty(id);
      setProperties((p) => p.filter((prop) => prop.id !== id));
      toast.success("Property deleted.");
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed to delete.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>🏘️ My Properties</h1>
        <Link to="/landlord/properties/new" className="btn btn-primary">
          + Add Property
        </Link>
      </div>

      {loading ? (
        <div className="loading">Loading...</div>
      ) : properties.length === 0 ? (
        <div className="empty-state">
          <p>No properties yet.</p>
          <Link to="/landlord/properties/new" className="btn btn-primary">
            Create First Listing
          </Link>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Location</th>
                <th>Type</th>
                <th>Price/mo</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {properties.map((p) => (
                <tr key={p.id}>
                  <td>
                    <Link to={`/properties/${p.id}`}>{p.title}</Link>
                  </td>
                  <td>{p.location}</td>
                  <td>{TYPE_LABELS[p.type]}</td>
                  <td>${p.price?.toLocaleString()}</td>
                  <td>
                    <span className={`badge ${STATUS_CLASSES[p.status]}`}>
                      {STATUS_LABELS[p.status]}
                    </span>
                  </td>
                  <td className="action-cell">
                    <Link
                      to={`/landlord/properties/${p.id}/edit`}
                      className="btn btn-outline btn-sm"
                    >
                      Edit
                    </Link>
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => handleDelete(p.id, p.title)}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
