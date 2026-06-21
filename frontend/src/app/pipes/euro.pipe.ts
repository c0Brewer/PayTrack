import { CurrencyPipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'euro', standalone: true })
export class EuroPipe implements PipeTransform {
  private readonly currency = new CurrencyPipe('de-DE');

  transform(value: number | null | undefined): string {
    if (value == null) return '—';
    return this.currency.transform(value, 'EUR', 'symbol', '1.2-2', 'de-DE') ?? '—';
  }
}
