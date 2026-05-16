import { MatPaginatorIntl } from '@angular/material/paginator';

/**
 * Spanish translations for Angular Material's MatPaginator.
 * Provided globally in app.config.ts so every paginator in the app shows
 * "Filas por página", "1 – 10 de 50", etc.
 */
export function spanishPaginatorIntl(): MatPaginatorIntl {
  const intl = new MatPaginatorIntl();
  intl.itemsPerPageLabel = 'Artículos por página:';
  intl.nextPageLabel = 'Página siguiente';
  intl.previousPageLabel = 'Página anterior';
  intl.firstPageLabel = 'Primera página';
  intl.lastPageLabel = 'Última página';

  intl.getRangeLabel = (page: number, pageSize: number, length: number): string => {
    if (length === 0 || pageSize === 0) return `0 de ${length}`;
    const total = Math.max(length, 0);
    const start = page * pageSize;
    const end = start < total ? Math.min(start + pageSize, total) : start + pageSize;
    return `${start + 1} – ${end} de ${total}`;
  };

  return intl;
}
