import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentRequestByTeamDto, TransactionStatus } from '../../../../../types/exporter';

import { TeamRequestDetailComponent } from './detail-component';

describe('TeamRequestDetailComponent', () => {
  let component: TeamRequestDetailComponent;
  let fixture: ComponentFixture<TeamRequestDetailComponent>;

  const mockRequest = {
    id: 1,
    status: TransactionStatus.Submitted,
    amount: 150.0,
    dueDate: '2026-06-01T00:00:00Z',
    purposeOfPayment: 'Engine repair',
    team: { name: 'Chassis Team' },
    costCentre: { name: 'CC-Engineering' },
    user: { name: 'Alice' },
    paymentReference: 'REF-001',
    createdAt: '2026-01-01T00:00:00Z',
    paidAt: null,
    statusHistory: [],
  } as unknown as PaymentRequestByTeamDto;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeamRequestDetailComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TeamRequestDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should show loading indicator when loading is true', () => {
    component.loading = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Loading...');
  });

  it('should not show detail card when loading is true', () => {
    component.loading = true;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.detail-card')).toBeNull();
  });

  it('should render request fields when request is provided', () => {
    component.request = mockRequest;
    component.loading = false;
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Engine repair');
    expect(text).toContain('REF-001');
  });

  it('should show team row when showTeam is true', () => {
    component.request = mockRequest;
    component.loading = false;
    component.showTeam = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Chassis Team');
  });

  it('should hide team row when showTeam is false', () => {
    component.request = mockRequest;
    component.loading = false;
    component.showTeam = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Chassis Team');
  });

  it('should show user row when showUser is true', () => {
    component.request = mockRequest;
    component.loading = false;
    component.showUser = true;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Alice');
  });

  it('should hide user row when showUser is false', () => {
    component.request = mockRequest;
    component.loading = false;
    component.showUser = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Alice');
  });

  it('should emit back event when back button is clicked', () => {
    fixture.detectChanges();
    let emitted = false;
    component.back.subscribe(() => (emitted = true));
    (fixture.nativeElement.querySelector('.back-btn') as HTMLButtonElement).click();
    expect(emitted).toBe(true);
  });

  it('should render status history table when entries exist', () => {
    component.request = {
      ...mockRequest,
      statusHistory: [
        {
          fromStatus: TransactionStatus.Submitted,
          toStatus: TransactionStatus.Approved,
          changedAt: '2026-01-02T00:00:00Z',
          comment: 'Approved by finance',
        },
      ],
    } as unknown as PaymentRequestByTeamDto;
    component.loading = false;
    fixture.detectChanges();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Status History');
    expect(text).toContain('Approved by finance');
  });

  it('should not render status history table when history is empty', () => {
    component.request = mockRequest;
    component.loading = false;
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).not.toContain('Status History');
  });

  it('should return correct status labels', () => {
    expect(component.getStatusLabel(TransactionStatus.Submitted)).toBe('Submitted');
    expect(component.getStatusLabel(TransactionStatus.Approved)).toBe('Approved');
    expect(component.getStatusLabel(99 as TransactionStatus)).toBe('Unknown');
  });
});
