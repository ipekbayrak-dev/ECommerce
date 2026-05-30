"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";

interface CartItem {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
}

export default function CartPage() {
  const [items, setItems] = useState<CartItem[]>([]);
  const router = useRouter();

  useEffect(() => {
    setItems(JSON.parse(localStorage.getItem("cart") ?? "[]"));
  }, []);

  const updateQty = (productId: number, qty: number) => {
    const updated = qty <= 0
      ? items.filter((i) => i.productId !== productId)
      : items.map((i) => i.productId === productId ? { ...i, quantity: qty } : i);
    setItems(updated);
    localStorage.setItem("cart", JSON.stringify(updated));
  };

  const remove = (productId: number) => updateQty(productId, 0);

  const total = items.reduce((acc, i) => acc + i.price * i.quantity, 0);

  return (
    <>
      <h2 className="mb-4">Your Cart</h2>
      {items.length === 0 ? (
        <div className="text-center py-5">
          <p className="text-muted">Your cart is empty.</p>
          <Link href="/products" className="btn btn-primary">Browse Products</Link>
        </div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="table align-middle">
              <thead className="table-light">
                <tr>
                  <th>Product</th>
                  <th>Price</th>
                  <th>Qty</th>
                  <th>Subtotal</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.productId}>
                    <td>{item.productName}</td>
                    <td>${item.price.toFixed(2)}</td>
                    <td>
                      <input
                        type="number"
                        min={1}
                        value={item.quantity}
                        onChange={(e) => updateQty(item.productId, Number(e.target.value))}
                        className="form-control"
                        style={{ width: 70 }}
                      />
                    </td>
                    <td>${(item.price * item.quantity).toFixed(2)}</td>
                    <td>
                      <button className="btn btn-sm btn-outline-danger" onClick={() => remove(item.productId)}>
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="d-flex justify-content-end align-items-center gap-4 mt-3">
            <span className="fs-5 fw-bold">Total: ${total.toFixed(2)}</span>
            <button className="btn btn-primary btn-lg" onClick={() => router.push("/checkout")}>
              Proceed to Checkout
            </button>
          </div>
        </>
      )}
    </>
  );
}
