﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelChangeHandler.Excel
{
    public interface IRange
    {
        string Address { get; }
        int RowCount { get; }
        int ColumnCount { get; }
        string[,] RangeData { get; }

        void FillRange(int colour);
    }
}
