namespace RevolutionaryStuff.Core.Caching
{
    public class CacheCreationResult
    {
        public object Val { get; set; }

        public ICacheEntryRetentionPolicy RetentionPolicy { get; set; }

        public CacheCreationResult(object val, ICacheEntryRetentionPolicy retentionPolicy = null)
        {
            this.Val = val;
            this.RetentionPolicy = retentionPolicy;
        }
    }
}
