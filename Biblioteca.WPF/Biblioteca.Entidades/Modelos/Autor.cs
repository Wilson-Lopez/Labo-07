using System;

namespace Biblioteca.Entidades.Modelos
{
    public class Autor
    {
        public int AutorId { get; set; }
        public string Nombre { get; set; }
        public string Nacionalidad { get; set; }
        public bool Activo { get; set; }
    }
}