import React, { useState, useEffect } from "react";
import {
  getApplications,
  updateApplicationStatus,
} from "../../api/applicationsApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import toast from "react-hot-toast";

const STATUS_LABELS = ["Pending", "Accepted", "Rejected"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function ApplicationsPage() {
  const [applications, setApplications] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getApplications()
      .then((res) => setApplications(res.data))
      .catch((err) =>
        toast.error(getApiErrorMessage(err, "Failed to load applications.")),
      )
      .finally(() => setLoading(false));
  }, []);

  const handleStatus = async (id, status) => {
    try {
      await updateApplicationStatus(id, status);
      setApplications((apps) =>
        apps.map((a) => (a.id === id ? { ...a, status } : a)),
      );
      toast.success(`Application ${status === 1 ? "accepted" : "rejected"}.`);
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed to update.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>📄 Rental Applications</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : applications.length === 0 ? (
        <div className="empty-state">No rental applications yet.</div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Tenant</th>
                <th>Property</th>
                <th>Applied On</th>
                <th>Document</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {applications.map((a) => (
                <tr key={a.id}>
                  <td>{a.tenantName}</td>
                  <td>{a.propertyTitle}</td>
                  <td>{new Date(a.appliedAt).toLocaleDateString()}</td>
                  <td>
                    <a
                      href={a.documentUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="link"
                    >
                      View Doc
                    </a>
                  </td>
                  <td>
                    <span className={`badge ${STATUS_CLASSES[a.status]}`}>
                      {STATUS_LABELS[a.status]}
                    </span>
                  </td>
                  <td className="action-cell">
                    {a.status === 0 && (
                      <>
                        <button
                          className="btn btn-success btn-sm"
                          onClick={() => handleStatus(a.id, 1)}
                        >
                          Accept
                        </button>
                        <button
                          className="btn btn-danger btn-sm"
                          onClick={() => handleStatus(a.id, 2)}
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
