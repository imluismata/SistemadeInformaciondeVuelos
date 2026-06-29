namespace SIV.Shared.Exceptions;

public sealed class VueloNoEncontradoException : Exception
{
    public VueloNoEncontradoException(Guid vueloId)
        : base($"No se encontró el vuelo con identificador {vueloId}.")
    {
    }
}