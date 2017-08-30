using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevolutionaryStuff.ETL
{
    public class FixedWidthColumnInfo : IFixedWidthColumnInfo
    {
        [JsonProperty("columnName")]
        public string ColumnName { get; set; }

        [JsonProperty("startAt")]
        public int StartAt { get; set; }

        [JsonProperty("endAt")]
        public int? EndAt { get; set; }

        [JsonProperty("length")]
        public int? Length { get; set; }

        [JsonProperty("dataType")]
        public Type DataType { get; set; }

        public string ToJson()
            => JsonConvert.SerializeObject(this);

        public static FixedWidthColumnInfo CreateFromJson(string json)
            => JsonConvert.DeserializeObject<FixedWidthColumnInfo>(json);

        public static IList<IFixedWidthColumnInfo> CreateFromCsv(string csv)
        {
            var infos = new List<IFixedWidthColumnInfo>();
            foreach (var row in CSV.ParseText(csv).Where(z=>z.Length>=2))
            {
                var info = new FixedWidthColumnInfo
                {
                    ColumnName = row[0],
                    StartAt = int.Parse(row[1])
                };
                if (row.Length > 2)
                {
                    info.EndAt = Parse.ParseNullableInt32(row[2]);
                }
                if (row.Length > 3)
                {
                    info.Length = Parse.ParseNullableInt32(row[3]);
                }
                if (row.Length > 4 && null!=StringHelpers.TrimOrNull(row[4]))
                {
                    info.DataType = Type.GetType(row[4].Trim());
                }
                infos.Add(info);
            }
            return infos;
        }
    }
}
