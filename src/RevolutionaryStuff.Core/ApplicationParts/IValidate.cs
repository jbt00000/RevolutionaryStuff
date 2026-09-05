using System.ComponentModel.DataAnnotations;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IValidate
    {
        void Validate();

        IEnumerable<ValidationResult> GetValidationResults(ValidationContext validationContext)
        {
            try
            {
                Validate();
                return [];
            }
            catch (AggregateException ex)
            {

                return ex.InnerExceptions
                    .Select(z => z is ValidationException vex ? vex.ValidationResult : new ValidationResult(z.Message, [GetType().Name]))
                    .ToList();
            }
            catch (ValidationException vex)
            {
                return [vex.ValidationResult];
            }
            catch (Exception ex)
            {
                return [new ValidationResult(ex.Message, [GetType().Name])];
            }
        }
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
