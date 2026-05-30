"use client";

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";

export default function Navbar() {
  const { user, logout } = useAuth();
  const router = useRouter();
  const [open, setOpen] = useState(false);

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container">
        <Link className="navbar-brand fw-bold" href="/products">
          🛍️ ECommerce
        </Link>
        <button
          className="navbar-toggler"
          type="button"
          aria-expanded={open}
          aria-label="Toggle navigation"
          onClick={() => setOpen((prev) => !prev)}
        >
          <span className="navbar-toggler-icon" />
        </button>
        <div className={`collapse navbar-collapse${open ? " show" : ""}`} id="navbarNav">
          <ul className="navbar-nav me-auto">
            <li className="nav-item">
              <Link className="nav-link" href="/products" onClick={() => setOpen(false)}>Products</Link>
            </li>
            {user && (
              <>
                <li className="nav-item">
                  <Link className="nav-link" href="/cart" onClick={() => setOpen(false)}>Cart</Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" href="/orders" onClick={() => setOpen(false)}>My Orders</Link>
                </li>
              </>
            )}
          </ul>
          <ul className="navbar-nav ms-auto">
            {user ? (
              <>
                {user.role === "Admin" && (
                  <li className="nav-item">
                    <Link className="nav-link text-warning" href="/admin/dashboard" onClick={() => setOpen(false)}>Admin Panel</Link>
                  </li>
                )}
                <li className="nav-item">
                  <span className="nav-link text-light">Hi, {user.username}</span>
                </li>
                <li className="nav-item">
                  <button className="btn btn-outline-light btn-sm mt-1" onClick={handleLogout}>
                    Logout
                  </button>
                </li>
              </>
            ) : (
              <>
                <li className="nav-item">
                  <Link className="nav-link" href="/login" onClick={() => setOpen(false)}>Login</Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" href="/register" onClick={() => setOpen(false)}>Register</Link>
                </li>
              </>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
}
