using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionaryStuff.ETL
{
    public class LoadRowsFromFixedWidthTextSettings : LoadRowsSettings
    {
        public IList<IFixedWidthColumnInfo> ColumnInfos { get; set; }
    }
}
