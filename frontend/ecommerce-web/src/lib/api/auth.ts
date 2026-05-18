import { apiFetch } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfileResponse, ChangeRoleRequest } from "@/types";

export function login(data: LoginRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/auth/login", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function register(data: RegisterRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/auth/register", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export function getProfile(userId: number): Promise<UserProfileResponse> {
  return apiFetch<UserProfileResponse>(`/auth/profile/${userId}`);
}

export function changeRole(userId: number, data: ChangeRoleRequest): Promise<void> {
  return apiFetch<void>(`/auth/role/${userId}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
}
