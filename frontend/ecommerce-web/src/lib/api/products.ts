import { apiFetch } from "./client";
import type { ProductResponse, CreateProductRequest, UpdateProductRequest, CategoryResponse } from "@/types";

export function getProducts(params?: {
  search?: string;
  categoryId?: number;
  page?: number;
  pageSize?: number;
}): Promise<ProductResponse[]> {
  const query = new URLSearchParams();
  if (params?.search) query.set("search", params.search);
  if (params?.categoryId) query.set("categoryId", String(params.categoryId));
  if (params?.page) query.set("page", String(params.page));
  if (params?.pageSize) query.set("pageSize", String(params.pageSize));
  const qs = query.toString();
  return apiFetch<ProductResponse[]>(`/products${qs ? `?${qs}` : ""}`);
}

export function getProductById(id: number): Promise<ProductResponse> {
  return apiFetch<ProductResponse>(`/products/${id}`);
}

export function createProduct(data: CreateProductRequest): Promise<ProductResponse> {
  return apiFetch<ProductResponse>("/products", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function updateProduct(id: number, data: UpdateProductRequest): Promise<ProductResponse> {
  return apiFetch<ProductResponse>(`/products/${id}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}

export function deleteProduct(id: number): Promise<void> {
  return apiFetch<void>(`/products/${id}`, { method: "DELETE" });
}

export function getCategories(): Promise<CategoryResponse[]> {
  return apiFetch<CategoryResponse[]>("/products/categories");
}
