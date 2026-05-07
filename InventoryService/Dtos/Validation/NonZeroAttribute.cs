using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos.Validation
{
    public class NonZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is int intValue)
            {
                return intValue != 0;
            }
            return true;
        }
    }
}