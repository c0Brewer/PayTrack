import { AfterViewInit, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { CostCentreDto } from '../../../types/exporter';

@Component({
  selector: 'app-cost-centre-list-component',
  imports: [RouterLink, MatTableModule, MatSortModule, MatPaginatorModule, MatFormFieldModule, MatInputModule],
  templateUrl: './cost-centre-list-component.html',
  styleUrl: './cost-centre-list-component.scss',
})
export class CostCentreListComponent implements OnChanges, AfterViewInit {
  @Input() costCentres: CostCentreDto[] = [];

  @Output() openEdit = new EventEmitter<CostCentreDto>();
  @Output() openDelete = new EventEmitter<CostCentreDto>();

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns = ['id', 'name', 'description', 'budgets', 'actions'];
  dataSource = new MatTableDataSource<CostCentreDto>([]);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['costCentres']) {
      this.dataSource.data = this.costCentres;
    }
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sortingDataAccessor = (item, property) => {
      if (property === 'budgets') return item.budgets.length;
      return (item as unknown as Record<string, unknown>)[property] as string | number;
    };
  }

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }

  onOpenEdit(costCentre: CostCentreDto): void {
    this.openEdit.emit(costCentre);
  }

  onOpenDelete(costCentre: CostCentreDto): void {
    this.openDelete.emit(costCentre);
  }
}
