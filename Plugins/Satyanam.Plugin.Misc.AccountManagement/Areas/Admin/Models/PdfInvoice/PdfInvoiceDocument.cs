using App.Core.Infrastructure;
using App.Services.Configuration;
using App.Services.Media;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PdfInvoice;

public partial class PdfInvoiceDocument : IDocument
{
    #region Fields

    protected readonly PdfInvoiceModel _pdfInvoiceModel;

    #endregion

    #region Ctor

    public PdfInvoiceDocument(PdfInvoiceModel pdfInvoiceModel)
    {
        _pdfInvoiceModel = pdfInvoiceModel;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    #endregion

    #region Generate Invoice Methods

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            page.Header().Stack(stack =>
            {
                if (_pdfInvoiceModel.LogoImage != null)
                    stack.Item().Container().Padding(5).Height(60).Width(200).Image(_pdfInvoiceModel.LogoImage, ImageScaling.FitArea);

                stack.Item().Row(row =>
                {
                    row.RelativeColumn().Stack(subStack =>
                    {
                        subStack.Item().Text(_pdfInvoiceModel.CompanyName).FontSize(18).Bold();
                        subStack.Item().Text(_pdfInvoiceModel.CompanyAddress);
                        subStack.Item().Text(_pdfInvoiceModel.CompanyWebsite);
                    });

                    row.ConstantColumn(200).Stack(subStack =>
                    {
                        subStack.Item().Text($"INVOICE #{_pdfInvoiceModel.InvoiceId}").FontSize(16).Bold();
                        subStack.Item().Text($"Date: {_pdfInvoiceModel.InvoiceDate:dd/MM/yyyy}");
                    });
                });
            });

            page.Content().PaddingVertical(20).Column(col =>
            {
                col.Item().Text("Bill To").Bold();
                col.Item().Text(_pdfInvoiceModel.BillTo);

                col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(10);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(4);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Description").Bold();
                        header.Cell().AlignRight().Text("Quantity").Bold();
                        header.Cell().AlignRight().Text("Rate").Bold();
                        header.Cell().AlignRight().Text("Amount").Bold();
                    });

                    foreach (var item in _pdfInvoiceModel.Items)
                    {
                        table.Cell().Text(item.Description);
                        table.Cell().AlignRight().Text(item.Hours.ToString("0.##"));
                        table.Cell().AlignRight().Text(item.Rate.ToString("C"));
                        table.Cell().AlignRight().Text(item.Amount.ToString("C"));
                    }

                    table.Cell().ColumnSpan(4).PaddingTop(10);

                    AddSummaryRow("Subtotal:", _pdfInvoiceModel.SubTotal);
                    AddSummaryRow("Discount:", _pdfInvoiceModel.Discount);
                    AddSummaryRow("Tax:", _pdfInvoiceModel.Tax);
                    if (_pdfInvoiceModel.Total > decimal.Zero)
                    {
                        AddSummaryRow("Total:", _pdfInvoiceModel.Total, bold: true, isTotal: true);
                    }
                    else
                    {
                        AddSummaryRow($"Total: ({_pdfInvoiceModel.PrimaryCurrency})", _pdfInvoiceModel.TotalPrimaryAmount, bold: true, isTotal: true);
                        AddSummaryRow($"Total: ({_pdfInvoiceModel.PaymentCurrency})", _pdfInvoiceModel.TotalPaymentAmount, bold: true, isTotal: true);
                    }

                    void AddSummaryRow(string label, decimal value, bool bold = false, bool isTotal = false)
                    {
                        table.Cell().Text("");
                        table.Cell().Text("");

                        var labelCell = table.Cell().AlignRight().Text(label);
                        var valueCell = table.Cell().AlignRight().Text($"{value:C}");

                        if (bold)
                        {
                            labelCell.Bold();
                            valueCell.Bold();
                        }
                    }
                });

                if (!string.IsNullOrEmpty(_pdfInvoiceModel.BankDetails))
                {
                    col.Item().PaddingTop(20).Text("Payment Instructions:").Bold();
                    col.Item().Text(_pdfInvoiceModel.BankDetails);
                }

                if (!string.IsNullOrEmpty(_pdfInvoiceModel.Notes))
                {
                    col.Item().PaddingTop(20).Text("Notes:").Bold();
                    col.Item().Text(_pdfInvoiceModel.Notes);
                }
            });
        });
    }


    #endregion
}
