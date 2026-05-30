"use client";

import { createContext, useContext, useEffect, useState } from "react";
import { saveToken, clearToken } from "@/lib/api/client";
import { login as apiLogin, register as apiRegister } from "@/lib/api/auth";
import type { AuthResponse, LoginRequest, RegisterRequest } from "@/types";

interface AuthUser {
  userId: number;
  username: string;
  role: string;
  token: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Rehydrate from localStorage on mount
  useEffect(() => {
    const token = localStorage.getItem("token");
    const stored = localStorage.getItem("authUser");
    if (token && stored) {
      setUser(JSON.parse(stored));
    }
    setIsLoading(false);
  }, []);

  const login = async (data: LoginRequest) => {
    const res: AuthResponse = await apiLogin(data);
    const authUser: AuthUser = {
      userId: res.userId,
      username: res.username,
      role: res.role,
      token: res.token!,
    };
    saveToken(res.token!);
    localStorage.setItem("authUser", JSON.stringify(authUser));
    setUser(authUser);
  };

  const register = async (data: RegisterRequest) => {
    const res: AuthResponse = await apiRegister(data);
    const authUser: AuthUser = {
      userId: res.userId,
      username: res.username,
      role: res.role,
      token: res.token!,
    };
    saveToken(res.token!);
    localStorage.setItem("authUser", JSON.stringify(authUser));
    setUser(authUser);
  };

  const logout = () => {
    clearToken();
    localStorage.removeItem("authUser");
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}
