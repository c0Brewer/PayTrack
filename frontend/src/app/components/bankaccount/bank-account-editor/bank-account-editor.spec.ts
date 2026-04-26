//AI helped with the test cases

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SimpleChange } from '@angular/core';

import { BankAccountEditorComponent } from './bank-account-editor';

describe('BankAccountEditorComponent', () => {
  let component: BankAccountEditorComponent;
  let fixture: ComponentFixture<BankAccountEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BankAccountEditorComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BankAccountEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnChanges should set form values when initialValue exists', () => {
    component.initialValue = {
      id: 5,
      accountHolder: 'Alice',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    };

    component.ngOnChanges({
      initialValue: new SimpleChange(null, component.initialValue, true),
    });

    expect(component.form.controls.accountHolder.value).toBe('Alice');
    expect(component.form.controls.iban.value).toBe('AT611904300234573201');
    expect(component.form.controls.bic.value).toBe('BKAUATWW');
  });

  it('ngOnChanges should reset form when initialValue becomes null', () => {
    component.initialValue = {
      id: 1,
      accountHolder: 'Before',
      iban: 'AT611904300234573202',
      bic: 'BKAUATWW',
    };
    component.ngOnChanges({
      initialValue: new SimpleChange(null, component.initialValue, true),
    });

    component.initialValue = null;
    component.ngOnChanges({
      initialValue: new SimpleChange({ id: 1 }, null, false),
    });

    expect(component.form.controls.accountHolder.value).toBe('');
    expect(component.form.controls.iban.value).toBe('');
    expect(component.form.controls.bic.value).toBe('');
  });

  it('onSubmit should set validation message and not emit when form is invalid', () => {
    const emitSpy = vi.spyOn(component.submitForm, 'emit');

    component.form.setValue({
      accountHolder: '',
      iban: '',
      bic: '',
    });

    component.onSubmit();

    expect(component.validationMessage).toContain('Please enter valid values');
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('onSubmit should emit normalized values when form is valid', () => {
    const emitSpy = vi.spyOn(component.submitForm, 'emit');

    component.form.setValue({
      accountHolder: '  Max  ',
      iban: ' at61 1904 3002 3457 3201 ',
      bic: ' bkau atww ',
    });

    component.onSubmit();

    expect(emitSpy).toHaveBeenCalledWith({
      accountHolder: 'Max',
      iban: 'AT611904300234573201',
      bic: 'BKAUATWW',
    });
  });

  it('form value changes should clear validation message', () => {
    component.validationMessage = 'Some validation error';

    component.form.controls.accountHolder.setValue('Valid Name');

    expect(component.validationMessage).toBe('');
  });
});
