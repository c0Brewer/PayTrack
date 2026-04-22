import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { TeamDto } from '../../../types/exporter';
import { ModalComponent } from '../../general/modal-component/modal-component';

@Component({
  selector: 'app-team-edit-modal-component',
  imports: [FormsModule, ModalComponent],
  templateUrl: './team-edit-modal-component.html',
  styleUrl: './team-edit-modal-component.scss',
})
export class TeamEditModalComponent implements OnChanges {
  @Input() team: TeamDto = {
    id: -1,
    name: '',
    description: '',
    displayColor: '',
  };

  @Output() saveEvent = new EventEmitter<TeamDto>();
  @Output() closeEvent = new EventEmitter<void>();

  originalTeam: TeamDto | null = null;

  ngOnChanges(): void {
    if (this.team) {
      // Deep clone to avoid mutating the original reference during editing.
      this.originalTeam = structuredClone(this.team);
    }
  }

  hasTeamBeenChanged(): boolean {
    if (!this.originalTeam) return false;

    return (
      this.team.name !== this.originalTeam.name ||
      this.team.description !== this.originalTeam.description ||
      this.team.displayColor !== this.originalTeam.displayColor
    );
  }

  onSave(): void {
    if (!this.hasTeamBeenChanged()) {
      this.onClose();
      return;
    }

    this.saveEvent.emit(this.team);
  }

  onClose(): void {
    this.closeEvent.emit();
  }
}
