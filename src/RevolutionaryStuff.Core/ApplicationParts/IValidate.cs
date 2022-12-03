using RevolutionaryStuff.Core.ApplicationParts;

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
            ArgumentNullException.ThrowIfNull(v, "v");
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
