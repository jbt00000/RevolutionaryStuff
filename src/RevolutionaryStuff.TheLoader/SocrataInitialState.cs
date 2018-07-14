using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionaryStuff.TheLoader.Socrata
{
#if true
    public class Rootobject
    {
        public View view { get; set; }
        public object[] relatedViews { get; set; }
        public object[] featuredContent { get; set; }
        public object[] revisions { get; set; }
    }

    public class View
    {
        public object[] allAccessPoints { get; set; }
        public string apiFoundryUrl { get; set; }
        public object attachments { get; set; }
        public object attribution { get; set; }
        public object attributionLink { get; set; }
        public object blobFilename { get; set; }
        public object blobId { get; set; }
        public object blobMimeType { get; set; }
        public bool blobType { get; set; }
        public string bootstrapUrl { get; set; }
        public bool canPublish { get; set; }
        public string cartoUrl { get; set; }
        public string category { get; set; }
        public object collectionParentView { get; set; }
        public Column1[] columns { get; set; }
        public object commentUrl { get; set; }
        public DateTime createdAt { get; set; }
        public string csvResourceUrl { get; set; }
        public Custommetadatafieldset[] customMetadataFieldsets { get; set; }
        public string description { get; set; }
        public object disableContactDatasetOwner { get; set; }
        public object editMetadataUrl { get; set; }
        public string editUrl { get; set; }
        public string emailShareUrl { get; set; }
        public string[] exportFormats { get; set; }
        public string facebookShareUrl { get; set; }
        public string geoJsonResourceUrl { get; set; }
        public string gridUrl { get; set; }
        public string id { get; set; }
        public bool isBlobby { get; set; }
        public bool isHref { get; set; }
        public bool isPrivate { get; set; }
        public bool isTabular { get; set; }
        public bool isUnpublished { get; set; }
        public DateTime lastUpdatedAt { get; set; }
        public object licenseLink { get; set; }
        public object licenseLogo { get; set; }
        public object licenseName { get; set; }
        public Metadata metadata { get; set; }
        public string name { get; set; }
        public object namedResourceUrl { get; set; }
        public string odataUrl { get; set; }
        public string odataUrlV4 { get; set; }
        public string ownerName { get; set; }
        public string plotlyUrl { get; set; }
        public string provenance { get; set; }
        public string resourceUrl { get; set; }
        public string rowLabel { get; set; }
        public string rowLabelMultiple { get; set; }
        public object statsUrl { get; set; }
        public string[] tags { get; set; }
        public string twitterShareUrl { get; set; }
        public int viewCount { get; set; }
        public Coreview coreView { get; set; }
    }

    public class Metadata
    {
        public string rdfSubject { get; set; }
        public string rdfClass { get; set; }
        public Custom_Fields custom_fields { get; set; }
        public string rowIdentifier { get; set; }
        public string rowLabel { get; set; }
        public object[] flags { get; set; }
    }

    public class Custom_Fields
    {
        public CommonCore CommonCore { get; set; }
    }

    public class CommonCore
    {
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ProgramCode { get; set; }
        public string BureauCode { get; set; }
    }

    public class Coreview
    {
        public string id { get; set; }
        public string name { get; set; }
        public float averageRating { get; set; }
        public string category { get; set; }
        public int createdAt { get; set; }
        public string description { get; set; }
        public string displayType { get; set; }
        public string domainCName { get; set; }
        public int downloadCount { get; set; }
        public bool hideFromCatalog { get; set; }
        public bool hideFromDataJson { get; set; }
        public int indexUpdatedAt { get; set; }
        public bool newBackend { get; set; }
        public int numberOfComments { get; set; }
        public int oid { get; set; }
        public string provenance { get; set; }
        public bool publicationAppendEnabled { get; set; }
        public int publicationDate { get; set; }
        public int publicationGroup { get; set; }
        public string publicationStage { get; set; }
        public string rowClass { get; set; }
        public int rowsUpdatedAt { get; set; }
        public string rowsUpdatedBy { get; set; }
        public int tableId { get; set; }
        public int totalTimesRated { get; set; }
        public int viewCount { get; set; }
        public int viewLastModified { get; set; }
        public string viewType { get; set; }
        public Column[] columns { get; set; }
        public Grant[] grants { get; set; }
        public Metadata1 metadata { get; set; }
        public Owner owner { get; set; }
        public Query query { get; set; }
        public string[] rights { get; set; }
        public Tableauthor tableAuthor { get; set; }
        public string[] tags { get; set; }
        public string[] flags { get; set; }
    }

    public class Metadata1
    {
        public string rdfSubject { get; set; }
        public string rdfClass { get; set; }
        public Custom_Fields1 custom_fields { get; set; }
        public string rowIdentifier { get; set; }
        public string rowLabel { get; set; }
    }

    public class Custom_Fields1
    {
        public CommonCore1 CommonCore { get; set; }
    }

    public class CommonCore1
    {
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ProgramCode { get; set; }
        public string BureauCode { get; set; }
    }

    public class Owner
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string screenName { get; set; }
        public string type { get; set; }
    }

    public class Query
    {
        public Orderby[] orderBys { get; set; }
    }

    public class Orderby
    {
        public bool ascending { get; set; }
        public Expression expression { get; set; }
    }

    public class Expression
    {
        public int columnId { get; set; }
        public string type { get; set; }
    }

    public class Tableauthor
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string screenName { get; set; }
        public string type { get; set; }
    }

    public class Column
    {
        public int id { get; set; }
        public string name { get; set; }
        public string fieldName { get; set; }
        public int position { get; set; }
        public int width { get; set; }
        public string dataTypeName { get; set; }
        public string renderTypeName { get; set; }
        public int tableColumnId { get; set; }
        public Cachedcontents cachedContents { get; set; }
        public Format format { get; set; }
        public object flags { get; set; }
    }

    public class Cachedcontents
    {
        public object largest { get; set; }
        public int non_null { get; set; }
        public int _null { get; set; }
        public Top[] top { get; set; }
        public object smallest { get; set; }
        public string average { get; set; }
        public string sum { get; set; }
    }

    public class Top
    {
        public object item { get; set; }
        public int count { get; set; }
    }

    public class Format
    {
        public string view { get; set; }
        public string align { get; set; }
        public string displayStyle { get; set; }
    }

    public class Grant
    {
        public bool inherited { get; set; }
        public string type { get; set; }
        public string[] flags { get; set; }
    }

    public class Column1
    {
        public int id { get; set; }
        public string name { get; set; }
        public string fieldName { get; set; }
        public int position { get; set; }
        public int width { get; set; }
        public string dataTypeName { get; set; }
        public string renderTypeName { get; set; }
        public int tableColumnId { get; set; }
        public Cachedcontents1 cachedContents { get; set; }
        public Format1 format { get; set; }
        public object flags { get; set; }
    }

    public class Cachedcontents1
    {
        public object largest { get; set; }
        public int non_null { get; set; }
        public int _null { get; set; }
        public Top1[] top { get; set; }
        public object smallest { get; set; }
        public string average { get; set; }
        public string sum { get; set; }
    }

    public class Top1
    {
        public object item { get; set; }
        public int count { get; set; }
    }

    public class Format1
    {
        public string view { get; set; }
        public string align { get; set; }
        public string displayStyle { get; set; }
    }

    public class Custommetadatafieldset
    {
        public string name { get; set; }
        public Field[] fields { get; set; }
        public Existing_Fields existing_fields { get; set; }
    }

    public class Existing_Fields
    {
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ProgramCode { get; set; }
        public string BureauCode { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public bool required { get; set; }
        public string[] options { get; set; }
        public string type { get; set; }
    }


#else
    public class Rootobject
        {
            public View view { get; set; }
            public object[] relatedViews { get; set; }
            public object[] featuredContent { get; set; }
            public object[] revisions { get; set; }
        }

        public class View
        {
            public object[] allAccessPoints { get; set; }
            public string apiFoundryUrl { get; set; }
            public Attachment2[] attachments { get; set; }
            public string attribution { get; set; }
            public object attributionLink { get; set; }
            public object blobFilename { get; set; }
            public object blobId { get; set; }
            public object blobMimeType { get; set; }
            public bool blobType { get; set; }
            public string bootstrapUrl { get; set; }
            public bool canPublish { get; set; }
            public object cartoUrl { get; set; }
            public string category { get; set; }
            public object collectionParentView { get; set; }
            public Column1[] columns { get; set; }
            public object commentUrl { get; set; }
            public DateTime createdAt { get; set; }
            public string csvResourceUrl { get; set; }
            public Custommetadatafieldset[] customMetadataFieldsets { get; set; }
            public string description { get; set; }
            public object disableContactDatasetOwner { get; set; }
            public object editMetadataUrl { get; set; }
            public string editUrl { get; set; }
            public string emailShareUrl { get; set; }
            public string[] exportFormats { get; set; }
            public string facebookShareUrl { get; set; }
            public object geoJsonResourceUrl { get; set; }
            public string gridUrl { get; set; }
            public string id { get; set; }
            public bool isBlobby { get; set; }
            public bool isHref { get; set; }
            public bool isPrivate { get; set; }
            public bool isTabular { get; set; }
            public bool isUnpublished { get; set; }
            public DateTime lastUpdatedAt { get; set; }
            public object licenseLink { get; set; }
            public object licenseLogo { get; set; }
            public object licenseName { get; set; }
            public Metadata metadata { get; set; }
            public string name { get; set; }
            public object namedResourceUrl { get; set; }
            public string odataUrl { get; set; }
            public string odataUrlV4 { get; set; }
            public string ownerName { get; set; }
            public string plotlyUrl { get; set; }
            public string provenance { get; set; }
            public string resourceUrl { get; set; }
            public string rowLabel { get; set; }
            public string rowLabelMultiple { get; set; }
            public object statsUrl { get; set; }
            public string[] tags { get; set; }
            public string twitterShareUrl { get; set; }
            public int viewCount { get; set; }
            public Coreview coreView { get; set; }
        }

        public class Metadata
        {
            public string rdfSubject { get; set; }
            public Attachment[] attachments { get; set; }
            public Custom_Fields custom_fields { get; set; }
            public string rowLabel { get; set; }
            public object[] flags { get; set; }
        }

        public class Custom_Fields
        {
            public CommonCore CommonCore { get; set; }
        }

        public class CommonCore
        {
            public string ContactEmail { get; set; }
            public string ContactName { get; set; }
            public string ProgramCode { get; set; }
            public string BureauCode { get; set; }
        }

        public class Attachment
        {
            public string filename { get; set; }
            public string assetId { get; set; }
            public string name { get; set; }
        }

        public class Coreview
        {
            public string id { get; set; }
            public string name { get; set; }
            public string attribution { get; set; }
            public float averageRating { get; set; }
            public string category { get; set; }
            public int createdAt { get; set; }
            public string description { get; set; }
            public string displayType { get; set; }
            public string domainCName { get; set; }
            public int downloadCount { get; set; }
            public bool hideFromCatalog { get; set; }
            public bool hideFromDataJson { get; set; }
            public int indexUpdatedAt { get; set; }
            public string licenseId { get; set; }
            public bool newBackend { get; set; }
            public int numberOfComments { get; set; }
            public int oid { get; set; }
            public string provenance { get; set; }
            public bool publicationAppendEnabled { get; set; }
            public int publicationDate { get; set; }
            public int publicationGroup { get; set; }
            public string publicationStage { get; set; }
            public int rowsUpdatedAt { get; set; }
            public string rowsUpdatedBy { get; set; }
            public int tableId { get; set; }
            public int totalTimesRated { get; set; }
            public int viewCount { get; set; }
            public int viewLastModified { get; set; }
            public string viewType { get; set; }
            public Column[] columns { get; set; }
            public Grant[] grants { get; set; }
            public License license { get; set; }
            public Metadata1 metadata { get; set; }
            public Owner owner { get; set; }
            public Query query { get; set; }
            public string[] rights { get; set; }
            public Tableauthor tableAuthor { get; set; }
            public string[] tags { get; set; }
            public string[] flags { get; set; }
        }

        public class License
        {
            public string name { get; set; }
        }

        public class Metadata1
        {
            public string rdfSubject { get; set; }
            public Attachment1[] attachments { get; set; }
            public Custom_Fields1 custom_fields { get; set; }
            public string rowLabel { get; set; }
        }

        public class Custom_Fields1
        {
            public CommonCore1 CommonCore { get; set; }
        }

        public class CommonCore1
        {
            public string ContactEmail { get; set; }
            public string ContactName { get; set; }
            public string ProgramCode { get; set; }
            public string BureauCode { get; set; }
        }

        public class Attachment1
        {
            public string filename { get; set; }
            public string assetId { get; set; }
            public string name { get; set; }
        }

        public class Owner
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string screenName { get; set; }
            public string type { get; set; }
        }

        public class Query
        {
            public Orderby[] orderBys { get; set; }
        }

        public class Orderby
        {
            public bool ascending { get; set; }
            public Expression expression { get; set; }
        }

        public class Expression
        {
            public int columnId { get; set; }
            public string type { get; set; }
        }

        public class Tableauthor
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public string screenName { get; set; }
            public string type { get; set; }
        }

        public class Column
        {
            public int id { get; set; }
            public string name { get; set; }
            public string fieldName { get; set; }
            public int position { get; set; }
            public int width { get; set; }
            public string dataTypeName { get; set; }
            public string renderTypeName { get; set; }
            public int tableColumnId { get; set; }
            public Cachedcontents cachedContents { get; set; }
            public Format format { get; set; }
            public object flags { get; set; }
        }

        public class Cachedcontents
        {
            public string largest { get; set; }
            public int non_null { get; set; }
            public string average { get; set; }
            public int _null { get; set; }
            public Top[] top { get; set; }
            public string smallest { get; set; }
            public string sum { get; set; }
        }

        public class Top
        {
            public string item { get; set; }
            public int count { get; set; }
        }

        public class Format
        {
            public string precisionStyle { get; set; }
            public string noCommas { get; set; }
            public string align { get; set; }
        }

        public class Grant
        {
            public bool inherited { get; set; }
            public string type { get; set; }
            public string[] flags { get; set; }
        }

        public class Attachment2
        {
            public string name { get; set; }
            public string href { get; set; }
            public string link { get; set; }
        }

        public class Column1
        {
            public int id { get; set; }
            public string name { get; set; }
            public string fieldName { get; set; }
            public int position { get; set; }
            public int width { get; set; }
            public string dataTypeName { get; set; }
            public string renderTypeName { get; set; }
            public int tableColumnId { get; set; }
            public Cachedcontents1 cachedContents { get; set; }
            public Format1 format { get; set; }
            public object flags { get; set; }
        }

        public class Cachedcontents1
        {
            public string largest { get; set; }
            public int non_null { get; set; }
            public string average { get; set; }
            public int _null { get; set; }
            public Top1[] top { get; set; }
            public string smallest { get; set; }
            public string sum { get; set; }
        }

        public class Top1
        {
            public string item { get; set; }
            public int count { get; set; }
        }

        public class Format1
        {
            public string precisionStyle { get; set; }
            public string noCommas { get; set; }
            public string align { get; set; }
        }

        public class Custommetadatafieldset
        {
            public string name { get; set; }
            public Field[] fields { get; set; }
            public Existing_Fields existing_fields { get; set; }
        }

        public class Existing_Fields
        {
            public string ContactEmail { get; set; }
            public string ContactName { get; set; }
            public string ProgramCode { get; set; }
            public string BureauCode { get; set; }
        }

        public class Field
        {
            public string name { get; set; }
            public bool required { get; set; }
            public string[] options { get; set; }
            public string type { get; set; }
        }
#endif

}
