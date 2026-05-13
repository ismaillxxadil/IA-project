import React, { useState, useEffect } from "react";
import { getFavorites, removeFavorite } from "../../api/favoritesApi";
import { getApiErrorMessage } from "../../api/errorMessage";
import { Link } from "react-router-dom";
import toast from "react-hot-toast";

export default function FavoritesPage() {
  const [favorites, setFavorites] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    try {
      const res = await getFavorites();
      setFavorites(res.data);
    } catch (err) {
      toast.error(getApiErrorMessage(err, "Failed to load favorites."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleRemove = async (propertyId) => {
    try {
      await removeFavorite(propertyId);
      setFavorites((f) => f.filter((fav) => fav.propertyId !== propertyId));
      toast.success("Removed from favorites.");
    } catch {
      toast.error("Failed to remove.");
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>❤️ My Favorites</h1>
      </div>
      {loading ? (
        <div className="loading">Loading...</div>
      ) : favorites.length === 0 ? (
        <div className="empty-state">
          <p>No saved properties yet.</p>
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
                <th>Saved On</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {favorites.map((f) => (
                <tr key={f.id}>
                  <td>
                    <Link to={`/properties/${f.propertyId}`}>
                      {f.propertyTitle}
                    </Link>
                  </td>
                  <td>{new Date(f.addedAt).toLocaleDateString()}</td>
                  <td>
                    <button
                      className="btn btn-danger btn-sm"
                      onClick={() => handleRemove(f.propertyId)}
                    >
                      Remove
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
