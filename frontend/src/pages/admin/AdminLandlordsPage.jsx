import React, { useState, useEffect } from "react";
import { getPendingLandlords, updateLandlordStatus } from "../../api/adminApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import toast from "react-hot-toast";

export default function AdminLandlordsPage() {
  const [landlords, setLandlords] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      const res = await getPendingLandlords();
      setLandlords(res.data);
    } catch (err) {
      toast.error(getApiErrorMessage(err, "Failed to load pending landlords."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleAction = async (id, approve) => {
    // AccountStatus enum: 0=Pending, 1=Approved, 2=Rejected
    const status = approve ? 1 : 2;
    try {
      await updateLandlordStatus(id, status);
      setLandlords((l) => l.filter((landlord) => landlord.id !== id));
      toast.success(`Landlord ${approve ? "approved" : "rejected"}.`);
    } catch (err) {
      toast.error(err.response?.data?.message || "Failed.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>👤 Pending Landlord Accounts</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : landlords.length === 0 ? (
        <div className="empty-state">✅ No pending landlord accounts.</div>
      ) : (
        <div className="table-container">
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Registered On</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {landlords.map((l) => (
                <tr key={l.id}>
                  <td>{l.fullName}</td>
                  <td>{l.email}</td>
                  <td>{new Date(l.createdAt).toLocaleDateString()}</td>
                  <td className="action-cell">
                    <button
                      className="btn btn-success btn-sm"
                      onClick={() => handleAction(l.id, true)}
                    >
                      ✓ Approve
                    </button>
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => handleAction(l.id, false)}
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
