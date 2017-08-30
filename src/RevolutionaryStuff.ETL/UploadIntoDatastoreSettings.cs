namespace RevolutionaryStuff.ETL
{
    public class UploadIntoDatastoreSettings
    {
        public int RowsTransferredNotifyIncrement { get; set; } = 1000;

        public RowsTransferredEventHandler RowsTransferredEventHandler { get; set; }
    }
}
