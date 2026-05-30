"use client";

import { useEffect, useState } from "react";
import { getProducts } from "@/lib/api/products";
import { getOrders } from "@/lib/api/orders";

export default function AdminDashboardPage() {
  const [productCount, setProductCount] = useState<number | null>(null);
  const [orderCount, setOrderCount] = useState<number | null>(null);
  const [pendingOrders, setPendingOrders] = useState<number | null>(null);

  useEffect(() => {
    getProducts({ page: 1, pageSize: 1000 }).then((data) => setProductCount(data.length)).catch(console.error);
    getOrders().then((data) => {
      setOrderCount(data.length);
      setPendingOrders(data.filter((o) => o.orderStatus === "Pending").length);
    }).catch(console.error);
  }, []);

  const cards = [
    { label: "Total Products", value: productCount, color: "primary" },
    { label: "Total Orders", value: orderCount, color: "info" },
    { label: "Pending Orders", value: pendingOrders, color: "warning" },
  ];

  return (
    <>
      <h2 className="mb-4">Dashboard</h2>
      <div className="row g-4">
        {cards.map((card) => (
          <div className="col-md-4" key={card.label}>
            <div className={`card text-white bg-${card.color} shadow`}>
              <div className="card-body">
                <h6 className="card-title text-uppercase">{card.label}</h6>
                <h2 className="card-text">
                  {card.value === null ? <span className="spinner-border spinner-border-sm" /> : card.value}
                </h2>
              </div>
            </div>
          </div>
        ))}
      </div>
    </>
  );
}
