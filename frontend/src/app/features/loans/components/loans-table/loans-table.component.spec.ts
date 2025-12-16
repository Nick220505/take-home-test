import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';
import { of, throwError } from 'rxjs';
import { MatSort } from '@angular/material/sort';
import { Loan } from '../../models/loan';
import { LoansService } from '../../services/loans.service';
import { LoansTableComponent } from './loans-table.component';

describe('LoansTableComponent', () => {
  let fixture: ComponentFixture<LoansTableComponent>;
  let component: LoansTableComponent;
  let loansServiceSpy: jasmine.SpyObj<LoansService>;

  beforeEach(async () => {
    loansServiceSpy = jasmine.createSpyObj<LoansService>('LoansService', [
      'listLoans',
    ]);

    await TestBed.configureTestingModule({
      imports: [LoansTableComponent, NoopAnimationsModule],
      providers: [{ provide: LoansService, useValue: loansServiceSpy }],
    }).compileComponents();
  });

  it('should create', () => {
    loansServiceSpy.listLoans.and.returnValue(of([]));

    fixture = TestBed.createComponent(LoansTableComponent);
    component = fixture.componentInstance;

    expect(component).toBeTruthy();
  });

  it('should load loans on init and render a table', () => {
    const mockLoans: Loan[] = [
      {
        id: '1',
        amount: 1500,
        currentBalance: 500,
        applicantName: 'Maria Silva',
        status: 'active',
      },
    ];

    loansServiceSpy.listLoans.and.returnValue(of(mockLoans));

    fixture = TestBed.createComponent(LoansTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(loansServiceSpy.listLoans).toHaveBeenCalledTimes(1);
    expect(component.loading()).toBeFalse();
    expect(component.error()).toBeNull();
    expect(component.loans()).toEqual(mockLoans);
    expect(component.dataSource.data).toEqual(mockLoans);

    const table = fixture.nativeElement.querySelector('table');
    expect(table).not.toBeNull();

    const rows = fixture.nativeElement.querySelectorAll('tr[mat-row]');
    expect(rows.length).toBe(1);
  });

  it('should attach MatSort to the data source when available', () => {
    const mockLoans: Loan[] = [
      {
        id: '1',
        amount: 1500,
        currentBalance: 500,
        applicantName: 'Maria Silva',
        status: 'active',
      },
    ];

    loansServiceSpy.listLoans.and.returnValue(of(mockLoans));

    fixture = TestBed.createComponent(LoansTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    TestBed.flushEffects();

    const sortDirective = fixture.debugElement.query(By.directive(MatSort));
    expect(sortDirective).not.toBeNull();
    expect(component.dataSource.sort).toBeTruthy();
  });

  it('should render empty state when there are no loans', () => {
    loansServiceSpy.listLoans.and.returnValue(of([]));

    fixture = TestBed.createComponent(LoansTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.loading()).toBeFalse();
    expect(component.error()).toBeNull();

    expect(fixture.nativeElement.textContent).toContain('No loans found.');
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });

  it('should render an error state when the service errors', () => {
    loansServiceSpy.listLoans.and.returnValue(
      throwError(() => new Error('boom')),
    );

    fixture = TestBed.createComponent(LoansTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.loading()).toBeFalse();
    expect(component.error()).toBe('Failed to load loans. Please try again.');

    expect(fixture.nativeElement.textContent).toContain(
      'Failed to load loans. Please try again.',
    );
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });
});
