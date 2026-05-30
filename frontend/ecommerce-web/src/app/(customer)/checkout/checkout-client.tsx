"use client";

import { useEffect, useState, useMemo } from "react";
import { useRouter } from "next/navigation";
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  PaymentElement,
  useStripe,
  useElements,
} from "@stripe/react-stripe-js";
import { useAuth } from "@/lib/auth-context";
import { createOrder } from "@/lib/api/orders";
import { getPaymentByOrder, confirmPaymentByOrder } from "@/lib/api/payments";
import type { OrderResponse } from "@/types";

function StripePaymentForm({
  order,
  onSuccess,
}: {
  order: OrderResponse;
  onSuccess: () => void;
}) {
  const stripe = useStripe();
  const elements = useElements();
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handlePay = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) return;
    setLoading(true);
    setError("");

    const result = await stripe.confirmPayment({
      elements,
      redirect: "if_required",
    });

    if (result.error) {
      setError(result.error.message ?? "Payment failed");
      setLoading(false);
    } else {
      onSuccess();
    }
  };

  return (
    <form onSubmit={handlePay}>
      <PaymentElement />
      {error && <div className="alert alert-danger mt-3">{error}</div>}
      <button
        type="submit"
        className="btn btn-success w-100 mt-4"
        disabled={!stripe || loading}
      >
        {loading ? "Processing..." : `Pay $${order.total.toFixed(2)}`}
      </button>
    </form>
  );
}

interface CartItem {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
}

export default function CheckoutClient({ stripeKey }: { stripeKey: string }) {
  const { user, isLoading } = useAuth();
  const router = useRouter();
  const stripePromise = useMemo(
    () => (stripeKey ? loadStripe(stripeKey) : null),
    [stripeKey]
  );
  const [items, setItems] = useState<CartItem[]>([]);
  const [order, setOrder] = useState<OrderResponse | null>(null);
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (isLoading) return;
    if (!user) { router.push("/login"); return; }
    const cart: CartItem[] = JSON.parse(localStorage.getItem("cart") ?? "[]");
    if (cart.length === 0) { router.push("/cart"); return; }
    setItems(cart);
  }, [user, isLoading, router]);

  const total = items.reduce((acc, i) => acc + i.price * i.quantity, 0);

  const handleCreateOrder = async () => {
    if (!user) return;
    setError("");
    setCreating(true);
    try {
      const newOrder = await createOrder({
        userId: user.userId,
        items: items.map((i) => ({
          productId: i.productId,
          productName: i.productName,
          quantity: i.quantity,
          unitPrice: i.price,
          discount: 0,
        })),
      });

      // Payment is auto-created by RabbitMQ consumer — poll until it appears
      let payment = null;
      for (let attempt = 0; attempt < 8; attempt++) {
        await new Promise((r) => setTimeout(r, 1000));
        try {
          payment = await getPaymentByOrder(newOrder.id);
          if (payment?.clientSecret) break;
        } catch {
          // not ready yet, keep polling
        }
      }

      if (!payment?.clientSecret) {
        throw new Error("Payment could not be initialized. Please try again.");
      }

      setOrder(newOrder);
      setClientSecret(payment.clientSecret);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to create order");
    } finally {
      setCreating(false);
    }
  };

  const handleSuccess = async () => {
    if (order) {
      try {
        await confirmPaymentByOrder(order.id);
      } catch {
        // best-effort — order status update via RabbitMQ is the source of truth
      }
    }
    localStorage.removeItem("cart");
    router.push("/orders");
  };

  if (!order || !clientSecret) {
    return (
      <>
        <h2 className="mb-4">Checkout</h2>
        {error && <div className="alert alert-danger">{error}</div>}
        <div className="row">
          <div className="col-md-7">
            <div className="card mb-4">
              <div className="card-header fw-bold">Order Summary</div>
              <ul className="list-group list-group-flush">
                {items.map((item) => (
                  <li key={item.productId} className="list-group-item d-flex justify-content-between">
                    <span>{item.productName} x {item.quantity}</span>
                    <span>${(item.price * item.quantity).toFixed(2)}</span>
                  </li>
                ))}
                <li className="list-group-item d-flex justify-content-between fw-bold">
                  <span>Total</span>
                  <span>${total.toFixed(2)}</span>
                </li>
              </ul>
            </div>
          </div>
          <div className="col-md-5">
            <div className="card">
              <div className="card-body text-center">
                <p className="text-muted mb-4">Click below to place your order and enter payment details.</p>
                <button className="btn btn-primary btn-lg w-100" onClick={handleCreateOrder} disabled={creating}>
                  {creating ? "Preparing..." : `Proceed to Payment - $${total.toFixed(2)}`}
                </button>
              </div>
            </div>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <h2 className="mb-4">Payment</h2>
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card shadow p-4">
            <h5 className="mb-1">Order #{order.id}</h5>
            <p className="text-muted mb-4">Total: <strong>${order.total.toFixed(2)}</strong></p>
            <Elements stripe={stripePromise} options={{ clientSecret }}>
              <StripePaymentForm order={order} onSuccess={handleSuccess} />
            </Elements>
          </div>
        </div>
      </div>
    </>
  );
}
