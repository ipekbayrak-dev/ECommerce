"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { getOrdersByUser } from "@/lib/api/orders";
import { getPaymentByOrder } from "@/lib/api/payments";
import type { OrderResponse, PaymentResponse } from "@/types";

const STATUS_BADGE: Record<string, string> = {
  Pending: "warning",
  Paid: "info",
  Shipped: "primary",
  Delivered: "success",
  Cancelled: "danger",
};

const PAYMENT_BADGE: Record<string, string> = {
  Pending: "warning",
  Processing: "info",
  Completed: "success",
  Failed: "danger",
  Declined: "danger",
  Cancelled: "secondary",
  Refunded: "dark",
};

export default function OrdersPage() {
  const { user, isLoading } = useAuth();
  const router = useRouter();
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [payments, setPayments] = useState<Record<number, PaymentResponse>>({});
  const [loading, setLoading] = useState(true);
  const [expanded, setExpanded] = useState<number | null>(null);

  useEffect(() => {
    if (isLoading) return;
    if (!user) { router.push("/login"); return; }
    getOrdersByUser(user.userId)
      .then(async (data) => {
        setOrders(data);
        // Fetch payment status for each order
        const map: Record<number, PaymentResponse> = {};
        await Promise.allSettled(
          data.map(async (o) => {
            try {
              const p = await getPaymentByOrder(o.id);
              map[o.id] = p;
            } catch {
              // No payment yet — ignore
            }
          })
        );
        setPayments(map);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [user, isLoading, router]);

  if (loading) return <div className="text-center py-5"><div className="spinner-border" /></div>;

  return (
    <>
      <h2 className="mb-4">My Orders</h2>
      {orders.length === 0 ? (
        <p className="text-muted">You haven&apos;t placed any orders yet.</p>
      ) : (
        <div className="accordion" id="ordersAccordion">
          {orders.map((order) => (
            <div className="accordion-item" key={order.id}>
              <h2 className="accordion-header">
                <button
                  className="accordion-button collapsed"
                  type="button"
                  onClick={() => setExpanded(expanded === order.id ? null : order.id)}
                >
                  <span className="me-3">Order #{order.id}</span>
                  <span className={`badge bg-${STATUS_BADGE[order.orderStatus] ?? "secondary"} me-2`}>
                    {order.orderStatus}
                  </span>
                  <span className="text-muted small me-3">{new Date(order.date).toLocaleDateString()}</span>
                  <span className="fw-bold text-success">${order.total.toFixed(2)}</span>
                  {payments[order.id] && (
                    <span className={`badge bg-${PAYMENT_BADGE[payments[order.id].status] ?? "secondary"} ms-2`}>
                      Payment: {payments[order.id].status}
                    </span>
                  )}
                </button>
              </h2>
              {expanded === order.id && (
                <div className="accordion-collapse">
                  <div className="accordion-body">
                    <table className="table table-sm mb-0">
                      <thead>
                        <tr>
                          <th>Product</th>
                          <th>Unit Price</th>
                          <th>Qty</th>
                          <th>Discount</th>
                          <th>Subtotal</th>
                        </tr>
                      </thead>
                      <tbody>
                        {order.items.map((item) => (
                          <tr key={item.productId}>
                            <td>{item.productName}</td>
                            <td>${item.unitPrice.toFixed(2)}</td>
                            <td>{item.quantity}</td>
                            <td>{item.discount > 0 ? `${item.discount}%` : "—"}</td>
                            <td>${(item.unitPrice * item.quantity * (1 - item.discount / 100)).toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </>
  );
}
