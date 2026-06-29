namespace SIV.Shared.Exceptions;

public class VueloNoEncontradoException : Exception
{
    public VueloNoEncontradoException(Guid vueloId)
        : base($"No se encontró un vuelo con Id {vueloId}.") { }

    public VueloNoEncontradoException(string numeroVuelo)
        : base($"No se encontró un vuelo con número {numeroVuelo}.") { }
}
