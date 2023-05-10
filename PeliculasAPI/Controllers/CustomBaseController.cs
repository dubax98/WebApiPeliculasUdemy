using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entities;
using PeliculasAPI.Helpers;

namespace PeliculasAPI.Controllers
{
    public class CustomBaseController:ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;

        public CustomBaseController(ApplicationDBContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        protected async Task<List<TDTO>> Get<Tentidad, TDTO>() where Tentidad : class
        {
            var entidades = await _context.Set<Tentidad>().AsNoTracking().ToListAsync();
            var dtos = _mapper.Map<List<TDTO>>(entidades);
            return dtos;
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDTO paginacionDTO) where TEntidad : class
        {
            var queryable = _context.Set<TEntidad>().AsQueryable();
            await HttpContext.InsertarParametrosPaginacion(queryable, paginacionDTO.CantidadRegistrosPorPagina);
            var entidad = await queryable.Paginar(paginacionDTO).ToListAsync();
            return _mapper.Map<List<TDTO>>(entidad);
        }

        protected async Task<ActionResult<TDTO>> Get<Tentidad, TDTO>(int id) where Tentidad : class, IId
        {
            var entidad = await _context.Set<Tentidad>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
                return NotFound();

            var dto = _mapper.Map<TDTO>(entidad);
            return dto;
        }

        protected async Task<ActionResult> Post <TCreacion, TEntidad, TLectura> (TCreacion creacionDTO, string nombreRuta) where TEntidad:class, IId
        {
            var entidad = _mapper.Map<TEntidad>(creacionDTO);
            _context.Add(entidad);
            await _context.SaveChangesAsync();
            var dtoLectura = _mapper.Map<TLectura>(entidad);

            return new CreatedAtRouteResult(nombreRuta, new { id = entidad.Id }, dtoLectura);
        }

        protected async Task<ActionResult> Put<TCreacion, TEntidad> (int id, TCreacion creacion) where TEntidad:class, IId
        {
            var entidad = _mapper.Map<TEntidad>(creacion);
            entidad.Id = id;
            _context.Entry(entidad).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Patch<TEntidad, TDTO> (int id, JsonPatchDocument<TDTO> patchDocument) 
            where TDTO : class 
            where TEntidad:class, IId
        {
            if (patchDocument == null)
                return BadRequest();

            var entidad = await _context.Set<TEntidad>().FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
                return NotFound();

            var dto = _mapper.Map<TDTO>(entidad);

            patchDocument.ApplyTo(dto, ModelState);

            var esValido = TryValidateModel(dto);

            if (!esValido)
                return BadRequest(ModelState);

            _mapper.Map(dto, entidad);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Delete<TEntidad> (int id) where TEntidad:class, IId, new()
        {
            var existe = await _context.Set<TEntidad>().AnyAsync(x => x.Id == id);

            if (!existe)
                return NotFound();

            _context.Remove(new TEntidad() { Id = id });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
