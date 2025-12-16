import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoansPageComponent } from './loans-page.component';
import { LoansTableComponent } from './components/loans-table/loans-table.component';

@Component({
  selector: 'app-loans-table',
  standalone: true,
  template: '',
})
class LoansTableStubComponent {}

describe('LoansPageComponent', () => {
  let fixture: ComponentFixture<LoansPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoansPageComponent],
    })
      .overrideComponent(LoansPageComponent, {
        remove: { imports: [LoansTableComponent] },
        add: { imports: [LoansTableStubComponent] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(LoansPageComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render heading and loans table', () => {
    expect(fixture.nativeElement.textContent).toContain('Loan Management');
    expect(
      fixture.nativeElement.querySelector('app-loans-table'),
    ).not.toBeNull();
  });
});
