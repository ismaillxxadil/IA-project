const trimTrailingSlash = (value) => value.replace(/\/$/, "");

export const API_BASE_URL = trimTrailingSlash(
  process.env.REACT_APP_API_BASE_URL || "http://localhost:5184",
);

export const buildApiUrl = (path) => {
  if (!path) return API_BASE_URL;
  return `${API_BASE_URL}${path.startsWith("/") ? "" : "/"}${path}`;
};
