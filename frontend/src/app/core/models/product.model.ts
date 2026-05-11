export interface Product {
  id: string;
  storeId: string;
  storeName: string;
  name: string;
  description?: string | null;
  imageUrl?: string | null;
  sku?: string | null;
  price: number;
  discountPercent: number;
  effectivePrice: number;
  stock: number;
  isAvailable: boolean;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface ProductFilter {
  storeId?: string;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  onSale?: boolean;
  page?: number;
  pageSize?: number;
}
