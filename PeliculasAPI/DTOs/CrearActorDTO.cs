using Microsoft.AspNetCore.Mvc;
using PeliculasAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class CrearActorDTO:ActorPatchDTO
    {
        [PesoArchivoValidacion(PesoMaximoMegaBytes: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo:GrupoTipoArchivo.Imagen)]
        public IFormFile Foto { get; set; }
    }
}
