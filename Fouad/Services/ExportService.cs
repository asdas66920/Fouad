using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Fouad.Services
{
    /// <summary>
    /// Service for exporting data to various formats including Excel and PDF.
    /// </summary>
    public class ExportService : IExportService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportService"/> class.
        /// </summary>
        public ExportService()
        {
            // Constructor is empty now
        }
        
        /// <summary>
        /// Exports data to Excel format using EPPlus library.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the Excel file</param>
        /// <param name="headers">Optional headers for columns</param>
        public void ExportToExcel<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null)
        {
            try
            {
                // Set EPPlus license for non-commercial use
                ExcelPackage.License.SetNonCommercialPersonal("khaled");
                
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Export");
                
                var dataList = data.ToList();
                if (!dataList.Any()) return;
                
                // Get properties of the type T
                var properties = typeof(T).GetProperties();
                
                // Add headers
                if (headers != null)
                {
                    var headerList = headers.ToList();
                    for (int i = 0; i < Math.Min(headerList.Count, properties.Length); i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headerList[i];
                    }
                }
                else
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = properties[i].Name;
                    }
                }
                
                // Add data rows
                for (int row = 0; row < dataList.Count; row++)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var value = properties[col].GetValue(dataList[row]);
                        worksheet.Cells[row + 2, col + 1].Value = value?.ToString() ?? "";
                    }
                }
                
                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();
                
                // Save the file
                var fileInfo = new FileInfo(filePath);
                package.SaveAs(fileInfo);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error exporting to Excel: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to export data to Excel format", ex);
            }
        }
        
        /// <summary>
        /// Asynchronously exports data to Excel format.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the Excel file</param>
        /// <param name="headers">Optional headers for columns</param>
        public async Task ExportToExcelAsync<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null)
        {
            await Task.Run(() => ExportToExcel(data, filePath, headers));
        }

        /// <summary>
        /// Exports data to PDF format using iText7 library.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the PDF file</param>
        /// <param name="headers">Optional headers for columns</param>
        public void ExportToPdf<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null)
        {
            try
            {
                var dataList = data.ToList();
                if (!dataList.Any()) return;
                
                // Create PDF document
                var writer = new PdfWriter(filePath);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);
                
                // Get properties of the type T
                var properties = typeof(T).GetProperties();
                
                // Create table
                var columnCount = properties.Length;
                var table = new Table(columnCount);
                table.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));
                
                // Add headers
                if (headers != null)
                {
                    var headerList = headers.ToList();
                    for (int i = 0; i < Math.Min(headerList.Count, properties.Length); i++)
                    {
                        var cell = new Cell().Add(new Paragraph(headerList[i]));
                        cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                        table.AddHeaderCell(cell);
                    }
                }
                else
                {
                    foreach (var prop in properties)
                    {
                        var cell = new Cell().Add(new Paragraph(prop.Name));
                        cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                        table.AddHeaderCell(cell);
                    }
                }
                
                // Add data rows
                foreach (var item in dataList)
                {
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(item)?.ToString() ?? "";
                        table.AddCell(new Cell().Add(new Paragraph(value)));
                    }
                }
                
                document.Add(table);
                document.Close();
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Error exporting to PDF: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to export data to PDF format", ex);
            }
        }
        
        /// <summary>
        /// Asynchronously exports data to PDF format.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the PDF file</param>
        /// <param name="headers">Optional headers for columns</param>
        public async Task ExportToPdfAsync<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null)
        {
            await Task.Run(() => ExportToPdf(data, filePath, headers));
        }
    }
}