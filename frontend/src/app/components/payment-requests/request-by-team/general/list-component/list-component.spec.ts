import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentRequestByTeamDto, TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestListComponent } from './list-component';

describe('TeamRequestListComponent', () => {
  let component: TeamRequestListComponent;
  let fixture: ComponentFixture<TeamRequestListComponent>;

  const mockRequests = [
    {
      id: 1,
      amount: 150,
      status: TransactionStatus.Submitted,
      purposeOfPayment: 'Engine repair',
      dueDate: '2026-06-01T00:00:00Z',
      team: { name: 'Chassis Team' },
      costCentre: { name: 'CC-Eng' },
      user: { name: 'Alice' },
      createdAt: '2026-01-01T00:00:00Z',
    },
  ] as unknown as PaymentRequestByTeamDto[];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamRequestListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render a row for each request', () => {
    component.requests = mockRequests;
    fixture.detectChanges();
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows.length).toBe(1);
  });

  it('should show cost centre column when showCostCentreColumn is true', () => {
    component.requests = mockRequests;
    component.showCostCentreColumn = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('CC-Eng');
  });

  it('should hide cost centre column when showCostCentreColumn is false', () => {
    component.requests = mockRequests;
    component.showCostCentreColumn = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('CC-Eng');
  });

  it('should show user column when showUserColumn is true', () => {
    component.requests = mockRequests;
    component.showUserColumn = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Alice');
  });

  it('should hide user column when showUserColumn is false', () => {
    component.requests = mockRequests;
    component.showUserColumn = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Alice');
  });

  it('should emit openDetail when view button is clicked', () => {
    component.requests = mockRequests;
    fixture.detectChanges();

    let emitted: PaymentRequestByTeamDto | undefined;
    component.openDetail.subscribe((r) => (emitted = r));

    (fixture.nativeElement.querySelector('.view-btn') as HTMLButtonElement).click();

    expect(emitted).toEqual(mockRequests[0]);
  });

  it('should show empty state when requests is empty', () => {
    component.requests = [];
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'No payment requests found.',
    );
  });

  it('should return correct status labels', () => {
    expect(component.getTransactionStatusLabel(TransactionStatus.Submitted)).toBe('Submitted');
    expect(component.getTransactionStatusLabel(99 as TransactionStatus)).toBe('Unknown');
  });
});
