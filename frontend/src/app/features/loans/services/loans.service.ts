import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env';
import { Loan } from '../models/loan';

@Injectable({ providedIn: 'root' })
export class LoansService {
  private readonly apiUrl = `${environment.apiBaseUrl}/loans`;
  private readonly http = inject(HttpClient);

  listLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(this.apiUrl);
  }
}
