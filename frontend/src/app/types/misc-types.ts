import type {
  CostCentreDto,
  TeamDto,
  UpsertBudgetEntryDto,
  UpsertTeamBudgetEntryDto,
} from './exporter';

export interface CostCentreSaveEvent {
  costCentre: CostCentreDto;
  budgetsToUpsert: UpsertBudgetEntryDto[];
  budgetIdsToDelete: number[];
}

export interface TeamSaveEvent {
  team: TeamDto;
  budgetsToUpsert: UpsertTeamBudgetEntryDto[];
  budgetIdsToDelete: number[];
}
