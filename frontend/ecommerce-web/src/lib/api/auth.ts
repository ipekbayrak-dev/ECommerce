import { apiFetch } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfileResponse, ChangeRoleRequest } from "@/types";

export function login(data: LoginRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function register(data: RegisterRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify(data)
  })
}

export function getProfile(): Promise<UserProfileResponse> {
  return apiFetch<UserProfileResponse>("/api/auth/me")
}

export function changeRole( userId:number, data: ChangeRoleRequest): Promise<void> {
  return apiFetch<void>(`/api/auth/${userId}`, {
    method: "PATCH",
    body: JSON.stringify(data)
  })
}