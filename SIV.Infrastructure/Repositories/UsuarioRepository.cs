using Microsoft.EntityFrameworkCore;
using SIV.Modules.Usuarios.Application;
using SIV.Modules.Usuarios.Domain;

namespace SIV.Infrastructure.Repositories;

internal class UsuarioRepository : IUsuarioRepository
{
    private readonly SivDbContext _context;

    public UsuarioRepository(SivDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
    }

    public async Task<Usuario?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids)
    {
        return await _context.Usuarios
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamano)
    {
        // Ordeno por fecha para que las paginas salgan siempre en el mismo orden.
        var query = _context.Usuarios.OrderBy(u => u.CreadoEn);
        var total = await query.CountAsync();      // cuantos usuarios hay en total
        var items = await query
            .Skip((pagina - 1) * tamano)   // me salto los de las paginas anteriores
            .Take(tamano)                  // y agarro solo los de esta pagina
            .ToListAsync();
        return (items, total);
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await Task.CompletedTask;
    }

    public async Task EliminarAsync(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
        await Task.CompletedTask;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
