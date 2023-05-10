namespace PeliculasAPI.DTOs
{
    public class PeliculaDetallesDTO:PeliculaDTO
    {
        public List<GeneroDTO> Generos { get; set; }
        public List<ActorPeliculasDetalleDTO> Actores { get; set; }
    }
}
