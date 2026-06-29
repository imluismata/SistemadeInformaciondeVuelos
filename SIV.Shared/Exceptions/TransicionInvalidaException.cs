namespace SIV.Shared.Exceptions;

public class TransicionInvalidaException : Exception
{
    public TransicionInvalidaException(string mensaje) : base(mensaje) { }

    public TransicionInvalidaException(string estadoOrigen, string estadoDestino)
        : base($"Transición inválida de '{estadoOrigen}' a '{estadoDestino}'.") { }
}
