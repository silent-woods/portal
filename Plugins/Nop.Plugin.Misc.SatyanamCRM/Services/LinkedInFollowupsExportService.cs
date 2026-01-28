using ClosedXML.Excel;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// LinkedInFollowups service
    /// </summary>
    public partial class LinkedInFollowupsExportService : ILinkedInFollowupsExportService
    {
        #region Fields

        #endregion

        #region Ctor

        public LinkedInFollowupsExportService()
        {

        }

        #endregion

        #region Methods

        #region LinkedInFollowups Export

        public async Task<byte[]> ExportLinkedInFollowupsToExcelAsync(List<LinkedInFollowupsDto> items)
        {
            await using var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("LinkedInFollowups");

                // Set column widths
                worksheet.Column("A").Width = 14; // FirstName
                worksheet.Column("B").Width = 14; // LastName
                worksheet.Column("C").Width = 30; // LinkedinUrl
                worksheet.Column("D").Width = 25; // Email
                worksheet.Column("E").Width = 30; // WebsiteUrl
                worksheet.Column("F").Width = 22; // LastMessDate
                worksheet.Column("G").Width = 14; // FollowUp
                worksheet.Column("H").Width = 18; // NextFollowupsDate
                worksheet.Column("I").Width = 14; // DaysUntilNext
                worksheet.Column("J").Width = 16; // RemainingFollowUps
                worksheet.Column("K").Width = 30; // AutoStatus
                worksheet.Column("L").Width = 14; // Status
                worksheet.Column("M").Width = 40; // Notes

                int row = 1;

                // Header row
                worksheet.Cell(row, 1).Value = "First Name";
                worksheet.Cell(row, 2).Value = "Last Name";
                worksheet.Cell(row, 3).Value = "Linkedin Url";
                worksheet.Cell(row, 4).Value = "Email";
                worksheet.Cell(row, 5).Value = "Website Url";
                worksheet.Cell(row, 6).Value = "Last Message Date";
                worksheet.Cell(row, 7).Value = "Follow-up # (next to send)";
                worksheet.Cell(row, 8).Value = "Next Follow-up Date";
                worksheet.Cell(row, 9).Value = "Days until Next";
                worksheet.Cell(row, 10).Value = "Remaining Follow-ups";
                worksheet.Cell(row, 11).Value = "Auto Status";
                worksheet.Cell(row, 12).Value = "Status (manual)";
                worksheet.Cell(row, 13).Value = "Notes";

                worksheet.Row(row).Style.Font.Bold = true;
                worksheet.Range($"A{row}:M{row}").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.75);

                row++;

                // Data rows
                foreach (var item in items)
                {
                    int currentRow = row;

                    worksheet.Cell(currentRow, 1).Value = item.FirstName;
                    worksheet.Cell(currentRow, 2).Value = item.LastName;
                    worksheet.Cell(currentRow, 3).Value = item.LinkedinUrl;
                    worksheet.Cell(currentRow, 4).Value = item.Email;
                    worksheet.Cell(currentRow, 5).Value = item.WebsiteUrl;
                    worksheet.Cell(currentRow, 6).Value = item.LastMessDate;
                    worksheet.Cell(currentRow, 7).Value = item.FollowUp;

                    // --- FORMULA COLUMNS START ---
                    // H: Next Follow-up Date (depends on F and G)
                    worksheet.Cell(currentRow, 8).FormulaA1 =
                        $"=IF(AND(F{currentRow}<>\"\",G{currentRow}>=1,G{currentRow}<=10), F{currentRow} + CHOOSE(G{currentRow},3,5,8,10,15,20,25,30,40,50), \"\")";

                    // I: Days until Next (depends on H)
                    worksheet.Cell(currentRow, 9).FormulaA1 =
                        $"=IF(H{currentRow}=\"\",\"\", H{currentRow} - TODAY())";

                    // J: Remaining Follow-ups (depends on G)
                    worksheet.Cell(currentRow, 10).FormulaA1 =
                        $"=IF(OR(G{currentRow}=\"\",G{currentRow}>10),0,11-G{currentRow})";

                    // K: Auto Status (depends on H & I)
                    worksheet.Cell(currentRow, 11).FormulaA1 =
                        $"=IF(H{currentRow}=\"\",\"No follow-up scheduled\", IF(I{currentRow}<0, \"Overdue by \"&ABS(I{currentRow})&\" days\", IF(I{currentRow}=0,\"Due Today\",\"Scheduled in \"&I{currentRow}&\" days\")))";
                    // --- FORMULA COLUMNS END ---

                    worksheet.Cell(currentRow, 12).Value = item.StatusId > 0? ((FollowUpStatusEnum)item.StatusId).ToString(): "None";
                    worksheet.Cell(currentRow, 13).Value = item.Notes;

                    //worksheet.Row(currentRow).Style.Alignment.WrapText = true;
                    row++;
                }

                // Adjust styles for date columns
                worksheet.Column(6).Style.DateFormat.Format = "yyyy-mm-dd";
                worksheet.Column(8).Style.DateFormat.Format = "yyyy-mm-dd";

                workbook.CalculateMode = XLCalculateMode.Auto; // ensure formulas recalc in Excel
                workbook.SaveAs(stream);
            }

            return stream.ToArray();
        }

        #endregion

        #endregion
    }
}