import { apiFetch } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfileResponse, ChangeRoleRequest } from "@/types";

export function login(data: LoginRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/auth/login", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

