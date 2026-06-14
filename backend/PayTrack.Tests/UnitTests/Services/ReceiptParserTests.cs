using FluentAssertions;
using PayTrack.Application.Services.Implementation;

namespace PayTrack.Tests.UnitTests.Services
{
    public class ReceiptParserTests
    {
        private readonly ReceiptParser parser = new();

        [Fact]
        public void Parse_ExtractsGermanInvoiceFields()
        {
            const string Text = """
                ACME Bürobedarf GmbH
                Musterstraße 12, 1010 Wien
                Rechnung
                Rechnungsnummer: INV-2026-0042
                Rechnungsdatum: 14.06.2026
                Zwischensumme EUR 100,00
                MwSt EUR 20,00
                Gesamtbetrag: EUR 120,00
                """;

            var result = this.parser.Parse(Text);

            result.ExtractionSucceeded.Should().BeTrue();
            result.Amount.Value.Should().Be(120m);
            result.Amount.Confidence.Should().Be(0.95m);
            result.InvoiceDate.Value.Should().Be(new DateTime(2026, 6, 14));
            result.InvoiceNumber.Value.Should().Be("INV-2026-0042");
        }

        [Fact]
        public void Parse_PrefersAmountDueOverTaxAndSubtotal()
        {
            const string Text = """
                Example Consulting Ltd
                Invoice No: C-9918
                Invoice Date: June 10, 2026
                Subtotal $1,200.00
                Tax $240.00
                Amount Due $1,440.00
                """;

            var result = this.parser.Parse(Text);

            result.Amount.Value.Should().Be(1440m);
            result.InvoiceDate.Value.Should().Be(new DateTime(2026, 6, 10));
            result.InvoiceNumber.Value.Should().Be("C-9918");
        }

        [Fact]
        public void Parse_ReturnsNullableFieldsWhenTextHasNoInvoiceData()
        {
            var result = this.parser.Parse("Thank you for your business");

            result.Amount.Value.Should().BeNull();
            result.InvoiceDate.Value.Should().BeNull();
            result.InvoiceNumber.Value.Should().BeNull();
            result.ExtractionSucceeded.Should().BeFalse();
        }

        [Fact]
        public void Parse_ReturnsFailedResultForEmptyText()
        {
            var result = this.parser.Parse(" \n ");

            result.ExtractionSucceeded.Should().BeFalse();
            result.Amount.Value.Should().BeNull();
            result.Message.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Parse_UsesFallbackCandidatesWhenLabelsAreMissing()
        {
            const string Text = """
                Example Store
                2026-06-10
                Item 19.99 EUR
                Card payment 1 234,56 EUR
                """;

            var result = this.parser.Parse(Text);

            result.ExtractionSucceeded.Should().BeTrue();
            result.Amount.Value.Should().Be(1234.56m);
            result.Amount.Confidence.Should().Be(0.45m);
            result.InvoiceDate.Value.Should().Be(new DateTime(2026, 6, 10));
            result.InvoiceDate.Confidence.Should().Be(0.55m);
            result.InvoiceNumber.Value.Should().BeNull();
        }

        [Fact]
        public void Parse_IgnoresExcludedAmountsAndInvalidDates()
        {
            const string Text = """
                Example Store
                Invoice Date: 31/02/2026
                Subtotal EUR 100,00
                Tax EUR 20,00
                """;

            var result = this.parser.Parse(Text);

            result.ExtractionSucceeded.Should().BeFalse();
            result.Amount.Value.Should().BeNull();
            result.InvoiceDate.Value.Should().BeNull();
        }
    }
}
