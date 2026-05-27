import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';

import { TypeaheadItem, TypeaheadSelectComponent } from './typeahead-select-component';

describe('TypeaheadSelectComponent', () => {
  let component: TypeaheadSelectComponent;
  let fixture: ComponentFixture<TypeaheadSelectComponent>;

  const mockItems: TypeaheadItem[] = [
    { id: 1, primaryText: 'Alice', secondaryText: 'alice@example.com' },
    { id: 2, primaryText: 'Bob', secondaryText: 'bob@example.com' },
    { id: 3, primaryText: 'Charlie', secondaryText: 'charlie@example.com' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TypeaheadSelectComponent, ReactiveFormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(TypeaheadSelectComponent);
    component = fixture.componentInstance;
    component.items = mockItems;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should filter items by primaryText immediately', () => {
    component.searchControl.setValue('ali');
    expect(component.results).toEqual([mockItems[0]]);
    expect(component.showDropdown).toBe(true);
  });

  it('should filter items by secondaryText', () => {
    component.searchControl.setValue('bob@');
    expect(component.results).toEqual([mockItems[1]]);
  });

  it('should be case-insensitive', () => {
    component.searchControl.setValue('ALICE');
    expect(component.results).toEqual([mockItems[0]]);
  });

  it('should hide dropdown when input is below minChars', () => {
    component.searchControl.setValue('');
    expect(component.results).toEqual([]);
    expect(component.showDropdown).toBe(false);
  });

  it('select should set selectedItem, hide dropdown, and emit itemSelected', () => {
    const spy = vi.spyOn(component.itemSelected, 'emit');
    component.select(mockItems[0]);
    expect(spy).toHaveBeenCalledWith(mockItems[0]);
    expect(component.showDropdown).toBe(false);
    expect(component.selectedItem).toBe(mockItems[0]);
  });

  it('select should format display text with secondaryText', () => {
    component.select(mockItems[0]);
    expect(component.searchControl.value).toBe('Alice (alice@example.com)');
  });

  it('clear should emit cleared and reset state', () => {
    const spy = vi.spyOn(component.cleared, 'emit');
    component.select(mockItems[0]);
    component.clear();
    expect(spy).toHaveBeenCalled();
    expect(component.selectedItem).toBeNull();
    expect(component.results).toEqual([]);
    expect(component.showDropdown).toBe(false);
    expect(component.searchControl.value).toBe('');
  });

  it('clear should not re-trigger filtering', () => {
    component.searchControl.setValue('ali');
    component.select(mockItems[0]);
    component.clear();
    expect(component.results).toEqual([]);
  });

  it('reset should clear state', () => {
    component.select(mockItems[0]);
    component.reset();
    expect(component.selectedItem).toBeNull();
    expect(component.results).toEqual([]);
  });

  it('onFocus should show dropdown when results are cached and no item is selected', () => {
    component.results = mockItems;
    component.showDropdown = false;
    component.selectedItem = null;
    component.onFocus();
    expect(component.showDropdown).toBe(true);
  });

  it('onFocus should not show dropdown when an item is selected', () => {
    component.results = mockItems;
    component.selectedItem = mockItems[0];
    component.showDropdown = false;
    component.onFocus();
    expect(component.showDropdown).toBe(false);
  });

  it('onFocus should not show dropdown when results are empty', () => {
    component.results = [];
    component.selectedItem = null;
    component.showDropdown = false;
    component.onFocus();
    expect(component.showDropdown).toBe(false);
  });

  it('onDocumentMousedown outside component should hide dropdown', () => {
    component.showDropdown = true;
    const outsideElement = document.createElement('div');
    component.onDocumentMousedown({ target: outsideElement } as unknown as MouseEvent);
    expect(component.showDropdown).toBe(false);
  });
});
