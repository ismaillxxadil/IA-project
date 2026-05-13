import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/authApi";
import toast from "react-hot-toast";

const ROLES = [
  { label: "Tenant", value: 2 },
  { label: "Landlord", value: 1 },
];

export default function RegisterPage() {
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    role: 2,
  });
  const [loading, setLoading] = useState(false);
  const [registered, setRegistered] = useState(false);
  const [isLandlord, setIsLandlord] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    const value =
      e.target.name === "role" ? parseInt(e.target.value) : e.target.value;
    setForm({ ...form, [e.target.name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await register(form);

      if (form.role === 1) {
        // Landlord: redirect to login with pending message
        setIsLandlord(true);
        setRegistered(true);
      } else {
        // Tenant: auto-approved, go to login
        toast.success("Account created! Please login.");
        navigate("/login");
      }
    } catch (err) {
      const msg = err.response?.data;
      toast.error(typeof msg === "string" ? msg : "Registration failed.");
    } finally {
      setLoading(false);
    }
  };

  if (registered && isLandlord) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <h1>SmartRent</h1>
          <div className="alert alert-warning">
            <h3>⏳ Account Pending Approval</h3>
            <p>
              Your landlord account has been created successfully. You{" "}
              <strong>cannot login</strong> until an Admin reviews and approves
              your account.
            </p>
            <p>Please check back later.</p>
          </div>
          <Link
            to="/login"
            className="btn btn-primary btn-full"
            style={{ marginTop: "1rem" }}
          >
            Back to Login
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>SmartRent</h1>
        <h2>Create Account</h2>
        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label>Full Name</label>
            <input
              type="text"
              name="fullName"
              value={form.fullName}
              onChange={handleChange}
              required
              placeholder="John Doe"
            />
          </div>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              required
              placeholder="you@example.com"
            />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              required
              placeholder="••••••••"
              minLength={6}
            />
          </div>
          <div className="form-group">
            <label>I am a...</label>
            <select name="role" value={form.role} onChange={handleChange}>
              {ROLES.map((r) => (
                <option key={r.value} value={r.value}>
                  {r.label}
                </option>
              ))}
            </select>
          </div>
          {form.role === 1 && (
            <p className="form-hint">
              ℹ️ Landlord accounts require admin approval before you can login.
            </p>
          )}
          <button
            type="submit"
            className="btn btn-primary btn-full"
            disabled={loading}
          >
            {loading ? "Creating account..." : "Register"}
          </button>
        </form>
        <p className="auth-link">
          Already have an account? <Link to="/login">Login</Link>
        </p>
      </div>
    </div>
  );
}
