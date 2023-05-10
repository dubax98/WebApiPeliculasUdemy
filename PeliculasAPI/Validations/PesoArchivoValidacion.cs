using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Validations
{
    public class PesoArchivoValidacion:ValidationAttribute
    {
        private readonly int pesoMaximoMegaBytes;

        public PesoArchivoValidacion(int PesoMaximoMegaBytes)
        {
            pesoMaximoMegaBytes = PesoMaximoMegaBytes;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            IFormFile formFile = value as IFormFile;

            if(formFile == null)
                return ValidationResult.Success;

            if (formFile.Length > (pesoMaximoMegaBytes * 1024 * 1024))
                return new ValidationResult($"El peso máximmo del archivo no puede ser superior a {pesoMaximoMegaBytes}mb");

            return ValidationResult.Success;
        }
    }
}
