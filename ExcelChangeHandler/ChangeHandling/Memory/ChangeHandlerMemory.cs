﻿using ExcelChangeHandler.Api.Config;
using ExcelChangeHandler.Base;
using ExcelChangeHandler.Excel;
using System;

namespace ExcelChangeHandler.ChangeHandling.Memory
{
    class ChangeHandlerMemory : BaseClass, IChangeHandlerMemory
    {
        private const int ExcelMaxColumnCount = 16384;
        private const int ExcelMaxRowCount = 1048576;

        private readonly IConfiguration _configuration;

        public long MaxRangeSizeForStoringData => _configuration.MaxMemorySize;
        private bool MemorySet { get; set; }
        public string? SheetName { get; private set; }
        public int? SheetRows { get; private set; }
        public int? SheetColumns { get; private set; }
        public string? RangeAddress { get; private set; }
        public long? RangeDataSize { get; private set; }
        public string[,]? RangeData { get; private set; }

        public ChangeHandlerMemory(ILoggingManager loggingManager, IConfiguration configuration) : base(loggingManager)
        {
            _configuration = configuration;
        }

        public void UnsetMemory()
        {
            MemorySet = false;
            SheetName = null;
            SheetRows = null;
            SheetColumns = null;
            RangeAddress = null;
            RangeDataSize = null;
            RangeData = null;
        }

        public void SetMemory(IWorksheet sheet, IRange range)
        {
            SheetName = sheet.Name;
            SheetRows = sheet.RowCount;
            SheetColumns = sheet.ColumnCount;
            RangeAddress = range.Address;

            RangeDataSize = (long)range.RowCount * (long)range.ColumnCount;
            if (RangeDataSize <= MaxRangeSizeForStoringData)
            {
                RangeData = TryReadRangeData(sheet, range);
            }
            else
            {
                RangeData = null;
            }
            MemorySet = true;
        }

        public IMemoryComparison Compare(IWorksheet sheet, IRange range)
        {
            bool dataMatches = false;
            string[,]? newRangeData = null;

            bool locationMatches = CheckLocationMatches(sheet, range);
            if (locationMatches && RangeData != null && CheckRangeSizeMatchesData(RangeData, range))
            {
                newRangeData = TryReadRangeData(sheet, range);
                dataMatches = newRangeData != null && CompareDataArrays(RangeData, newRangeData);
            }

            IChangeProperties? propertiesBeforeChange = GetChangePropertiesBeforeChangeOrNull();
            long cellCountAfterChange = (long)range.RowCount * (long)range.ColumnCount;
            IChangeProperties propertiesAfterChange = new ChangePropertiesImpl(sheet.Name, sheet.ColumnCount, sheet.RowCount, range.Address, cellCountAfterChange, newRangeData);

            return new MemoryComparison(locationMatches: locationMatches,
                                        dataMatches: dataMatches,
                                        isNewRow: CheckForNewRow(sheet, range),
                                        isRowDelete: CheckForRowDelete(sheet, range),
                                        isNewColumn: CheckForNewColumn(sheet, range),
                                        isColumnDelete: CheckForColumnDelete(sheet, range),
                                        propertiesBeforeChange: propertiesBeforeChange,
                                        propertiesAfterChange: propertiesAfterChange);
        }

        private IChangeProperties? GetChangePropertiesBeforeChangeOrNull()
        {
            if (!MemorySet)
            {
                return null;
            }
            return new ChangePropertiesImpl(SheetName, SheetColumns, SheetRows, RangeAddress, RangeDataSize, RangeData);
        }

        private string[,]? TryReadRangeData(IWorksheet sheet, IRange range)
        {
            try
            {
                return range.RangeData;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error reading range data into memory. Sheet: {0} ; Range: {1}", sheet.Name, RangeAddress), ex);
                return null;
            }
        }

        private bool CheckLocationMatches(IWorksheet sheet, IRange range)
        {
            return string.Equals(SheetName, sheet.Name, StringComparison.Ordinal)
                && string.Equals(RangeAddress, range.Address, StringComparison.Ordinal);
        }

        private bool CheckRangeSizeMatchesData(string[,] data, IRange range)
        {
            return data.GetLength(0) == range.RowCount && data.GetLength(1) == range.ColumnCount;
        }

        private bool CompareDataArrays(string[,] data1, string[,] data2)
        {
            if (data1.GetLength(0) != data2.GetLength(0) || data1.GetLength(1) != data2.GetLength(1))
            {
                return false;
            }

            for (int row = 0; row < data1.GetLength(0); row++)
            {
                for (int col = 0; col < data1.GetLength(1); col++)
                {
                    if (!string.Equals(data1[row, col], data2[row, col], StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckForNewRow(IWorksheet sheet, IRange range)
        {
            if (SheetRows == null || sheet.RowCount <= SheetRows)
            {
                return false;
            }
            return range.ColumnCount == ExcelMaxColumnCount;
        }

        private bool CheckForRowDelete(IWorksheet sheet, IRange range)
        {
            if (SheetRows == null || sheet.RowCount >= SheetRows)
            {
                return false;
            }
            return range.ColumnCount == ExcelMaxColumnCount;
        }

        private bool CheckForNewColumn(IWorksheet sheet, IRange range)
        {
            if (SheetColumns == null || sheet.ColumnCount <= SheetColumns)
            {
                return false;
            }
            return range.RowCount == ExcelMaxRowCount;
        }

        private bool CheckForColumnDelete(IWorksheet sheet, IRange range)
        {
            if (SheetColumns == null || sheet.ColumnCount >= SheetColumns)
            {
                return false;
            }
            return range.RowCount == ExcelMaxRowCount;
        }

    }
}
