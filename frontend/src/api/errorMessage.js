export const getApiErrorMessage = (error, fallbackMessage) => {
  const responseData = error?.response?.data;

  if (typeof responseData === "string" && responseData.trim()) {
    return responseData;
  }

  if (responseData?.message) {
    return responseData.message;
  }

  if (responseData?.title) {
    return responseData.title;
  }

  if (!error?.response) {
    return `${fallbackMessage} Check that the API is running.`;
  }

  return fallbackMessage;
};
