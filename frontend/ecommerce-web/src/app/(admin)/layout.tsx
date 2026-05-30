"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import { useAuth } from "@/lib/auth-context";

const NAV_LINKS = [
  { href: "/admin/dashboard", label: "Dashboard" },
  { href: "/admin/products", label: "Products" },
  { href: "/admin/orders", label: "Orders" },
  { href: "/admin/users", label: "Users" },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { user, isLoading } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (isLoading) return;
    if (!user || user.role !== "Admin") router.replace("/products");
  }, [user, isLoading, router]);

  if (isLoading || !user) return null;

  return (
    <div className="d-flex min-vh-100">
      {/* Sidebar */}
      <div className="admin-sidebar text-white d-flex flex-column p-3" style={{ width: 220, minWidth: 220 }}>
        <h5 className="text-center mb-4 pt-2">⚙️ Admin</h5>
        <nav className="nav flex-column gap-1">
          {NAV_LINKS.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={`nav-link text-white rounded px-3 py-2 ${pathname === link.href ? "bg-secondary" : ""}`}
            >
              {link.label}
            </Link>
          ))}
        </nav>
        <div className="mt-auto pt-3 border-top border-secondary d-flex flex-column gap-2">
          <Link href="/products" className="btn btn-outline-light btn-sm w-100">
            ← Back to Store
          </Link>
          <small className="text-secondary">{user.username}</small>
        </div>
      </div>

      {/* Main content */}
      <main className="flex-grow-1 p-4 bg-light">
        {children}
      </main>
    </div>
  );
}
