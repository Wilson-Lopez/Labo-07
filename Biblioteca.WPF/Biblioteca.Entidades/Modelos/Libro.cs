using System;

namespace Biblioteca.Entidades.Modelos
{
    public class Libro
    {
        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string ISBN { get; set; }
        public int AutorId { get; set; }
        public int Ejemplares { get; set; }
        public bool Activo { get; set; }

        // Propiedad auxiliar para mostrar el nombre del autor en WPF
        // (requisito 12: "mostrando el nombre del autor")
        public string NombreAutor { get; set; }
    }
}