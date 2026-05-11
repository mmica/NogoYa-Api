export type ImportMode = 'InsertOnly' | 'Upsert';

export interface ProductImportError {
  rowNumber: number;
  sku?: string | null;
  message: string;
}

export interface ProductImportResult {
  totalRows: number;
  imported: number;
  updated: number;
  skipped: number;
  failed: number;
  errors: ProductImportError[];
}
