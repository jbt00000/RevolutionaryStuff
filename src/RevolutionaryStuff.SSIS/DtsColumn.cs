﻿using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace RevolutionaryStuff.SSIS
{
    public class DtsColumn : IDtsColumn
    {
        public bool IsInputColumn { get; private set; }

        public bool IsOutputColumn => !IsInputColumn;

        public string Name { get; private set; }

        public DataType DataType { get; private set; }

        private readonly IDTSInputColumn100 InnerInputColumn;
        private readonly IDTSOutputColumn100 InnerOutputColumn;

        private DtsColumn(bool isInputColumn, string name, DataType dataType)
        {
            IsInputColumn = isInputColumn;
            Name = name;
            DataType = dataType;
        }

        public DtsColumn(IDTSInputColumn100 column)
            : this(true, column.Name, column.DataType)
        {
            InnerInputColumn = column;
        }

        public DtsColumn(IDTSOutputColumn100 column)
            : this(false, column.Name, column.DataType)
        {
            InnerOutputColumn = column;
        }
    }
}
