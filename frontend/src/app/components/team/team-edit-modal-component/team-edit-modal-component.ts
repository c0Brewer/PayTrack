import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-team-edit-modal-component',
  imports: [FormsModule, ModalComponent],
  templateUrl: './team-edit-modal-component.html',
  styleUrl: './team-edit-modal-component.scss',
})
export class TeamEditModalComponent {}
