import React, { useState, useEffect, useCallback } from "react";
import { getProperties } from "../api/propertiesApi";
import { addFavorite, removeFavorite, getFavorites } from "../api/favoritesApi";
import { getApiErrorMessage } from "../api/errorMessage";
import PropertyCard from "../components/PropertyCard";
import { useAuth } from "../context/AuthContext";
import toast from "react-hot-toast";

const PROPERTY_TYPES = [
  { label: "All Types", value: "" },
  { label: "Apartment", value: 0 },
  { label: "Villa", value: 1 },
  { label: "Shared Room", value: 2 },
];

export default function HomePage() {
  const { user, isAuthenticated } = useAuth();
  const [properties, setProperties] = useState([]);
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState({
    location: "",
    minPrice: "",
    maxPrice: "",
    type: "",
  });

  const fetchProperties = useCallback(async () => {
    setLoading(true);
    try {
      const params = {};
      if (filters.location) params.location = filters.location;
      if (filters.minPrice) params.minPrice = filters.minPrice;
      if (filters.maxPrice) params.maxPrice = filters.maxPrice;
      if (filters.type !== "") params.type = filters.type;
      const res = await getProperties(params);
      setProperties(res.data);
    } catch (err) {
      toast.error(getApiErrorMessage(err, "Failed to load properties."));
    } finally {
      setLoading(false);
    }
  }, [filters]);

  const fetchFavorites = useCallback(async () => {
    if (!isAuthenticated || user?.role !== "Tenant") return;
    try {
      const res = await getFavorites();
      setFavorites(res.data.map((f) => f.propertyId));
    } catch {}
  }, [isAuthenticated, user]);

  useEffect(() => {
    fetchProperties();
  }, [fetchProperties]);
  useEffect(() => {
    fetchFavorites();
  }, [fetchFavorites]);

  const handleFilterChange = (e) => {
    setFilters({ ...filters, [e.target.name]: e.target.value });
  };

  const handleSearch = (e) => {
    e.preventDefault();
    fetchProperties();
  };

  const toggleFavorite = async (propertyId) => {
    if (!isAuthenticated) {
      toast.error("Please login to save favorites.");
      return;
    }
    if (user?.role !== "Tenant") {
      toast.error("Only tenants can save favorites.");
      return;
    }
    const isFav = favorites.includes(propertyId);
    try {
      if (isFav) {
        await removeFavorite(propertyId);
        setFavorites((f) => f.filter((id) => id !== propertyId));
        toast.success("Removed from favorites.");
      } else {
        await addFavorite(propertyId);
        setFavorites((f) => [...f, propertyId]);
        toast.success("Added to favorites!");
      }
    } catch (err) {
      toast.error(err.response?.data || "Failed to update favorites.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>Browse Properties</h1>
        <p>Find your perfect home</p>
      </div>

      {/* Filters */}
      <form className="filter-bar" onSubmit={handleSearch}>
        <input
          type="text"
          name="location"
          placeholder="🔍 Location..."
          value={filters.location}
          onChange={handleFilterChange}
        />
        <input
          type="number"
          name="minPrice"
          placeholder="Min Price"
          value={filters.minPrice}
          onChange={handleFilterChange}
          min={0}
        />
        <input
          type="number"
          name="maxPrice"
          placeholder="Max Price"
          value={filters.maxPrice}
          onChange={handleFilterChange}
          min={0}
        />
        <select name="type" value={filters.type} onChange={handleFilterChange}>
          {PROPERTY_TYPES.map((t) => (
            <option key={t.value} value={t.value}>
              {t.label}
            </option>
          ))}
        </select>
        <button type="submit" className="btn btn-primary">
          Search
        </button>
        <button
          type="button"
          className="btn btn-outline"
          onClick={() => {
            setFilters({ location: "", minPrice: "", maxPrice: "", type: "" });
          }}
        >
          Clear
        </button>
      </form>

      {loading ? (
        <div className="loading">Loading properties...</div>
      ) : properties.length === 0 ? (
        <div className="empty-state">No properties found.</div>
      ) : (
        <div className="property-grid">
          {properties.map((p) => (
            <PropertyCard
              key={p.id}
              property={p}
              actions={
                isAuthenticated && user?.role === "Tenant" ? (
                  <button
                    className={`btn btn-sm ${favorites.includes(p.id) ? "btn-danger" : "btn-outline"}`}
                    onClick={() => toggleFavorite(p.id)}
                  >
                    {favorites.includes(p.id) ? "❤️ Saved" : "🤍 Save"}
                  </button>
                ) : null
              }
            />
          ))}
        </div>
      )}
    </div>
  );
}
