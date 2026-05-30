"use client";

import { useEffect, useState } from "react";
import { getOrders, updateOrderStatus } from "@/lib/api/orders";
import type { OrderResponse, OrderStatus } from "@/types";

const STATUS_BADGE: Record<string, string> = {
  Pending: "warning",
  Paid: "info",
  Shipped: "primary",
  Delivered: "success",
  Cancelled: "danger",
};

const VALID_TRANSITIONS: Record<OrderStatus, OrderStatus[]> = {
  Pending: ["Paid", "Cancelled"],
  Paid: ["Shipped", "Cancelled"],
  Shipped: ["Delivered"],
  Delivered: [],
  Cancelled: [],
};

export default function AdminOrdersPage() {
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState<number | null>(null);

  const load = () =>
    getOrders()
      .then(setOrders)
      .catch(console.error)
      .finally(() => setLoading(false));

  useEffect(() => { load(); }, []);

  const handleStatusChange = async (orderId: number, status: OrderStatus) => {
    setUpdating(orderId);
    try {
      await updateOrderStatus(orderId, { orderStatus: status });
      await load();
    } catch (err) {
      console.error(err);
    } finally {
      setUpdating(null);
    }
  };

  return (
    <>
      <h2 className="mb-4">Orders</h2>
      {loading ? (
        <div className="text-center py-5"><div className="spinner-border" /></div>
      ) : (
        <div className="table-responsive">
          <table className="table table-bordered table-hover align-middle">
            <thead className="table-light">
              <tr>
                <th>ID</th><th>User</th><th>Date</th><th>Total</th><th>Status</th><th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.id}>
                  <td>{o.id}</td>
                  <td>{o.userId}</td>
                  <td>{new Date(o.date).toLocaleDateString()}</td>
                  <td>${o.total.toFixed(2)}</td>
                  <td>
                    <span className={`badge bg-${STATUS_BADGE[o.orderStatus] ?? "secondary"}`}>
                      {o.orderStatus}
                    </span>
                  </td>
                  <td>
                    {VALID_TRANSITIONS[o.orderStatus].map((next) => (
                      <button
                        key={next}
                        className={`btn btn-sm btn-outline-${STATUS_BADGE[next] ?? "secondary"} me-1`}
                        disabled={updating === o.id}
                        onClick={() => handleStatusChange(o.id, next)}
                      >
                        → {next}
                      </button>
                    ))}
                    {VALID_TRANSITIONS[o.orderStatus].length === 0 && (
                      <span className="text-muted small">—</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
