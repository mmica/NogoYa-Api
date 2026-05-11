import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ImportMode, ProductImportResult } from '@core/models/product-import.model';

export interface ImportProgress {
  progress: number;
  result?: ProductImportResult;
}

@Injectable({ providedIn: 'root' })
export class ProductImportService {
  private readonly http = inject(HttpClient);

  upload(storeId: string, file: File, mode: ImportMode = 'Upsert'): Observable<ImportProgress> {
    const form = new FormData();
    form.append('storeId', storeId);
    form.append('file', file);
    form.append('mode', mode);
    return this.http
      .post<ProductImportResult>('products/import', form, { reportProgress: true, observe: 'events' })
      .pipe(map(evt => this.toProgress(evt)));
  }

  private toProgress(evt: HttpEvent<ProductImportResult>): ImportProgress {
    switch (evt.type) {
      case HttpEventType.UploadProgress:
        return { progress: evt.total ? Math.round((evt.loaded / evt.total) * 100) : 0 };
      case HttpEventType.Response:
        return { progress: 100, result: evt.body ?? undefined };
      default:
        return { progress: 0 };
    }
  }
}
