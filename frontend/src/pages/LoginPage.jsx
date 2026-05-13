import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { login } from "../api/authApi";
import { useAuth } from "../context/AuthContext";
import toast from "react-hot-toast";

const ROLE_MAP = { 0: "Admin", 1: "Landlord", 2: "Tenant" };

export default function LoginPage() {
  const [form, setForm] = useState({ email: "", password: "" });
  const [loading, setLoading] = useState(false);
  const { login: authLogin } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await login(form);
      const { token, user } = res.data;

      // Normalize numeric role to string so ProtectedRoute works
      const normalizedUser = {
        ...user,
        role: ROLE_MAP[user.role] ?? user.role,
      };

      authLogin(token, normalizedUser);
      toast.success(`Welcome back, ${user.fullName}!`);

      // Role-based redirect
      if (normalizedUser.role === "Admin") navigate("/admin/landlords");
      else if (normalizedUser.role === "Landlord")
        navigate("/landlord/properties");
      else navigate("/");
    } catch (err) {
      const msg = err.response?.data || "Login failed. Check your credentials.";
      toast.error(typeof msg === "string" ? msg : "Login failed.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>SmartRent</h1>
        <h2>Login</h2>
        <form onSubmit={handleSubmit} className="form">
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
            />
          </div>
          <button
            type="submit"
            className="btn btn-primary btn-full"
            disabled={loading}
          >
            {loading ? "Logging in..." : "Login"}
          </button>
        </form>
        <p className="auth-link">
          Don't have an account? <Link to="/register">Register</Link>
        </p>
      </div>
    </div>
  );
}
