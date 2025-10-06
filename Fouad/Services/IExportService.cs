using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fouad.Services
{
    /// <summary>
    /// Interface for export service operations.
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exports data to Excel format.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the Excel file</param>
        /// <param name="headers">Optional headers for columns</param>
        void ExportToExcel<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null);
        
        /// <summary>
        /// Asynchronously exports data to Excel format.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the Excel file</param>
        /// <param name="headers">Optional headers for columns</param>
        Task ExportToExcelAsync<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null);

        /// <summary>
        /// Exports data to PDF format.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the PDF file</param>
        /// <param name="headers">Optional headers for columns</param>
        void ExportToPdf<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null);
        
        /// <summary>
        /// Asynchronously exports data to PDF format.
        /// </summary>
        /// <typeparam name="T">Type of data to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="filePath">Path to save the PDF file</param>
        /// <param name="headers">Optional headers for columns</param>
        Task ExportToPdfAsync<T>(IEnumerable<T> data, string filePath, IEnumerable<string>? headers = null);
    }
}