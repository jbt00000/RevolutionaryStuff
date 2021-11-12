namespace RevolutionaryStuff.ETL;

public delegate void RowsTransferredEventHandler(object sender, RowsTransferredEventArgs e);

public class RowsTransferredEventArgs : EventArgs
{
    public RowsTransferredEventArgs(long rowsTransferred)
    {
        RowsTransferred = rowsTransferred;
    }

    public bool Abort { get; set; }

    public long RowsTransferred { get; }
}
