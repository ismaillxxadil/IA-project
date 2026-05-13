import React, {
  createContext,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import * as signalR from "@microsoft/signalr";
import toast from "react-hot-toast";
import { useAuth } from "./AuthContext";
import { buildApiUrl } from "../api/config";

const NotificationContext = createContext(null);

export function NotificationProvider({ children }) {
  const { token } = useAuth();
  const connectionRef = useRef(null);
  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    if (!token) {
      // If logged out, stop the connection
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
      return;
    }

    // Build SignalR connection with JWT via query string
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(buildApiUrl(`/notifications?access_token=${token}`))
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveNotification", (message) => {
      toast(message, { icon: "🔔", duration: 5000 });
      setNotifications((prev) => [
        { message, time: new Date() },
        ...prev.slice(0, 19),
      ]);
    });

    connection
      .start()
      .catch((err) => console.error("SignalR connection failed:", err));

    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [token]);

  const clearNotifications = () => setNotifications([]);

  return (
    <NotificationContext.Provider value={{ notifications, clearNotifications }}>
      {children}
    </NotificationContext.Provider>
  );
}

export const useNotifications = () => useContext(NotificationContext);
