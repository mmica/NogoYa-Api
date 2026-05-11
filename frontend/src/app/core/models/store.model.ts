export interface Store {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  logoUrl?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  isActive: boolean;
  productsCount: number;
}

export interface CreateStorePayload {
  name: string;
  slug: string;
  description?: string | null;
  logoUrl?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
}

export interface UpdateStorePayload {
  name: string;
  description?: string | null;
  logoUrl?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  isActive: boolean;
}

export interface StoreFilter {
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}
