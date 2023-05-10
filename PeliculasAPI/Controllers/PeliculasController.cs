using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entities;
using PeliculasAPI.Helpers;
using PeliculasAPI.Services;
using System.Linq.Dynamic.Core;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController:CustomBaseController
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos _almacenadorArchivos;
        private readonly ILogger<PeliculasController> _logger;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDBContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos, ILogger<PeliculasController> logger) : base(context, mapper)
        {
            this._context = context;
            this._mapper = mapper;
            this._almacenadorArchivos = almacenadorArchivos;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PeliculasIndexDTO>> Get()
        {

            var top = 5;
            var hoy = DateTime.Today;

            var proximosEstrenos = await _context.Peliculas
                .Where(x => x.FechaEstreno > hoy)
                .OrderBy(x=>x.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var enCines = await _context.Peliculas
                .Where(x => x.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDTO();
            resultado.FututosEstrenos = _mapper.Map<List<PeliculaDTO>>(proximosEstrenos);
            resultado.EnCines = _mapper.Map<List<PeliculaDTO>>(enCines);

            return resultado;

            //var entidades = await _context.Peliculas.ToListAsync();
            //var dtos = _mapper.Map<List<PeliculaDTO>>(entidades);

            //return dtos;
        }

        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] FiltroPeliculasDTO filtroPeliculasDTO)
        {
            var peliculasQueryable =_context.Peliculas.AsQueryable();

            if(!string.IsNullOrEmpty(filtroPeliculasDTO.Titulo))
                peliculasQueryable = peliculasQueryable.Where(x => x.Titulo.Contains(filtroPeliculasDTO.Titulo));

            if (filtroPeliculasDTO.EnCines)
                peliculasQueryable = peliculasQueryable.Where(x => x.EnCines);

            if (filtroPeliculasDTO.ProximosEstrenos)
                peliculasQueryable = peliculasQueryable.Where(x => x.FechaEstreno > DateTime.Today);

            if (filtroPeliculasDTO.GeneroId != 0)
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.PeliculasGeneros
                    .Select(x => x.GeneroId).
                    Contains(filtroPeliculasDTO.GeneroId));

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.CampoOrdenar))
            {
                var tipoOrden = filtroPeliculasDTO.OrdenAscendente ? "ascending" : "descending";

                try
                {
                    peliculasQueryable = peliculasQueryable.OrderBy($"{filtroPeliculasDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }           

            await HttpContext.InsertarParametrosPaginacion(peliculasQueryable, filtroPeliculasDTO.CantidadRegistrosPorpagina);
            var peliculas = await peliculasQueryable.Paginar(filtroPeliculasDTO.Paginacion).ToListAsync();

            var dtos = _mapper.Map<List<PeliculaDTO>>(peliculas);
            return dtos;
        }

        [HttpGet("{id:int}", Name = "ObtenerPelicula")]
        public async Task<ActionResult<PeliculaDetallesDTO>> Get(int id)
        {
            var entidad = await _context.Peliculas
                .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(entidad == null)
                return NotFound();

            entidad.PeliculasActores = entidad.PeliculasActores.OrderBy(x => x.Orden).ToList();

            var dto = _mapper.Map<PeliculaDetallesDTO>(entidad);

            return dto;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] CrearPeliculaDTO crearPeliculaDTO)
        {
            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDTO);

            if (crearPeliculaDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await crearPeliculaDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(crearPeliculaDTO.Poster.FileName);
                    pelicula.Poster = await _almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor, crearPeliculaDTO.Poster.ContentType);
                }
            }
            _context.Add(pelicula);
            await _context.SaveChangesAsync();
            var dto = _mapper.Map<PeliculaDTO>(pelicula);

            return new CreatedAtRouteResult("ObtenerPelicula", new { id = pelicula.Id }, dto);
        }

        private void AsignarOrdenActores(Pelicula pelicula)
        {
            if(pelicula.PeliculasActores != null)
            {
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] CrearPeliculaDTO crearPeliculaDTO)
        {
            var pelicula = await _context.Peliculas.Include(x => x.PeliculasActores).Include(x => x.PeliculasGeneros).FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula == null)
                return NotFound();

            pelicula = _mapper.Map(crearPeliculaDTO, pelicula);

            if (crearPeliculaDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await crearPeliculaDTO.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(crearPeliculaDTO.Poster.FileName);
                    pelicula.Poster = await _almacenadorArchivos.EditarArchivo(contenido, extension, contenedor, pelicula.Poster, crearPeliculaDTO.Poster.ContentType);
                }
            }

            AsignarOrdenActores(pelicula);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PeliculaPatchDTO> patchDocument)
        {
            return await Patch<Pelicula, PeliculaPatchDTO>(id, patchDocument);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Pelicula>(id);
        }
    }
}
