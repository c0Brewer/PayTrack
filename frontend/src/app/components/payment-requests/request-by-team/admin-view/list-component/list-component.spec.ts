import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentRequestByTeamDto, TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestAdminListComponent } from './list-component';

describe('TeamRequestAdminListComponent', () => {
  let component: TeamRequestAdminListComponent;
  let fixture: ComponentFixture<TeamRequestAdminListComponent>;

  const mockRequests = [
    {
      id: 1,
      amount: 150,
      status: TransactionStatus.Submitted,
      purposeOfPayment: 'Engine repair',
      dueDate: '2026-06-01T00:00:00Z',
      team: { name: 'Chassis Team' },
      budget: { name: 'CC-Eng' },
      user: { name: 'Alice' },
      createdAt: '2026-01-01T00:00:00Z',
    },
  ] as unknown as PaymentRequestByTeamDto[];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamRequestAdminListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestAdminListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render admin columns', () => {
    component.requests = mockRequests;
    fixture.detectChanges();

    const textContent = (fixture.nativeElement as HTMLElement).textContent;
    expect(textContent).toContain('Alice');
    expect(textContent).toContain('Chassis Team');
    expect(textContent).toContain('CC-Eng');
  });

  it('should emit openDetail when view button is clicked', () => {
    component.requests = mockRequests;
    fixture.detectChanges();

    let emitted: PaymentRequestByTeamDto | undefined;
    component.openDetail.subscribe((request) => (emitted = request));

    (fixture.nativeElement.querySelector('.icon-hover-btn') as HTMLButtonElement).click();

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
