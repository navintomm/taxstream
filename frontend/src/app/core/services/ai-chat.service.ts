import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AiChatRequest, AiChatResponse, AiHistoryResponse } from '../models/ai-chat.model';

@Injectable({
  providedIn: 'root'
})
export class AiChatService {
  private readonly baseUrl = '/api/ai';

  constructor(private http: HttpClient) {}

  ask(prompt: string): Observable<AiChatResponse> {
    const payload: AiChatRequest = { prompt };
    return this.http.post<AiChatResponse>(`${this.baseUrl}/chat`, payload);
  }

  getHistory(pageNumber: number = 1, pageSize: number = 20): Observable<AiHistoryResponse> {
    return this.http.get<AiHistoryResponse>(`${this.baseUrl}/history?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
}
