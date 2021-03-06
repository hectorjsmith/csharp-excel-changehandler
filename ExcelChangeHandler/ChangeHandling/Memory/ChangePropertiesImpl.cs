﻿using System;

namespace ExcelChangeHandler.ChangeHandling.Memory
{
    class ChangePropertiesImpl : IChangeProperties
    {
        public string? SheetName { get; }

        public int? SheetColumns { get; }

        public int? SheetRows { get; }

        public string? RangeAddress { get; }

        public long? RangeCellCount { get; }

        public string[,]? RangeFormulas { get; }

        public ChangePropertiesImpl(string? sheetName, int? sheetColumns, int? sheetRows, string? rangeAddress, long? rangeCellCount, string[,]? rangeFormulas)
        {
            SheetName = sheetName;
            SheetColumns = sheetColumns;
            SheetRows = sheetRows;
            RangeAddress = rangeAddress;
            RangeCellCount = rangeCellCount;
            RangeFormulas = rangeFormulas;
        }
    }
}
