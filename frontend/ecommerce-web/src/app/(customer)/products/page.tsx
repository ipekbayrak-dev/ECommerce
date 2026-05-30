"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { getProducts, getCategories } from "@/lib/api/products";
import type { ProductResponse, CategoryResponse } from "@/types";

export default function ProductsPage() {
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState<number | undefined>();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCategories().then(setCategories).catch(console.error);
  }, []);

  useEffect(() => {
    setLoading(true);
    const timer = setTimeout(() => {
      getProducts({ search: search || undefined, categoryId, page: 1, pageSize: 20 })
        .then(setProducts)
        .catch(console.error)
        .finally(() => setLoading(false));
    }, 300);
    return () => clearTimeout(timer);
  }, [search, categoryId]);

  return (
    <>
      <h2 className="mb-4">Products</h2>

      {/* Filters */}
      <div className="row g-2 mb-4">
        <div className="col-md-6">
          <input
            type="text"
            className="form-control"
            placeholder="Search products…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <div className="col-md-4">
          <select
            className="form-select"
            value={categoryId ?? ""}
            onChange={(e) => setCategoryId(e.target.value ? Number(e.target.value) : undefined)}
          >
            <option value="">All Categories</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border" role="status" />
        </div>
      ) : products.length === 0 ? (
        <p className="text-muted">No products found.</p>
      ) : (
        <div className="row row-cols-1 row-cols-md-3 g-4">
          {products.map((p) => (
            <div className="col" key={p.id}>
              <div className="card h-100 shadow-sm">
                <div className="card-body">
                  <span className="badge bg-secondary mb-2">{p.categoryName}</span>
                  <h5 className="card-title">{p.name}</h5>
                  <p className="card-text text-muted small">{p.description ?? "No description"}</p>
                </div>
                <div className="card-footer d-flex justify-content-between align-items-center">
                  <span className="fw-bold text-success">${p.price.toFixed(2)}</span>
                  <Link href={`/products/${p.id}`} className="btn btn-sm btn-outline-primary">
                    View
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </>
  );
}
