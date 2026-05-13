import React, { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  createProperty,
  updateProperty,
  getPropertyById,
} from "../../api/propertiesApi";
import { useAuth } from "../../context/AuthContext";
import toast from "react-hot-toast";

const PROPERTY_TYPES = [
  { label: "Apartment", value: 0 },
  { label: "Villa", value: 1 },
  { label: "Shared Room", value: 2 },
];

const DEFAULT_FORM = {
  title: "",
  description: "",
  price: "",
  location: "",
  type: 0,
  hasParking: false,
  hasElevator: false,
  isFurnished: false,
  images: [],
};

export default function PropertyFormPage() {
  const { id } = useParams(); // only set when editing
  const isEditing = !!id;
  const { user } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState(DEFAULT_FORM);
  const [loading, setLoading] = useState(false);
  const [fetchLoading, setFetchLoading] = useState(isEditing);

  useEffect(() => {
    if (!isEditing) return;
    getPropertyById(id)
      .then((res) => {
        const p = res.data;
        setForm({
          title: p.title,
          description: p.description,
          price: p.price,
          location: p.location,
          type: p.type,
          hasParking: p.hasParking,
          hasElevator: p.hasElevator,
          isFurnished: p.isFurnished,
          images: p.imageUrls || [],
        });
      })
      .catch(() => {
        toast.error("Property not found.");
        navigate("/landlord/properties");
      })
      .finally(() => setFetchLoading(false));
  }, [id, isEditing, navigate]);

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm({
      ...form,
      [name]:
        type === "checkbox"
          ? checked
          : name === "type" || name === "price"
            ? parseFloat(value)
            : value,
    });
  };

  const handleImageChange = (index, value) => {
    const images = [...form.images];
    images[index] = value;
    setForm({ ...form, images });
  };

  const addImage = () => setForm({ ...form, images: [...form.images, ""] });
  const removeImage = (index) =>
    setForm({ ...form, images: form.images.filter((_, i) => i !== index) });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (isEditing) {
        // send images as imageUrls
        const payload = { ...form, imageUrls: form.images };
        await updateProperty(id, payload);
        toast.success("Property updated!");
      } else {
        await createProperty({
          ...form,
          landlordId: user.id,
          imageUrls: form.images,
        });
        toast.success("Property created! Awaiting admin approval.");
      }
      navigate("/landlord/properties");
    } catch (err) {
      const msg = err.response?.data;
      toast.error(typeof msg === "string" ? msg : "Failed to save property.");
    } finally {
      setLoading(false);
    }
  };

  if (fetchLoading) return <div className="loading">Loading...</div>;

  return (
    <div className="page">
      <div
        className="auth-card"
        style={{ maxWidth: "600px", margin: "2rem auto" }}
      >
        <h1>{isEditing ? "Edit Property" : "➕ New Property Listing"}</h1>
        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label>Title</label>
            <input
              name="title"
              value={form.title}
              onChange={handleChange}
              required
              placeholder="Beautiful apartment..."
            />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea
              name="description"
              value={form.description}
              onChange={handleChange}
              required
              rows={4}
              placeholder="Describe your property..."
            />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Price (per month, $)</label>
              <input
                type="number"
                name="price"
                value={form.price}
                onChange={handleChange}
                required
                min={0}
                step="0.01"
              />
            </div>
            <div className="form-group">
              <label>Property Type</label>
              <select name="type" value={form.type} onChange={handleChange}>
                {PROPERTY_TYPES.map((t) => (
                  <option key={t.value} value={t.value}>
                    {t.label}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div className="form-group">
            <label>Location</label>
            <input
              name="location"
              value={form.location}
              onChange={handleChange}
              required
              placeholder="City, Street Address..."
            />
          </div>
          <div className="form-group">
            <label>Amenities</label>
            <div className="checkbox-group">
              <label>
                <input
                  type="checkbox"
                  name="hasParking"
                  checked={form.hasParking}
                  onChange={handleChange}
                />{" "}
                🅿️ Parking
              </label>
              <label>
                <input
                  type="checkbox"
                  name="hasElevator"
                  checked={form.hasElevator}
                  onChange={handleChange}
                />{" "}
                🛗 Elevator
              </label>
              <label>
                <input
                  type="checkbox"
                  name="isFurnished"
                  checked={form.isFurnished}
                  onChange={handleChange}
                />{" "}
                🛋️ Furnished
              </label>
            </div>
          </div>
          <div className="form-group">
            <label>Images (optional)</label>
            {form.images.map((img, idx) => (
              <div
                key={idx}
                style={{ display: "flex", gap: "8px", marginBottom: "0.5rem" }}
              >
                <input
                  type="url"
                  placeholder="https://example.com/image.jpg"
                  value={img}
                  onChange={(e) => handleImageChange(idx, e.target.value)}
                  style={{ flex: 1 }}
                />
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => removeImage(idx)}
                >
                  Remove
                </button>
              </div>
            ))}
            <button
              type="button"
              className="btn btn-secondary"
              onClick={addImage}
            >
              Add Image URL
            </button>
            <p className="form-hint">
              Provide direct image URLs (jpg/png). At least one image will be
              shown in listings.
            </p>
          </div>
          <button
            type="submit"
            className="btn btn-primary btn-full"
            disabled={loading}
          >
            {loading
              ? "Saving..."
              : isEditing
                ? "Save Changes"
                : "Create Listing"}
          </button>
          <button
            type="button"
            className="btn btn-outline btn-full"
            style={{ marginTop: "0.5rem" }}
            onClick={() => navigate(-1)}
          >
            Cancel
          </button>
        </form>
      </div>
    </div>
  );
}
