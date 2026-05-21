import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ExternalNotificationService } from '../../../services/external-notification/external-notification-service';
import { NotificationService } from '../../../services/notification/notification-service';
import { ModalComponent } from '../modal-component/modal-component';

@Component({
  selector: 'app-external-notification-component',
  imports: [ModalComponent, FormsModule],
  templateUrl: './external-notification-component.html',
  styleUrl: './external-notification-component.scss',
})
export class ExternalNotificationComponent implements OnInit {
  @Input() type!: 'email' | 'slack';
  @Input() recipientEmail!: string;
  @Input() defaultSubject = '';
  @Input() defaultMessage = '';
  @Output() closeEvent = new EventEmitter<void>();

  subject = '';
  message = '';
  sending = false;

  constructor(
    private readonly externalNotificationService: ExternalNotificationService,
    private readonly notificationService: NotificationService,
  ) {}

  get title(): string {
    return this.type === 'email' ? 'Send Email Notification' : 'Send Slack Notification';
  }

  ngOnInit(): void {
    this.subject = this.defaultSubject;
    this.message = this.defaultMessage;
  }

  onSend(): void {
    this.sending = true;

    const request$ =
      this.type === 'email'
        ? this.externalNotificationService.sendEmail(
            this.recipientEmail,
            this.subject,
            this.message,
          )
        : this.externalNotificationService.sendSlack(this.recipientEmail, this.message);

    request$.subscribe({
      next: () => {
        this.notificationService.showSuccess('Notification sent successfully.');
        this.sending = false;
        this.closeEvent.emit();
      },
      error: (err: Error) => {
        this.notificationService.showError('Failed to send notification: ' + err.message);
        this.sending = false;
      },
    });
  }

  onClose(): void {
    this.closeEvent.emit();
  }
}
