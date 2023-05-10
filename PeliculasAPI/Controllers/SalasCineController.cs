using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entities;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/SalasCine")]
    public class SalasCineController:CustomBaseController
    {
        private readonly ApplicationDBContext _context;
        private readonly GeometryFactory _geometryFactory;

        public SalasCineController(ApplicationDBContext context, IMapper mapper, GeometryFactory geometryFactory) : base(context, mapper) 
        {
            this._context = context;
            this._geometryFactory = geometryFactory;
        }

        [HttpGet]
        public async Task<ActionResult<List<SalaCineDTO>>> Get()
        {
            return await Get<SalaCine, SalaCineDTO>();
        }

        [HttpGet("{id:int}", Name = "ObtenerSalaCine")]
        public async Task<ActionResult<SalaCineDTO>> Get(int id)
        {
            return await Get<SalaCine, SalaCineDTO>(id);
        }

        [HttpGet("Cercanos")]
        public async Task<List<SalaCineCercanoDTO>> Cercanos([FromQuery] SalaCineCercanoFiltroDTO filtro)
        {
            var ubicacionUsuario = _geometryFactory.CreatePoint(new Coordinate(filtro.Longitud, filtro.Latitud));
            var salasCines = await _context.SalasDeCine
                .OrderBy(x => x.Ubicacion.Distance(ubicacionUsuario))
                .Where(x => x.Ubicacion.IsWithinDistance(ubicacionUsuario, filtro.DistanciaEnKms * 1000))
                .Select(x => new SalaCineCercanoDTO
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Latitud = x.Ubicacion.Y,
                    Longitud = x.Ubicacion.X,
                    DistanciaEnMetros = Math.Round(x.Ubicacion.Distance(ubicacionUsuario))
                }).ToListAsync();

            return salasCines;
        }

        public async Task<ActionResult> Post([FromBody] CrearSalaCineDTO crearSalaCineDTO)
        {
            return await Post<CrearSalaCineDTO, SalaCine, SalaCineDTO>(crearSalaCineDTO, "ObtenerSalaCine");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] CrearSalaCineDTO crearSalaCineDTO)
        {
            return await Put<CrearSalaCineDTO, SalaCine>(id, crearSalaCineDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<SalaCine>(id);
        }
    }
}
