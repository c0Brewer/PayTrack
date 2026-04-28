import type { CostCentreDto, UpsertBudgetEntryDto } from "./exporter";

export interface CostCentreSaveEvent {
  costCentre: CostCentreDto;
  budgetsToUpsert: UpsertBudgetEntryDto[];
  budgetIdsToDelete: number[];
}
