using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalarySlip;

public partial class SalarySlipDocument : IDocument
{
    #region Fields

    protected readonly SalarySlipModel _model;

    #endregion

    #region Ctor

    public SalarySlipDocument(SalarySlipModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    #endregion

    #region Methods

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.DefaultTextStyle(x => x.FontSize(9));

            if (_model.LogoBytes != null && _model.LogoBytes.Length > 0)
            {
                var watermarkBytes = CreateWatermarkBytes(_model.LogoBytes, 0.12f);

                page.Background()
                    .AlignCenter()
                    .AlignTop()
                    .PaddingTop(200)
                    .Width(300)
                    .Image(watermarkBytes, ImageScaling.FitArea);
            }

            page.Content().Column(col =>
            {
                col.Item().Row(row =>
                {
                    var logoCol = row.RelativeColumn();
                    if (_model.LogoBytes != null && _model.LogoBytes.Length > 0)
                        logoCol.Height(55).Image(_model.LogoBytes, ImageScaling.FitArea);
                    else
                        logoCol.Text(_model.CompanyName ?? string.Empty).FontSize(14).Bold();

                    row.ConstantColumn(220).AlignRight().Column(addrCol =>
                    {
                        if (!string.IsNullOrEmpty(_model.CompanyAddress))
                            addrCol.Item().Text(_model.CompanyAddress).FontSize(8);
                        if (!string.IsNullOrEmpty(_model.CompanyCIN))
                            addrCol.Item().Text(_model.CompanyCIN).FontSize(7).Italic();
                    });
                });

                col.Item().PaddingTop(8);

                col.Item().AlignRight()
                    .Text($"{_model.SlipDate.Day}{GetDaySuffix(_model.SlipDate.Day)} {_model.SlipDate:MMMM yyyy}")
                    .FontSize(9);

                col.Item().PaddingTop(6);

                col.Item().AlignCenter()
                    .Text($"PAY SLIP FOR THE MONTH OF {_model.MonthYear.ToUpper()}")
                    .Bold().FontSize(11);

                col.Item().PaddingTop(8);

                col.Item().Border(1).BorderColor(Colors.Grey.Medium).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                    });

                    table.Cell().ColumnSpan(4).Background(Colors.Grey.Lighten3)
                        .Padding(4).Text($"Employee Name: {_model.EmployeeName}").Bold();

                    Cell("Working Days"); Cell(_model.WorkingDays.ToString("0.##"));
                    Cell("Designation"); Cell(_model.Designation ?? "—");

                    Cell("Date Of Joining"); Cell(_model.DateOfJoining ?? "—");
                    Cell("Pan Card"); Cell(_model.PanCardNumber ?? "");

                    Cell("Bank"); Cell(_model.BankName ?? "—");
                    Cell("Account Number"); Cell(_model.BankAccountNumber ?? "");

                    void Cell(string text, bool header = false)
                    {
                        var cell = table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4);
                        if (header) cell.Text(text).Bold();
                        else cell.Text(text);
                    }
                });

                col.Item().PaddingTop(8);

                var maxRows = Math.Max(_model.Earnings.Count, _model.Deductions.Count);

                col.Item().Border(1).BorderColor(Colors.Grey.Medium).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                    });

                    HeaderCell("Earnings"); HeaderCell("Amount");
                    HeaderCell("Deductions"); HeaderCell("Amount");

                    for (int i = 0; i < maxRows; i++)
                    {
                        var earn = i < _model.Earnings.Count ? _model.Earnings[i] : null;
                        var ded = i < _model.Deductions.Count ? _model.Deductions[i] : null;

                        DataCell(earn?.Name ?? "");
                        AmountCell(earn.HasValue() ? earn.Amount.ToString("N0") : "");
                        DataCell(ded?.Name ?? "");
                        AmountCell(ded.HasValue() ? ded.Amount.ToString("N0") : "");
                    }

                    TotalCell("Gross Salary"); TotalAmountCell(_model.GrossSalary.ToString("N0"));
                    TotalCell("Total Deductions"); TotalAmountCell(_model.TotalDeductions.ToString("N0"));

                    void HeaderCell(string text) =>
                        table.Cell().Background(Colors.Grey.Lighten3).BorderRight(1).BorderBottom(1)
                            .BorderColor(Colors.Grey.Medium).Padding(4).Text(text).Bold();

                    void DataCell(string text) =>
                        table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4).Text(text);

                    void AmountCell(string text) =>
                        table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4).AlignRight().Text(text);

                    void TotalCell(string text) =>
                        table.Cell().Background(Colors.Grey.Lighten3).BorderRight(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).Text(text).Bold();

                    void TotalAmountCell(string text) =>
                        table.Cell().Background(Colors.Grey.Lighten3).BorderRight(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignRight().Text(text).Bold();
                });

                var hasAdjustments = _model.AdjustmentAdditions.Any() || _model.AdjustmentDeductions.Any();
                if (hasAdjustments)
                {
                    col.Item().PaddingTop(4);

                    var adjMaxRows = Math.Max(_model.AdjustmentAdditions.Count, _model.AdjustmentDeductions.Count);
                    var totalAdjAdd = _model.AdjustmentAdditions.Sum(a => a.Amount);
                    var totalAdjDed = _model.AdjustmentDeductions.Sum(d => d.Amount);

                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1)
                            .BorderColor(Colors.Grey.Medium).Padding(4).Text("Other Additions").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1)
                            .BorderColor(Colors.Grey.Medium).Padding(4).AlignRight().Text("Amount").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1)
                            .BorderColor(Colors.Grey.Medium).Padding(4).Text("Other Deductions").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1)
                            .BorderColor(Colors.Grey.Medium).Padding(4).AlignRight().Text("Amount").Bold();

                        for (int i = 0; i < adjMaxRows; i++)
                        {
                            var add = i < _model.AdjustmentAdditions.Count ? _model.AdjustmentAdditions[i] : null;
                            var ded = i < _model.AdjustmentDeductions.Count ? _model.AdjustmentDeductions[i] : null;

                            table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4).Text(add?.Name ?? "");
                            table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4).AlignRight()
                                .Text(add != null ? add.Amount.ToString("N0") : "");
                            table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4).Text(ded?.Name ?? "");
                            table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(4).AlignRight()
                                .Text(ded != null ? ded.Amount.ToString("N0") : "");
                        }

                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).Text("Total Other Additions").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignRight().Text($"+ {totalAdjAdd:N0}").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).Text("Total Other Deductions").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignRight().Text($"− {totalAdjDed:N0}").Bold();

                        var totalEarnings = _model.GrossSalary + totalAdjAdd;
                        var totalAllDeductions = _model.TotalDeductions + totalAdjDed;
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).Text("Total Earnings").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignRight().Text(totalEarnings.ToString("N0")).Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).Text("Total Deductions").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).BorderRight(1).BorderColor(Colors.Grey.Medium)
                            .Padding(4).AlignRight().Text(totalAllDeductions.ToString("N0")).Bold();
                    });
                }

                col.Item().PaddingTop(6);

                col.Item().Background(Colors.Blue.Lighten4).Padding(6).Row(row =>
                {
                    row.RelativeColumn().Text($"Net Pay: Rs.{_model.NetSalary:N0}").Bold().FontSize(11);
                });

                col.Item().PaddingTop(4);
                col.Item().Text($"In Words: Rupees – {_model.NetSalaryInWords} only").Italic();

                col.Item().PaddingTop(30);

                col.Item().Text("Yours Sincerely,");
                col.Item().Text($"For {_model.CompanyName},").Bold();
                col.Item().PaddingTop(10).Row(row =>
                {
                    var sigCol = row.ConstantColumn(120);
                    if (_model.HrSignatureBytes != null && _model.HrSignatureBytes.Length > 0)
                        sigCol.Height(30).Image(_model.HrSignatureBytes, ImageScaling.FitArea);
                    else
                        sigCol.PaddingTop(30).Text("_______________________");
                });
                col.Item().Text(!string.IsNullOrEmpty(_model.HrPersonName) ? _model.HrPersonName : "Human Resource").Bold().FontSize(9);
                col.Item().Text("Human Resource").FontSize(8);
            });
        });
    }

    private static byte[] CreateWatermarkBytes(byte[] src, float opacity)
    {
        using var bitmap = SKBitmap.Decode(src);
        if (bitmap == null) return src;

        var result = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint
            {
                Color = SKColors.White.WithAlpha((byte)(opacity * 255))
            };
            canvas.DrawBitmap(bitmap, 0, 0, paint);
        }

        using var ms = new MemoryStream();
        result.Encode(ms, SKEncodedImageFormat.Png, 100);
        return ms.ToArray();
    }

    private static string GetDaySuffix(int day) => day switch
    {
        1 or 21 or 31 => "st",
        2 or 22 => "nd",
        3 or 23 => "rd",
        _ => "th"
    };

    #endregion
}

internal static class SalarySlipLineExtensions
{
    internal static bool HasValue(this SalarySlipLine line) => line != null && line.Amount != 0;
}
