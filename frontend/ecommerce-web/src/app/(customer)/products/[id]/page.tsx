"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { getProductById } from "@/lib/api/products";
import { getInventoryByProduct } from "@/lib/api/inventory";
import { useAuth } from "@/lib/auth-context";
import type { ProductResponse, InventoryItemResponse } from "@/types";

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const { user } = useAuth();
  const [product, setProduct] = useState<ProductResponse | null>(null);
  const [inventory, setInventory] = useState<InventoryItemResponse | null>(null);
  const [qty, setQty] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getProductById(Number(id))
      .then(setProduct)
      .catch(() => router.push("/products"))
      .finally(() => setLoading(false));
    getInventoryByProduct(Number(id))
      .then(setInventory)
      .catch(() => setInventory(null));
  }, [id, router]);

  const addToCart = () => {
    const existing = JSON.parse(localStorage.getItem("cart") ?? "[]") as {
      productId: number;
      productName: string;
      price: number;
      quantity: number;
    }[];
    const idx = existing.findIndex((i) => i.productId === product!.id);
    if (idx >= 0) {
      existing[idx].quantity += qty;
    } else {
      existing.push({ productId: product!.id, productName: product!.name, price: product!.price, quantity: qty });
    }
    localStorage.setItem("cart", JSON.stringify(existing));
    router.push("/cart");
  };

  if (loading) return <div className="text-center py-5"><div className="spinner-border" /></div>;
  if (!product) return null;

  const inStock = inventory ? inventory.quantity > 0 : null;

  return (
    <div className="row">
      <div className="col-md-8 offset-md-2">
        <nav aria-label="breadcrumb">
          <ol className="breadcrumb">
            <li className="breadcrumb-item"><a href="/products">Products</a></li>
            <li className="breadcrumb-item active">{product.name}</li>
          </ol>
        </nav>
        <div className="card shadow p-4">
          <span className="badge bg-secondary mb-2 w-auto" style={{ width: "fit-content" }}>{product.categoryName}</span>
          <h2>{product.name}</h2>
          <p className="text-muted">{product.description ?? "No description available."}</p>
          <h3 className="text-success">${product.price.toFixed(2)}</h3>
          {inStock !== null && (
            <p className={inStock ? "text-success" : "text-danger"}>
              {inStock ? `In stock (${inventory!.quantity} units)` : "Out of stock"}
            </p>
          )}
          <div className="d-flex align-items-center gap-3 mt-3">
            <input
              type="number"
              min={1}
              max={inventory?.quantity ?? 99}
              value={qty}
              onChange={(e) => setQty(Math.max(1, Number(e.target.value)))}
              className="form-control"
              style={{ width: 90 }}
            />
            <button
              className="btn btn-primary"
              onClick={addToCart}
              disabled={inStock === false}
            >
              {user ? "Add to Cart" : "Login to Buy"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
