namespace SIV.Shared.Exceptions;

public sealed class VueloNoEncontradoException : Exception
{
    public VueloNoEncontradoException(Guid vueloId)
        : base($"No se encontró el vuelo con identificador {vueloId}.")
    {
    }

    public VueloNoEncontradoException(string numeroVuelo)
        : base($"No se encontró un vuelo con número {numeroVuelo}.") { }
}
