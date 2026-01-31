import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider } from "react-router-dom";
import "./index.css";
import { router } from "./router";
import { ToastContainer } from "@/components/ui/toast-container";
import { ToastProvider } from "@/lib/toast";
import { ThemeProvider } from "@/lib/theme";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ThemeProvider>
      <ToastProvider>
        <RouterProvider router={router} />
        <ToastContainer />
      </ToastProvider>
    </ThemeProvider>
  </React.StrictMode>,
);
