using App.Core;
using App.Data;
using App.Services.Localization;
using ClosedXML.Excel.Drawings;
using ClosedXML.Excel;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LeadExportManager service
    /// </summary>
    public partial class LeadExportManagerService : ILeadExportManagerService
    {
        #region Fields

        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public LeadExportManagerService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        #endregion

        #region Methods
        public virtual async Task<byte[]> ExportLeadsToExcelAsync(List<LeadDto> leads)
        {
            await using var stream = new MemoryStream();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Lead");
                worksheet.Column("A").Width = 17;
                worksheet.Column("B").Width = 14;
                worksheet.Column("C").Width = 20;
                worksheet.Column("D").Width = 13;
                worksheet.Column("E").Width = 16;
                worksheet.Column("F").Width = 22;
                worksheet.Column("G").Width = 22;
                worksheet.Column("H").Width = 19;
                worksheet.Column("I").Width = 14;
                worksheet.Column("J").Width = 20;
                worksheet.Column("K").Width = 10;
                worksheet.Column("L").Width = 13;
                worksheet.Column("M").Width = 12;
                worksheet.Column("N").Width = 22;
                worksheet.Column("O").Width = 17;
                worksheet.Column("P").Width = 17;
                worksheet.Column("Q").Width = 17;
                worksheet.Column("R").Width = 17;
                worksheet.Column("S").Width = 12;
                worksheet.Column("T").Width = 13;
                worksheet.Column("U").Width = 14;
                worksheet.Column("V").Width = 16;
                worksheet.Column("W").Width = 22;

                var range = worksheet.Range("A1:B10");

                worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;

                worksheet.PageSetup.CenterHorizontally = true; 
                worksheet.PageSetup.CenterVertically = false;
                
                int row = 1; // Start from row 1 for recipe data
                worksheet.Range(row, 1, row, 22);
                //row += 1;
                worksheet.Cell(row, 1).Value = "Lead Owner";
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 2).Value = "Lead Name";
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 3).Value = "Company Name";
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 4).Value = "Title";
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 5).Value = "Phone";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 6).Value = "Email";
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 7).Value = "WebsiteUrl";
                worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 8).Value = "Industry";
                worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 9).Value = "Lead Source";
                worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 10).Value = "Lead Status";
                worksheet.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 11).Value = "NoofEmployee";
                worksheet.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 12).Value = "Annual Revenue";
                worksheet.Cell(row, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 13).Value = "Category";
                worksheet.Cell(row, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 14).Value = "Secondary Email";
                worksheet.Cell(row, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 15).Value = "Skyp ID";
                worksheet.Cell(row, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 16).Value = "Twitter";
                worksheet.Cell(row, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 17).Value = "LinkedinUrl";
                worksheet.Cell(row, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 18).Value = "FacebookUrl";
                worksheet.Cell(row, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 19).Value = "Country";
                worksheet.Cell(row, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 20).Value = "State";
                worksheet.Cell(row, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 21).Value = "City";
                worksheet.Cell(row, 21).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

				worksheet.Cell(row, 22).Value = "ZipCode";
				worksheet.Cell(row, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

				worksheet.Cell(row, 23).Value = "Description";
                worksheet.Cell(row, 23).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                var cookingHeaderRow = worksheet.Row(row).Style.Font.Bold = true;
                worksheet.Range($"A{row}:W{row}").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.75);
                row++;
                foreach (var item in leads)
                {
                    
                    worksheet.Cell(row, 1).Value = item.CustomerId;
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 1).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 2).Value = item.Name;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 2).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 3).Value = item.CompanyName;
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 3).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 4).Value = item.TitleId;
                    worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 4).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 5).Value = item.Phone;
                    worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 5).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 6).Value = item.Email;
                    worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 6).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 7).Value = item.WebsiteUrl;
                    worksheet.Cell(row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 7).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 8).Value = item.IndustryId;
                    worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 8).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 9).Value = item.LeadSourceId;
                    worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 9).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 10).Value = item.LeadStatusId;
                    worksheet.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 10).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 11).Value = item.NoofEmployee;
                    worksheet.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 11).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 12).Value = item.AnnualRevenue;
                    worksheet.Cell(row, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 12).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 13).Value = item.CategoryId;
                    worksheet.Cell(row, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 13).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 14).Value = item.SecondaryEmail;
                    worksheet.Cell(row, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 14).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 15).Value = item.SkypeId;
                    worksheet.Cell(row, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 15).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 16).Value = item.Twitter;
                    worksheet.Cell(row, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 16).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 17).Value = item.LinkedinUrl;
                    worksheet.Cell(row, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 17).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 18).Value = item.Facebookurl;
                    worksheet.Cell(row, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 18).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 19).Value = item.Country;
                    worksheet.Cell(row, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 19).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 19).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 20).Value = item.State;
                    worksheet.Cell(row, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 20).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 20).Style.Alignment.WrapText = true;

                    worksheet.Cell(row, 21).Value = item.City;
                    worksheet.Cell(row, 21).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 21).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 21).Style.Alignment.WrapText = true;

					worksheet.Cell(row, 22).Value = item.ZipCode;
					worksheet.Cell(row, 22).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
					worksheet.Cell(row, 22).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
					worksheet.Cell(row, 22).Style.Alignment.WrapText = true;

					worksheet.Cell(row, 23).Value = item.Description;
                    worksheet.Cell(row, 23).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, 23).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    worksheet.Cell(row, 23).Style.Alignment.WrapText = true;

                    row++;
                }
                row += 1;
                // Save the workbook
                workbook.SaveAs(stream);
            }
            return stream.ToArray();
        }

        #endregion
    }
}