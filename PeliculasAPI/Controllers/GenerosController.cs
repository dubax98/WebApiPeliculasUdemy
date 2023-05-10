using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entities;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/generos")]
    public class GenerosController:CustomBaseController
    {
        public GenerosController(ApplicationDBContext context, IMapper mapper) : base(context,mapper)
        {
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get()
        {
            return await Get<Genero, GeneroDTO>();
        }

        [HttpGet("{id:int}", Name = "obtenerGenero")]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {            
            return await Get<Genero, GeneroDTO>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CrearGeneroDTO crearGeneroDTO)
        {           
            return await Post<CrearGeneroDTO, Genero, GeneroDTO>(crearGeneroDTO, "obtenerGenero");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put (int id, [FromBody] CrearGeneroDTO crearGeneroDTO)
        {
            return await Put<CrearGeneroDTO, Genero>(id, crearGeneroDTO);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete (int id)
        {
            return await Delete<Genero>(id);
        }
    }
}
