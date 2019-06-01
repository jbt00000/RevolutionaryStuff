using System;

namespace RevolutionaryStuff.Core
{
    public static class EnumHelpers
    {
        public static T Random<T>(Random r = null) where T : Enum
        {
            var a = Enum.GetValues(typeof(T));
            var n = (r ?? Stuff.Random).Next(a.Length);
            return (T)a.GetValue(n);
        }
    }
}
