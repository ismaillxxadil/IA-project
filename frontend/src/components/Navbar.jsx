import React, { useState, useRef, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useNotifications } from "../context/NotificationContext";
import { FaBell, FaSignInAlt, FaUserPlus, FaSignOutAlt } from "react-icons/fa";

export default function Navbar() {
  const { user, logout, isAuthenticated } = useAuth();
  const { notifications, clearNotifications } = useNotifications();
  const [showNotif, setShowNotif] = useState(false);
  const notifRef = useRef(null);
  const navigate = useNavigate();

  const unread = notifications.length;

  // Close dropdown when clicking outside
  useEffect(() => {
    const handler = (e) => {
      if (notifRef.current && !notifRef.current.contains(e.target)) {
        setShowNotif(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <Link to="/">SmartRent</Link>
      </div>

      <div className="navbar-links">
        <Link to="/">Browse</Link>

        {isAuthenticated && user?.role === "Tenant" && (
          <>
            <Link to="/tenant/favorites">Favorites</Link>
            <Link to="/tenant/visits">My Visits</Link>
            <Link to="/tenant/applications">My Applications</Link>
          </>
        )}

        {isAuthenticated && user?.role === "Landlord" && (
          <>
            <Link to="/landlord/properties">My Properties</Link>
            <Link to="/landlord/visits">Visit Requests</Link>
            <Link to="/landlord/applications">Applications</Link>
          </>
        )}

        {isAuthenticated && user?.role === "Admin" && (
          <>
            <Link to="/admin/landlords">Pending Landlords</Link>
            <Link to="/admin/properties">Pending Properties</Link>
          </>
        )}
      </div>

      <div className="navbar-actions">
        {isAuthenticated ? (
          <>
            {/* Notification Bell */}
            <div className="notif-wrapper" ref={notifRef}>
              <button
                className="notif-btn"
                onClick={() => setShowNotif((s) => !s)}
                title="Notifications"
              >
                <FaBell />
                {unread > 0 && <span className="notif-badge">{unread}</span>}
              </button>

              {showNotif && (
                <div className="notif-dropdown">
                  <div className="notif-header">
                    <span>Notifications</span>
                    <button
                      className="notif-clear"
                      onClick={clearNotifications}
                    >
                      Clear
                    </button>
                  </div>
                  {notifications.length === 0 ? (
                    <p className="notif-empty">No notifications</p>
                  ) : (
                    notifications.map((n, i) => (
                      <div key={i} className="notif-item">
                        <span>{n.message}</span>
                        <small>{new Date(n.time).toLocaleTimeString()}</small>
                      </div>
                    ))
                  )}
                </div>
              )}
            </div>

            <span className="nav-username">👤 {user?.fullName}</span>
            <button className="btn btn-outline" onClick={handleLogout}>
              <FaSignOutAlt /> Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" className="btn btn-outline">
              <FaSignInAlt /> Login
            </Link>
            <Link to="/register" className="btn btn-primary">
              <FaUserPlus /> Register
            </Link>
          </>
        )}
      </div>
    </nav>
  );
}
