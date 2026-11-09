import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateTaxRecordRequest, TaxRecord, UpdateTaxRecordRequest } from '../models/tax-record.model';

@Injectable({
  providedIn: 'root'
})
export class TaxRecordService {
  private readonly baseUrl = '/api/taxrecords';

  constructor(private http: HttpClient) {}

  getAll(): Observable<TaxRecord[]> {
    return this.http.get<TaxRecord[]>(this.baseUrl);
  }

  getById(id: string): Observable<TaxRecord> {
    return this.http.get<TaxRecord>(`${this.baseUrl}/${id}`);
  }

  create(record: CreateTaxRecordRequest): Observable<TaxRecord> {
    return this.http.post<TaxRecord>(this.baseUrl, record);
  }

  update(id: string, record: UpdateTaxRecordRequest): Observable<TaxRecord> {
    return this.http.put<TaxRecord>(`${this.baseUrl}/${id}`, record);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
