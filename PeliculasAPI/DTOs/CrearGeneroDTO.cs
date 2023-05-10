using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class CrearGeneroDTO
    {
        [Required]
        [StringLength(40)]
        public string Nombre { get; set; }
    }
}
