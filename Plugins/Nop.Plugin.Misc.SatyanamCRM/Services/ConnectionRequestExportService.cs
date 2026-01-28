using ClosedXML.Excel;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// ConnectionRequestExport service
    /// </summary>
    public partial class ConnectionRequestExportService : IConnectionRequestExportService
    {
        #region Fields

        #endregion

        #region Ctor

        public ConnectionRequestExportService()
        {

        }

        #endregion

        #region Methods

        #region ConnectionRequest Export

        public async Task<byte[]> ExportConnectionRequestToExcelAsync(List<ConnectionRequestDto> items)
        {
            await using var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ConnectionRequest");

                // Set column widths
                worksheet.Column("A").Width = 14; // FirstName
                worksheet.Column("B").Width = 14; // LastName
                worksheet.Column("C").Width = 30; // LinkedinUrl
                worksheet.Column("D").Width = 25; // Email
                worksheet.Column("E").Width = 30; // WebsiteUrl
                worksheet.Column("F").Width = 14; // Status

                int row = 1;

                // Header row
                worksheet.Cell(row, 1).Value = "First Name";
                worksheet.Cell(row, 2).Value = "Last Name";
                worksheet.Cell(row, 3).Value = "Linkedin Url";
                worksheet.Cell(row, 4).Value = "Email";
                worksheet.Cell(row, 5).Value = "Website Url";
                worksheet.Cell(row, 6).Value = "Status";

                worksheet.Row(row).Style.Font.Bold = true;
                worksheet.Range($"A{row}:F{row}").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.75);

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
                    worksheet.Cell(currentRow, 6).Value = item.StatusId > 0? ((FollowUpStatusEnum)item.StatusId).ToString(): "None";
                    
                    //worksheet.Row(currentRow).Style.Alignment.WrapText = true;
                    row++;
                }

                workbook.SaveAs(stream);
            }

            return stream.ToArray();
        }

        #endregion

        #endregion
    }
}