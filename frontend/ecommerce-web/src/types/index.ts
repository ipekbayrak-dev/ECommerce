// ─── Auth ────────────────────────────────────────────────────────────────────

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  username: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  userId: number;
  username: string;
  role: string;
}

export interface UserProfileResponse {
  userId: number;
  username: string;
  email: string;
  role: string;
}

export interface ChangeRoleRequest {
  role: string;
}

// ─── Products ────────────────────────────────────────────────────────────────

export interface ProductResponse {
  id: number;
  name: string;
  description: string | null;
  price: number;
  categoryId: number;
  categoryName: string;
  createdAtUtc: string;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  price: number;
  categoryId: number;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  price?: number;
  categoryId?: number;
}

export interface CategoryResponse {
  id: number;
  name: string;
}

// ─── Orders ──────────────────────────────────────────────────────────────────

export type OrderStatus = "Pending" | "Paid" | "Shipped" | "Delivered" | "Cancelled";

export interface OrderItemResponse {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  discount: number;
}

export interface OrderResponse {
  id: number;
  userId: number;
  date: string;
  orderStatus: OrderStatus;
  total: number;
  items: OrderItemResponse[];
}

export interface CreateOrderItemRequest {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  discount: number;
}

export interface CreateOrderRequest {
  userId: number;
  items: CreateOrderItemRequest[];
}

export interface UpdateOrderRequest {
  orderStatus: OrderStatus;
}

// ─── Payments ────────────────────────────────────────────────────────────────

export type PaymentStatus = "Pending" | "Succeeded" | "Failed" | "Refunded";

export interface PaymentResponse {
  id: number;
  userId: number;
  orderId: number;
  stripePaymentIntentId: string | null;
  clientSecret: string | null;
  amount: number;
  date: string;
  method: string;
  status: PaymentStatus;
  failureReason: string | null;
}

export interface CreatePaymentRequest {
  userId: number;
  orderId: number;
  amount: number;
  method: string;
  paymentMethodId?: string;
}

// ─── Inventory ───────────────────────────────────────────────────────────────

export interface InventoryItemResponse {
  id: number;
  productId: number;
  quantity: number;
  lastUpdatedUtc: string;
}

export interface AdjustStockRequest {
  delta: number;
}

export interface SeedInventoryRequest {
  initialQuantity: number;
}

// ─── Shared ──────────────────────────────────────────────────────────────────

export interface ApiErrorResponse {
  message: string;
  correlationId: string;
  timestampUtc: string;
}