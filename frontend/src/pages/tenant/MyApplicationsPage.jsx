import React, { useState, useEffect } from "react";
import { getApplications } from "../../api/applicationsApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import { Link, useNavigate } from "react-router-dom";
import toast from "react-hot-toast";

const STATUS_LABELS = ["Pending", "Accepted", "Rejected"];
const STATUS_CLASSES = ["badge-warning", "badge-success", "badge-danger"];

export default function MyApplicationsPage() {
  const [applications, setApplications] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    getApplications()
      .then((res) => setApplications(res.data))
      .catch((err) =>
        toast.error(getApiErrorMessage(err, "Failed to load applications.")),
      )
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <div className="page-header">
        <h1>📄 My Rental Applications</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : applications.length === 0 ? (
        <div className="empty-state">
          <p>No rental applications yet.</p>
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
                <th>Applied On</th>
                <th>Document</th>
                <th>Status</th>
                <th>Review</th>
              </tr>
            </thead>
            <tbody>
              {applications.map((a) => (
                <tr key={a.id}>
                  <td>
                    <Link to={`/properties/${a.propertyId}`}>
                      {a.propertyTitle}
                    </Link>
                  </td>
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
                  <td>
                    {a.status === 1 && (
                      <button
                        className="btn btn-primary btn-sm"
                        onClick={() =>
                          navigate(`/tenant/review/${a.propertyId}`)
                        }
                      >
                        Leave Review
                      </button>
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
