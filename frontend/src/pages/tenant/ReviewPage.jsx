import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { addReview } from '../../api/reviewsApi';
import toast from 'react-hot-toast';

export default function ReviewPage() {
  const { propertyId } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState({ rating: 5, comment: '' });
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await addReview({ propertyId: parseInt(propertyId), rating: form.rating, comment: form.comment });
      toast.success('Review submitted! Thank you.');
      navigate('/tenant/applications');
    } catch (err) {
      toast.error(err.response?.data || 'Failed to submit review.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <div className="auth-card" style={{ maxWidth: '500px', margin: '2rem auto' }}>
        <h1>⭐ Leave a Review</h1>
        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label>Rating (1-5)</label>
            <div className="star-rating">
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  className={`star-btn ${form.rating >= star ? 'active' : ''}`}
                  onClick={() => setForm({ ...form, rating: star })}
                >
                  ⭐
                </button>
              ))}
            </div>
            <small>{form.rating} / 5 stars</small>
          </div>
          <div className="form-group">
            <label>Comment</label>
            <textarea
              value={form.comment}
              onChange={(e) => setForm({ ...form, comment: e.target.value })}
              placeholder="Share your experience..."
              rows={4}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary btn-full" disabled={loading}>
            {loading ? 'Submitting...' : 'Submit Review'}
          </button>
          <button
            type="button"
            className="btn btn-outline btn-full"
            style={{ marginTop: '0.5rem' }}
            onClick={() => navigate(-1)}
          >
            Cancel
          </button>
        </form>
      </div>
    </div>
  );
}
