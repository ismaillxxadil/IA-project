import React, { useState, useEffect } from "react";
import { getPendingProperties, updatePropertyStatus } from "../../api/adminApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";

const TYPE_LABELS = ["Apartment", "Villa", "Shared Room"];

export default function AdminPropertiesPage() {
  const [properties, setProperties] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      const res = await getPendingProperties();
      setProperties(res.data);
    } catch (err) {
      toast.error(
        getApiErrorMessage(err, "Failed to load pending properties."),
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleAction = async (id, approve) => {
    try {
      await updatePropertyStatus(id, approve);
      setProperties((p) => p.filter((prop) => prop.id !== id));
      toast.success(`Property ${approve ? "approved" : "rejected"}.`);
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>🏠 Pending Property Listings</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : properties.length === 0 ? (
        <div className="empty-state">✅ No pending properties.</div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Landlord</th>
                <th>Type</th>
                <th>Location</th>
                <th>Price/mo</th>
                <th>Submitted</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {properties.map((p) => (
                <tr key={p.id}>
                  <td>
                    <Link to={`/properties/${p.id}`}>{p.title}</Link>
                  </td>
                  <td>{p.landlordName}</td>
                  <td>{TYPE_LABELS[p.type]}</td>
                  <td>{p.location}</td>
                  <td>${p.price?.toLocaleString()}</td>
                  <td>{new Date(p.createdAt).toLocaleDateString()}</td>
                  <td className="action-cell">
                    <button
                      className="btn btn-success btn-sm"
                      onClick={() => handleAction(p.id, true)}
                    >
                      ✓ Approve
                    </button>
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => handleAction(p.id, false)}
                    >
                      ✗ Reject
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
