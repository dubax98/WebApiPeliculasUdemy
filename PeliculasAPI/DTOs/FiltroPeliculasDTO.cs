﻿namespace PeliculasAPI.DTOs
{
    public class FiltroPeliculasDTO
    {
        public int Pagina { get; set; } = 1;
        public int CantidadRegistrosPorpagina { get; set; } = 10;
        public PaginacionDTO Paginacion 
        {
            get { 
                return new PaginacionDTO() 
                { 
                    Pagina = Pagina, 
                    CantidadRegistrosPorPagina = CantidadRegistrosPorpagina 
                }; 
            }
        }

        public string Titulo { get; set; }
        public int GeneroId { get; set; }
        public bool EnCines { get; set; }
        public bool ProximosEstrenos { get; set; }
        public string CampoOrdenar { get; set; }
        public bool OrdenAscendente { get; set; } = true;
    }
}