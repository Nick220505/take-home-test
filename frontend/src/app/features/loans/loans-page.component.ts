import { Component } from '@angular/core';
import { LoansTableComponent } from './components/loans-table/loans-table.component';

@Component({
  selector: 'app-loans-page',
  standalone: true,
  imports: [LoansTableComponent],
  templateUrl: './loans-page.component.html',
  styleUrls: ['./loans-page.component.scss'],
})
export class LoansPageComponent {}
