using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ProgPOEP1.Models;
using System.ComponentModel.DataAnnotations;
public class ClaimReportDocument : IDocument
{
    private List<Claim> Claims;

    public ClaimReportDocument(List<Claim> claims)
    {
        Claims = claims;
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
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("ClaimID").Bold();
                    header.Cell().Text("LecturerID").Bold();
                    header.Cell().Text("Month").Bold();
                    header.Cell().Text("Hours").Bold();
                    header.Cell().Text("Total").Bold();
                });

                foreach (var claim in Claims)
                {
                    table.Cell().Text(claim.ClaimID);
                    table.Cell().Text(claim.ContractorID);
                    table.Cell().Text(claim.Month);
                    table.Cell().Text(claim.HoursWorked.ToString());
                    table.Cell().Text(claim.TotalAmount.ToString("F2"));
                }
            });
        });
    }
}
