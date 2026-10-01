using System;

namespace Biblioteca.Negocio.Excepciones
{
   
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException() : base() { }

        public ReglaNegocioException(string mensaje) : base(mensaje) { }

        public ReglaNegocioException(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }
    }
}