import React, { useState, useEffect } from "react";
import { getVisits } from "../../api/visitsApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";

const STATUS_LABELS = ["Pending", "Accepted", "Rejected"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function MyVisitsPage() {
  const [visits, setVisits] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getVisits()
      .then((res) => setVisits(res.data))
      .catch((err) =>
        toast.error(getApiErrorMessage(err, "Failed to load visits.")),
      )
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <div className="page-header">
        <h1>📅 My Visit Requests</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : visits.length === 0 ? (
        <div className="empty-state">
          <p>No visit requests yet.</p>
          <Link to="/" className="btn btn-primary">
            Browse Properties
          </Link>
        </div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Property</th>
                <th>Scheduled Date</th>
                <th>Requested On</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {visits.map((v) => (
                <tr key={v.id}>
                  <td>
                    <Link to={`/properties/${v.propertyId}`}>
                      {v.propertyTitle}
                    </Link>
                  </td>
                  <td>{new Date(v.scheduledDate).toLocaleString()}</td>
                  <td>{new Date(v.createdAt).toLocaleDateString()}</td>
                  <td>
                    <span className={`badge ${STATUS_CLASSES[v.status]}`}>
                      {STATUS_LABELS[v.status]}
                    </span>
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
