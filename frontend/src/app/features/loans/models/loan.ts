import { LoanStatus } from './loan-status';

export interface Loan {
  id: string;
  amount: number;
  currentBalance: number;
  applicantName: string;
  status: LoanStatus;
}
