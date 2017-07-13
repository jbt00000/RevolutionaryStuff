using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace RevolutionaryStuff.ETL
{
    public class UploadIntoSqlServerSettings : UploadIntoDatastoreSettings
    {
        public string Schema { get; set; } = "dbo";
        public bool GenerateTable { get; set; }
    }
}
