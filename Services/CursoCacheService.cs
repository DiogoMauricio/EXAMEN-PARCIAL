using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using examen_parcial.Data;
using examen_parcial.Models;

namespace examen_parcial.Services
{
    public class CursoCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _context;
        private const string CURSOS_CACHE_KEY = "cursos_activos";
        private const int CACHE_EXPIRATION_SECONDS = 60;

        public CursoCacheService(IDistributedCache cache, ApplicationDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public async Task<List<Curso>> GetCursosActivosAsync()
        {
            // Intentar obtener desde cache
            var cachedCursos = await _cache.GetStringAsync(CURSOS_CACHE_KEY);
            
            if (!string.IsNullOrEmpty(cachedCursos))
            {
                try
                {
                    return JsonSerializer.Deserialize<List<Curso>>(cachedCursos) ?? new List<Curso>();
                }
                catch
                {
                    // Si hay error de deserialización, invalidar cache y continuar
                    await _cache.RemoveAsync(CURSOS_CACHE_KEY);
                }
            }

            // Si no está en cache, obtener de BD
            var cursos = await _context.Cursos
                .Where(c => c.Activo)
                .Include(c => c.Matriculas)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // Guardar en cache por 60 segundos (sin incluir las relaciones)
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_EXPIRATION_SECONDS)
            };

            try
            {
                // Crear una versión simplificada para cache (sin relaciones complejas)
                var cursosParaCache = cursos.Select(c => new Curso
                {
                    Id = c.Id,
                    Codigo = c.Codigo,
                    Nombre = c.Nombre,
                    Creditos = c.Creditos,
                    CupoMaximo = c.CupoMaximo,
                    HorarioInicio = c.HorarioInicio,
                    HorarioFin = c.HorarioFin,
                    Activo = c.Activo,
                    Matriculas = c.Matriculas // Las matriculas básicas para calcular cupos
                }).ToList();

                var cursosJson = JsonSerializer.Serialize(cursosParaCache, new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });
                await _cache.SetStringAsync(CURSOS_CACHE_KEY, cursosJson, cacheOptions);
            }
            catch
            {
                // Si falla la serialización, continuar sin cache
            }

            return cursos;
        }

        public async Task InvalidateCacheAsync()
        {
            await _cache.RemoveAsync(CURSOS_CACHE_KEY);
        }
    }
}