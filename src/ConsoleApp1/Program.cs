using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;

namespace ConsoleApp1
{
    class Program
    {
        private static void WriteLine(object o)
        {
            Console.WriteLine(o);
            Trace.WriteLine(o);
        }

        public class Outer
        {
            [JsonProperty("i")]
            public int I1 { get; set; }

            [JsonProperty("s")]
            public string S1 { get; set; }

            [JsonProperty("name")]
            public Name Name { get; set; }

            [JsonProperty("is")]
            public IList<int> Ints { get; set; }

            [JsonProperty("ss")]
            public IList<string> Strings { get; set; }

            [JsonProperty("names")]
            public IList<Name> Names { get; set; }
        }

        public class Name
        {
            [JsonProperty("f")]
            public string First { get; set; }

            [JsonProperty("l")]
            public string Last { get; set; }
        }



        static void Main(string[] args)
        {
            Dictionary<string, string> d = new();
            d["a"] = "b";
            var c = WebHelpers.CreateHttpContent(d);
            Stuff.Noop(c);




            WriteLine("Hello World!");
            try
            {
                var root = new Outer();
                foreach (var kvp in new Dictionary<string, object> {
                    { "/s", "my string" },
                    { "/i", 7},
                    { "/is[3]", 3},
                    { "/is[9]", 9},
                    { "/ss[5]", "five"},
                    { "/ss[0]", "zero"},
                    { "/name/f", "jason"},
                    { "/name/l", "thomas"},
                    { "/names[2]/l", "l2"},
                    { "/names[1]/l", "l1"},
                    { "/names[0]/l", "l0"},
                })
                {
                    var segments = ValueSetter.Segment.CreateSegmentsFromJsonPath(kvp.Key);
                    ValueSetter.NewtonsoftJsonSetter.Set(root, kvp.Value, segments);
                }
                WriteLine(root);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }
}
