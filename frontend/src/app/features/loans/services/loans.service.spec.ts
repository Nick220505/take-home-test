import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { environment } from '@env';
import { Loan } from '../models/loan';
import { LoansService } from './loans.service';

describe('LoansService', () => {
  let httpTesting: HttpTestingController;
  let service: LoansService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    httpTesting = TestBed.inject(HttpTestingController);
    service = TestBed.inject(LoansService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('listLoans() should GET /loans', async () => {
    const mockLoans: Loan[] = [
      {
        id: '1',
        amount: 1500,
        currentBalance: 500,
        applicantName: 'Maria Silva',
        status: 'active',
      },
    ];

    const promise = firstValueFrom(service.listLoans());

    const req = httpTesting.expectOne(`${environment.apiBaseUrl}/loans`);
    expect(req.request.method).toBe('GET');

    req.flush(mockLoans);

    expect(await promise).toEqual(mockLoans);
  });
});
