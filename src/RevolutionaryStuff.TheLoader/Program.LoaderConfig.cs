using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RevolutionaryStuff.TheLoader
{
    public partial class Program
    {
        public class LoaderConfig
        {
            public const string ConfigSectionName = "LoaderConfig";

            public IDictionary<string, string> MakeFriendlyReplacements { get; set; }

            public class Profile
            { 
                public string ProfileName { get; set; }
                public string ConnectionStringName { get; set; }

                public IList<TableConfig> TableConfigs { get; set; }

                public TableConfig GetTableConfig(string tableName)
                {
                    if (TableConfigs != null)
                    {
                        foreach (var tc in TableConfigs.OrderBy(z => z.TableName == null))
                        {
                            if (tc.TableName == null) return tc;
                            if (0 == string.Compare(tc.TableName, tableName, true)) return tc;
                        }
                    }
                    return null;
                }
            }

            public IList<Profile> Profiles { get; set; }

            public class TableConfig
            { 
                public string TableName { get; set; }
                public IDictionary<string, string> ComputedColumns { get; set; }
            }
        }

        private LoaderConfig Config_p;
        public LoaderConfig Config
        {
            get
            {
                Config_p = Config_p ?? ServiceProvider.GetRequiredService<IOptions<LoaderConfig>>().Value;
                return Config_p;
            }
        }

        private LoaderConfig.Profile Profile_p;
        public LoaderConfig.Profile Profile
        {
            get
            {
                Profile_p = Profile_p ?? Config.Profiles?.FirstOrDefault(p => 0 == string.Compare(p.ProfileName, ProfileName, true));
                return Profile_p;
            }
        }
    }
}
