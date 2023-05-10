using AutoMapper;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entities;

namespace PeliculasAPI.Helpers
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory) 
        {         
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<CrearGeneroDTO, Genero>().ReverseMap();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<CrearActorDTO, Actor>().ForMember(x => x.Foto, options => options.Ignore());
            CreateMap<ActorPatchDTO, Actor>().ReverseMap();

            CreateMap<Pelicula, PeliculaDTO>().ReverseMap();
            CreateMap<CrearPeliculaDTO, Pelicula>()
                .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.PeliculasGeneros, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapPeliculasActores));
            CreateMap<PeliculaPatchDTO, Pelicula>().ReverseMap();

            CreateMap<Pelicula, PeliculaDetallesDTO>()
                .ForMember(x => x.Generos, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.Actores, options => options.MapFrom(MapPeliculasActores));

            CreateMap<SalaCine, SalaCineDTO>()
                .ForMember(x => x.Latitud, x => x.MapFrom(y => y.Ubicacion.Y))
                .ForMember(x => x.Longitud, x => x.MapFrom(y => y.Ubicacion.X));

            CreateMap<SalaCineDTO, SalaCine>().ForMember(x => x.Ubicacion, x => x.MapFrom(y => geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));          
            CreateMap<CrearSalaCineDTO, SalaCine>().ForMember(x => x.Ubicacion, x => x.MapFrom(y => geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

        }

        private List<ActorPeliculasDetalleDTO> MapPeliculasActores(Pelicula pelicula, PeliculaDetallesDTO peliculaDetallesDTO)
        {
            var resultado = new List<ActorPeliculasDetalleDTO>();
            if (pelicula.PeliculasActores == null)
                return resultado;

            foreach (var actorPelicula in pelicula.PeliculasActores)
            {
                resultado.Add(new ActorPeliculasDetalleDTO()
                {
                    ActorId = actorPelicula.ActorId,
                    Personaje = actorPelicula.Personaje,
                    Nombre = actorPelicula.Actor.Nombre
                });
            }

            return resultado;
        }

        private List<GeneroDTO> MapPeliculasGeneros (Pelicula pelicula, PeliculaDetallesDTO peliculaDetallesDTO)
        {
            var resultado = new List<GeneroDTO>();
            if(pelicula.PeliculasGeneros == null)
                return resultado;

            foreach (var generoPelicula in pelicula.PeliculasGeneros)
                resultado.Add(new GeneroDTO() { Id = generoPelicula.GeneroId, Nombre = generoPelicula.Genero.Nombre });

            return resultado;
        }

        private List<PeliculasGeneros> MapPeliculasGeneros(CrearPeliculaDTO crearPeliculaDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasGeneros>();
            if(crearPeliculaDTO == null) 
                return resultado;

            foreach(var id in crearPeliculaDTO.GenerosIDs)
                resultado.Add(new PeliculasGeneros() { GeneroId = id });

            return resultado;
        }

        private List<PeliculasActores> MapPeliculasActores(CrearPeliculaDTO crearPeliculaDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasActores>();
            if (crearPeliculaDTO == null)
                return resultado;

            foreach (var actor in crearPeliculaDTO.Actores)
                resultado.Add(new PeliculasActores() { ActorId = actor.ActorId, Personaje = actor.Personaje });

            return resultado;
        }
    }
}
