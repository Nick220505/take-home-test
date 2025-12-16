import {
  Component,
  DestroyRef,
  effect,
  OnInit,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { Loan } from '../../models/loan';
import { LoansService } from '../../services/loans.service';

@Component({
  selector: 'app-loans-table',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './loans-table.component.html',
  styleUrls: ['./loans-table.component.scss'],
})
export class LoansTableComponent implements OnInit {
  private readonly loansService = inject(LoansService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dataSource = new MatTableDataSource<Loan>([]);

  readonly matSort = viewChild(MatSort);

  readonly loans = signal<Loan[]>([]);
  readonly loading = signal<boolean>(true);
  readonly error = signal<string | null>(null);

  constructor() {
    effect(() => {
      const sort = this.matSort();
      if (sort) {
        this.dataSource.sort = sort;
      }
    });
  }

  ngOnInit(): void {
    this.loadLoans();
  }

  loadLoans(): void {
    this.loading.set(true);
    this.error.set(null);

    this.loansService
      .listLoans()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (loans) => {
          this.loans.set(loans);
          this.dataSource.data = loans;
        },
        error: () => this.error.set('Failed to load loans. Please try again.'),
      });
  }
}
