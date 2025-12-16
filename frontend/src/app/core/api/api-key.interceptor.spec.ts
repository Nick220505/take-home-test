import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '@env';
import { apiKeyInterceptor } from './api-key.interceptor';

describe('apiKeyInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  const originalKey = environment.apiKey;

  afterEach(() => {
    environment.apiKey = originalKey;
  });

  it('should add X-Api-Key header when apiKey is set', () => {
    environment.apiKey = 'some-test-key';

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiKeyInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);

    httpClient.get('/test').subscribe();

    const req = httpTesting.expectOne('/test');
    expect(req.request.headers.get('X-Api-Key')).toBe('some-test-key');

    req.flush({});
    httpTesting.verify();
  });

  it('should not add X-Api-Key header when apiKey is empty', () => {
    environment.apiKey = '';

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiKeyInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);

    httpClient.get('/test').subscribe();

    const req = httpTesting.expectOne('/test');
    expect(req.request.headers.has('X-Api-Key')).toBeFalse();

    req.flush({});
    httpTesting.verify();
  });
});
