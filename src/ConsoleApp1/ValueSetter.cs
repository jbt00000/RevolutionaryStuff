using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;

namespace ConsoleApp1
{
    public class ValueSetter
    {
        public static PropertyInfo FindPropertyInfoFromPropertyName(Type t, Segment s)
        {
            foreach (var pi in t.GetPropertiesPublicInstanceReadWrite())
            {
                if (pi.Name == s.Name) return pi;
            }
            return null;
        }

        public static PropertyInfo FindPropertyInfoFromNewtonsoftJsonPropertyName(Type t, Segment s)
        {
            foreach (var pi in t.GetPropertiesPublicInstanceReadWrite())
            {
                var jp = pi.GetCustomAttribute<JsonPropertyAttribute>();
                if (jp != null)
                {
                    if (jp.PropertyName == s.Name) return pi;
                }
            }
            return FindPropertyInfoFromPropertyName(t, s);
        }

        public class ValueSetterSettings
        {
            public Func<Type, Segment, PropertyInfo> PropertyFinder { get; set; }
        }

        public static readonly ValueSetter NewtonsoftJsonSetter = new ValueSetter(new ValueSetterSettings { PropertyFinder = FindPropertyInfoFromNewtonsoftJsonPropertyName });
        public static readonly ValueSetter PocoSetter = new ValueSetter(new ValueSetterSettings { PropertyFinder = FindPropertyInfoFromPropertyName });

        private readonly ValueSetterSettings Settings;

        private ValueSetter(ValueSetterSettings settings)
        {
            Settings = settings;
        }

        public class Segment
        {
            public string Name { get; }
            public int? Index { get; }
            public bool IsList { get; }

            public Segment(string part)
            {
                part = StringHelpers.TrimOrNull(part);
                Requires.Text(part);
                if (StringHelpers.Split(part, "[", true, out string left, out string right))
                {
                    right = right.LeftOf("]").Trim();
                    Index = int.Parse(right);
                    IsList = true;
                }
                Name = left;
            }

            public static IList<Segment> CreateSegmentsFromJsonPath(string jsonPath)
                => jsonPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ConvertAll(s => new Segment(s));
        }

        public static bool SetFromJsonPath(object root, object val, string jsonPath)
            => NewtonsoftJsonSetter.Set(root, val, Segment.CreateSegmentsFromJsonPath(jsonPath));

        private PropertyInfo FindPropertyInfo(object root, Segment s)
        {
            var c = Cache.DataCacher;
            var t = root.GetType();
            return c.FindOrCreateValue(
                Cache.CreateKey(t, s.Name, s.IsList, nameof(ValueSetter), Settings.PropertyFinder),
                () => Settings.PropertyFinder(t, s));
        }

        public bool Set(object root, object val, IList<Segment> segments)
        {
            Requires.NonNull(root);
            Requires.ListArg(segments, nameof(segments), 1);

            var baseVar = root;
            for (int z = 0; z < segments.Count - 1; ++z)
            {
                var segment = segments[z];
                var pi = FindPropertyInfo(baseVar, segment);
                var p = pi.GetValue(baseVar);
                if (p == null)
                {
                    var pType = pi.PropertyType;
                    if (pType.IsGenericType && (pType.GetGenericTypeDefinition() == typeof(IList<>) || pType.GetGenericTypeDefinition() == typeof(List<>)))
                    {
                        var itemType = pType.GetGenericArguments().Single();
                        p = typeof(List<>).MakeGenericType(itemType).Construct();
                    }
                    else
                    {
                        p = pi.PropertyType.Construct();
                    }
                    pi.SetValue(baseVar, p);
                }
                if (segment.IsList)
                {
                    var itemType = pi.PropertyType.GetGenericArguments().Single();
                    p = SetListValue(p, segment, (itemType, itemDef, itemCurr) => {
                        if (itemDef == itemCurr) return itemType.Construct();
                        return itemCurr;
                    });
                }
                baseVar = p;
            }
            return Set(baseVar, val, segments.Last());
        }

        private bool Set(object root, object val, Segment segment)
        {
            Requires.NonNull(root);
            Requires.NonNull(segment);

            var pi = FindPropertyInfo(root, segment);
            if (segment.IsList)
            {
                var coll = pi.GetValue(root);
                if (coll == null)
                {
                    var itemType = pi.PropertyType.GetGenericArguments().Single();
                    coll = typeof(List<>).MakeGenericType(itemType).Construct();
                    pi.SetValue(root, coll);
                }
                SetListValue(coll, segment, (itemType, itemDef, itemCurr)
                    =>TypeHelpers.ConvertValue(itemType, val));
            }
            else
            {
                val = TypeHelpers.ConvertValue(pi.PropertyType, val);
                pi.SetValue(root, val);
            }
            return true;
        }

        private object SetListValue(object collection, Segment segment, Func<Type, object, object, object> valueGetter)
        {
            Requires.NonNull(collection);
            Requires.True(segment.IsList, nameof(segment.IsList));

            var collectionType = collection.GetType();
            var itemType = collectionType.GetGenericArguments().Single();
            var itemDefault = itemType.GetDefaultValue();
            var collectionCountPi = collectionType.GetProperty("Count");
            var collectionAddMi = collectionType.GetMethod("Add", new[] { itemType });
            for (int cnt=(int)collectionCountPi.GetValue(collection); cnt <= segment.Index; ++cnt)
            {
                collectionAddMi.Invoke(collection, new object[] { itemDefault });
            }
            var collectionNdx = collectionType.GetIndexer();
            var itemVal = collectionNdx.GetValue(collection, new object[] { segment.Index });
            itemVal = valueGetter(itemType, itemDefault, itemVal);
            collectionNdx.SetValue(collection, itemVal, new object[] { segment.Index });
            return itemVal;
        }
    }
}
