import { apiFetch } from "./client";
import type { OrderResponse, CreateOrderRequest, UpdateOrderRequest } from "@/types";

export function getOrders(): Promise<OrderResponse[]> {
  return apiFetch<OrderResponse[]>("/api/orders");
}

export function getOrderById(id: number): Promise<OrderResponse> {
  return apiFetch<OrderResponse>(`/api/orders/${id}`);
}

export function getOrdersByUser(userId: number): Promise<OrderResponse[]> {
  return apiFetch<OrderResponse[]>(`/api/orders/user/${userId}`);
}

export function createOrder(data: CreateOrderRequest): Promise<OrderResponse> {
  return apiFetch<OrderResponse>("/api/orders", { method: "POST", body: JSON.stringify(data) });
}

export function updateOrderStatus(id: number, data: UpdateOrderRequest): Promise<OrderResponse> {
  return apiFetch<OrderResponse>(`/api/orders/${id}/status`, { method: "PUT", body: JSON.stringify(data) });
}

export function cancelOrder(id: number): Promise<void> {
  return apiFetch<void>(`/api/orders/${id}/cancel`, { method: "PUT" });
}