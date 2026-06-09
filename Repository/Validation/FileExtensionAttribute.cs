using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName); //hinhanh.jpg
                string[] extensions = {"jpg", "png", "jpeg"};


                bool result = extensions.Any(x => extension.EndsWith(x));

                if (!result)
                {
                    return new ValidationResult("Chỉ chấp nhận các file có đuôi jpg, png hoặc jpeg.");
                }    
            }

            return ValidationResult.Success;
        }
    }
}
