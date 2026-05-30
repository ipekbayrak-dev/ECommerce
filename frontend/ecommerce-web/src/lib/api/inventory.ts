import { apiFetch } from "./client";
import type { InventoryItemResponse, AdjustStockRequest, SeedInventoryRequest } from "@/types";

export function getInventoryByProduct(productId: number): Promise<InventoryItemResponse> {
  return apiFetch<InventoryItemResponse>(`/api/inventory/${productId}`);
}

export function adjustStock(productId: number, data: AdjustStockRequest): Promise<InventoryItemResponse> {
  return apiFetch<InventoryItemResponse>(`/api/inventory/${productId}/adjust`, { method: "PUT", body: JSON.stringify(data) });
}

export function seedInventory(productId: number, data: SeedInventoryRequest): Promise<InventoryItemResponse> {
  return apiFetch<InventoryItemResponse>(`/api/inventory/${productId}/seed`, { method: "POST", body: JSON.stringify(data) });
}