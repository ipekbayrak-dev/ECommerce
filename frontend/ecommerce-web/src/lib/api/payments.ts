import { apiFetch } from "./client";
import type { PaymentResponse, CreatePaymentRequest } from "@/types";

export function getPaymentById(id: number): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>(`/api/payments/${id}`);
}

export function getPaymentByOrder(orderId: number): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>(`/api/payments/order/${orderId}`);
}

export function getPaymentsByUser(userId: number): Promise<PaymentResponse[]> {
  return apiFetch<PaymentResponse[]>(`/api/payments/user/${userId}`);
}

export function createPayment(data: CreatePaymentRequest): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>("/api/payments", { method: "POST", body: JSON.stringify(data) });
}

export function confirmPaymentByOrder(orderId: number): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>(`/api/payments/order/${orderId}/confirm`, { method: "POST" });
}