using RevolutionaryStuff.Core.ApplicationParts;
using System;
using System.Collections.Generic;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IValidate
    {
        void Validate();
    }
}

namespace RevolutionaryStuff.Core
{
    public static class IValidateExtensions
    {
        public static void ValidateAll(this IEnumerable<IValidate> validators)
        {
            foreach (var v in validators)
            {
                v.Validate();
            }
        }

        public static bool IsValid(this IValidate v)
        {
            Requires.NonNull(v, "v");
            try
            {
                v.Validate();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
