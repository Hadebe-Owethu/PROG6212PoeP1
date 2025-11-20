using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ProgPOEP1.Models;
using System.Collections.Generic;

public class ClaimReportDocument : IDocument
{
    private readonly List<Claim> _claims;

    public ClaimReportDocument(List<Claim> claims)
    {
        _claims = claims ?? new List<Claim>();
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(20);
            page.Header().Text("Approved Claims Report").FontSize(20).Bold();
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120); // Claim ID
                    columns.RelativeColumn();    // Lecturer ID
                    columns.ConstantColumn(80);  // Hours
                    columns.ConstantColumn(80);  // Rate
                    columns.ConstantColumn(100); // Total
                });

                table.Header(header =>
                {
                    header.Cell().Text("Claim ID").Bold();
                    header.Cell().Text("Lecturer").Bold();
                    header.Cell().Text("Hours").Bold();
                    header.Cell().Text("Rate").Bold();
                    header.Cell().Text("Total").Bold();
                });

                foreach (var claim in _claims)
                {
                    var total = (claim?.HoursWorked ?? 0) * (claim?.HourlyRate ?? 0);

                    table.Cell().Text(claim?.ClaimID ?? "N/A");
                    table.Cell().Text(claim?.ContractorID ?? "N/A");
                    table.Cell().Text(claim?.HoursWorked.ToString() ?? "0");
                    table.Cell().Text(claim?.HourlyRate.ToString("C") ?? "R0.00");
                    table.Cell().Text(total.ToString("C"));
                }
            });
        });
    }
}
