import { apiFetch } from "./client";
import type { PaymentResponse, CreatePaymentRequest } from "@/types";

export function getPaymentById(id: number): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>(`/payments/${id}`);
}

export function getPaymentByOrder(orderId: number): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>(`/payments/order/${orderId}`);
}

export function getPaymentsByUser(userId: number): Promise<PaymentResponse[]> {
  return apiFetch<PaymentResponse[]>(`/payments/user/${userId}`);
}

export function createPayment(data: CreatePaymentRequest): Promise<PaymentResponse> {
  return apiFetch<PaymentResponse>("/payments", {
    method: "POST",
    body: JSON.stringify(data),
  });
}
