using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SpreadSheetImporter
{
    /// <summary>
    /// Import your Google SpreadSheet
    /// </summary>
    public static class SpreadSheetImporter
    {
        /// <summary>
        /// Get CSV from Google SpreadSheet
        /// [How to get SpreadSheet Id and Sheet Name]
        /// 1. Open your Google SpreadSheet
        /// 2. Click "Share" button
        /// 3. Click "Get shareable link"
        /// 4. Get the id from the link (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=0, the id is 1X2Y3Z)
        /// </summary>
        /// <param name="spreadSheetId">Spread Sheet Id (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=0, the id is 1X2Y3Z)</param>
        /// <param name="sheetGId">Sheet GId (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=123456, the id is 123456)</param>
        /// <returns>Raw Csv Text</returns>
        public static async Task<string> GetRawCsvAsync(string spreadSheetId, string sheetGId)
        {
            var req = UnityWebRequest.Get($"https://docs.google.com/spreadsheets/d/{spreadSheetId}/export?format=csv&gid={sheetGId}");
            await req.SendWebRequest();
            return req.downloadHandler.text;
        }

        /// <summary>
        /// Get CSV from Google SpreadSheet
        /// [How to get SpreadSheet Id and Sheet Name]
        /// 1. Open your Google SpreadSheet
        /// 2. Click "Share" button
        /// 3. Click "Get shareable link"
        /// 4. Get the id from the link (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=123456, the id is 1X2Y3Z)
        /// </summary>
        /// <param name="spreadSheetId">Spread Sheet Id (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=123456, the id is 1X2Y3Z)</param>
        /// <param name="sheetGId">Sheet GId (e.g. https://docs.google.com/spreadsheets/d/1X2Y3Z/edit#gid=123456, the id is 123456)</param>
        /// <param name="headerRow">Index of header row</param>
        /// <param name="dataStartRow">Index of data starting</param>
        /// <param name="primaryKeyColumn">Index of primary key column</param>
        /// <returns>Raw Csv Text</returns>
        public static async Task<SpreadSheetData> GetSpreadSheetDataAsync(string spreadSheetId, string sheetGId, int headerRow = 0, int dataStartRow = 1, int primaryKeyColumn = 0)
        {
            string csv = await GetRawCsvAsync(spreadSheetId, sheetGId);
            return new SpreadSheetData(csv, headerRow, dataStartRow, primaryKeyColumn);
        }
        
    }

    public class SpreadSheetRow
    {
        public string this[int index]
        {
            get => _cells[index];
            set => _cells[index] = value;
        }

        public string this[string header]
        {
            get => _cells[_parent.GetColumnIndex(header)];
            set => _cells[_parent.GetColumnIndex(header)] = value;
        }

        private readonly string[] _cells;
        private readonly SpreadSheetData _parent;
        
        public SpreadSheetRow(string[] cells, SpreadSheetData parent)
        {
            _cells = cells;
            _parent = parent;
        }
    }

    public class SpreadSheetData
    {
        /// <summary>
        /// Headers
        /// </summary>
        public IEnumerable<string> Headers => _headerToColumnIndex.Keys;
        
        /// <summary>
        /// Rows
        /// </summary>
        public IEnumerable<SpreadSheetRow> Rows => _rows;
        
        public SpreadSheetRow this[int index] => _rows[index];
        public SpreadSheetRow this[string primaryKey] => GetRow(primaryKey);
        public string this[string primaryKey, string header] => GetCell(primaryKey, header);
        public string this[int rowIndex, string header] => GetCell(rowIndex, header);
        public string this[int rowIndex, int columnIndex] => GetCell(rowIndex, columnIndex);
        public string this[string primaryKey, int columnIndex] => GetCell(primaryKey, columnIndex);
        
        private readonly Dictionary<string, int> _headerToColumnIndex = new();
        private readonly Dictionary<string, int> _primaryKeyToRowindex = new();
        private readonly List<SpreadSheetRow> _rows = new();
        
        public SpreadSheetData(string csv, int headerRow = 0, int dataStartRow = 1, int primaryKeyColumn = 0)
        {
            UpdateCsvData(csv, headerRow, dataStartRow, primaryKeyColumn);
        }
        
        /// <summary>
        /// Update Csv Data
        /// </summary>
        /// <param name="csv">Raw Csv Data</param>
        /// <param name="headerRow">Header Row Index</param>
        /// <param name="dataStartRow">First Data Index</param>
        /// <param name="primaryKeyColumn">Primary Key Data Index</param>
        public void UpdateCsvData(string csv, int headerRow = 0, int dataStartRow = 1, int primaryKeyColumn = 0)
        {
            _rows.Clear();
            _primaryKeyToRowindex.Clear();
            _headerToColumnIndex.Clear();
            
            string[] rows = csv.Split("\r\n");
            
            // Header
            string headerRowText = rows[headerRow];
            string[] headerCells = headerRowText.Split(',');
            for (int ci = 0; ci < headerCells.Length; ci++)
            {
                _headerToColumnIndex[headerCells[ci]] = ci;
            }
            
            // Data
            for (int ri = dataStartRow; ri < rows.Length; ri++)
            {
                string row = rows[ri];
                string[] cells = row.Split(',');
                for (int ci = 0; ci < cells.Length; ci++)
                {
                    cells[ci] = cells[ci].Trim('"');
                    
                    // Primary Key
                    if (ci == primaryKeyColumn)
                    {
                        _primaryKeyToRowindex[cells[ci]] = ri;
                    }
                }

                _rows.Add(new SpreadSheetRow(cells, this));
            }
        }
        
        public int GetRowIndex(string primaryKey)
        {
            if (_primaryKeyToRowindex.TryGetValue(primaryKey, out int rowIndex))
            {
                return rowIndex;
            }
            
            throw new IndexOutOfRangeException("Primary Key not found");
        }
        
        public int GetColumnIndex(string header)
        {
            if (_headerToColumnIndex.TryGetValue(header, out int columnIndex))
            {
                return columnIndex;
            }

            throw new IndexOutOfRangeException("Header not found");
        }
        
        public string GetCell(string primaryKey, string header)
        {
            if (!_primaryKeyToRowindex.TryGetValue(primaryKey, out int rowIndex)) return null;
            return _headerToColumnIndex.TryGetValue(header, out int columnIndex) ? _rows[rowIndex][columnIndex] : null;
        }
        
        public string GetCell(int rowIndex, string header)
        {
            return _headerToColumnIndex.TryGetValue(header, out int columnIndex) ? _rows[rowIndex][columnIndex] : null;
        }
        
        public string GetCell(int rowIndex, int columnIndex)
        {
            return _rows[rowIndex][columnIndex];
        }
        
        public string GetCell(string primaryKey, int columnIndex)
        {
            return _primaryKeyToRowindex.TryGetValue(primaryKey, out int rowIndex) ? _rows[rowIndex][columnIndex] : null;
        }
        
        public SpreadSheetRow GetRow(string primaryKey)
        {
            return _primaryKeyToRowindex.TryGetValue(primaryKey, out int rowIndex) ? _rows[rowIndex] : null;
        }
    }
}
