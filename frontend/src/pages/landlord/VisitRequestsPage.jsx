import React, { useState, useEffect } from "react";
import { getVisits, updateVisitStatus } from "../../api/visitsApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import toast from "react-hot-toast";

const STATUS_LABELS = ["Pending", "Accepted", "Rejected"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function VisitRequestsPage() {
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

  const handleStatus = async (id, status) => {
    try {
      await updateVisitStatus(id, status);
      setVisits((v) =>
        v.map((visit) => (visit.id === id ? { ...visit, status } : visit)),
      );
      toast.success(`Visit ${status === 1 ? "accepted" : "rejected"}.`);
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed to update.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>📅 Visit Requests</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : visits.length === 0 ? (
        <div className="empty-state">No visit requests yet.</div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Tenant</th>
                <th>Property</th>
                <th>Scheduled Date</th>
                <th>Requested On</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {visits.map((v) => (
                <tr key={v.id}>
                  <td>{v.tenantName}</td>
                  <td>{v.propertyTitle}</td>
                  <td>{new Date(v.scheduledDate).toLocaleString()}</td>
                  <td>{new Date(v.createdAt).toLocaleDateString()}</td>
                  <td>
                    <span className={`badge ${STATUS_CLASSES[v.status]}`}>
                      {STATUS_LABELS[v.status]}
                    </span>
                  </td>
                  <td className="action-cell">
                    {v.status === 0 && (
                      <>
                        <button
                          className="btn btn-success btn-sm"
                          onClick={() => handleStatus(v.id, 1)}
                        >
                          Accept
                        </button>
                        <button
                          className="btn btn-danger btn-sm"
                          onClick={() => handleStatus(v.id, 2)}
                        >
                          Reject
                        </button>
                      </>
                    )}
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
