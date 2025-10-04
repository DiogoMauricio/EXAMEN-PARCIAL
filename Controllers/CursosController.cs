using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examen_parcial.Data;
using examen_parcial.Models;
using Microsoft.AspNetCore.Authorization;
using examen_parcial.Services;

namespace examen_parcial.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CursoCacheService _cacheService;

        public CursosController(ApplicationDbContext context, CursoCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        // GET: Cursos - Catálogo de cursos con filtros (usando cache)
        public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, 
                                              TimeSpan? horarioInicio, TimeSpan? horarioFin)
        {
            ViewBag.NombreFiltro = nombre;
            ViewBag.CreditosMinFiltro = creditosMin;
            ViewBag.CreditosMaxFiltro = creditosMax;
            ViewBag.HorarioInicioFiltro = horarioInicio?.ToString(@"hh\:mm");
            ViewBag.HorarioFinFiltro = horarioFin?.ToString(@"hh\:mm");

            // Usar cache si no hay filtros aplicados
            List<Curso> cursos;
            if (string.IsNullOrEmpty(nombre) && !creditosMin.HasValue && !creditosMax.HasValue && 
                !horarioInicio.HasValue && !horarioFin.HasValue)
            {
                cursos = await _cacheService.GetCursosActivosAsync();
            }
            else
            {
                // Si hay filtros, consulta directa a BD
                IQueryable<Curso> cursosQuery = _context.Cursos
                    .Where(c => c.Activo)
                    .Include(c => c.Matriculas);

                // Filtro por nombre
                if (!string.IsNullOrEmpty(nombre))
                {
                    cursosQuery = cursosQuery.Where(c => c.Nombre.Contains(nombre) || c.Codigo.Contains(nombre));
                }

                // Filtro por rango de créditos
                if (creditosMin.HasValue)
                {
                    cursosQuery = cursosQuery.Where(c => c.Creditos >= creditosMin.Value);
                }

                if (creditosMax.HasValue)
                {
                    cursosQuery = cursosQuery.Where(c => c.Creditos <= creditosMax.Value);
                }

                // Filtro por horario
                if (horarioInicio.HasValue)
                {
                    cursosQuery = cursosQuery.Where(c => c.HorarioInicio >= horarioInicio.Value);
                }

                if (horarioFin.HasValue)
                {
                    cursosQuery = cursosQuery.Where(c => c.HorarioFin <= horarioFin.Value);
                }

                cursos = await cursosQuery.OrderBy(c => c.Nombre).ToListAsync();
            }

            return View(cursos);
        }

        // GET: Cursos/Details/5 - Vista detalle del curso
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (curso == null)
            {
                return NotFound();
            }

            // GUARDAR EL ÚLTIMO CURSO VISITADO EN SESIÓN
            HttpContext.Session.SetString("UltimoCursoVisitado", curso.Nombre);
            HttpContext.Session.SetInt32("UltimoCursoVisitadoId", curso.Id);

            // Verificar el estado de matrícula del usuario actual
            ViewBag.YaMatriculado = false;
            ViewBag.EstadoMatricula = "";
            if (User.Identity?.IsAuthenticated == true)
            {
                var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var matriculaUsuario = await _context.Matriculas
                    .FirstOrDefaultAsync(m => m.CursoId == id && m.UsuarioId == usuarioId);
                
                if (matriculaUsuario != null)
                {
                    ViewBag.YaMatriculado = true;
                    ViewBag.EstadoMatricula = matriculaUsuario.Estado.ToString();
                }
            }

            return View(curso);
        }

        // POST: Cursos/Inscribirse/5 - Inscribirse en un curso
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null || !curso.Activo)
            {
                TempData["Error"] = "El curso no existe o no está activo.";
                return RedirectToAction("Details", new { id });
            }

            var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (usuarioId == null)
            {
                TempData["Error"] = "Debe estar autenticado para inscribirse.";
                return RedirectToAction("Details", new { id });
            }

            // VALIDACIÓN 1: Verificar si ya está matriculado
            var matriculaExistente = await _context.Matriculas
                .AnyAsync(m => m.CursoId == id && m.UsuarioId == usuarioId);

            if (matriculaExistente)
            {
                TempData["Warning"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Details", new { id });
            }

            // VALIDACIÓN 2: Verificar cupo máximo (incluyendo pendientes y confirmadas)
            var matriculasOcupadas = await _context.Matriculas
                .CountAsync(m => m.CursoId == id && 
                    (m.Estado == EstadoMatricula.Confirmada || m.Estado == EstadoMatricula.Pendiente));

            if (matriculasOcupadas >= curso.CupoMaximo)
            {
                TempData["Error"] = "No hay cupos disponibles para este curso.";
                return RedirectToAction("Details", new { id });
            }

            // VALIDACIÓN 3: Verificar solapamiento de horarios
            var cursosMatriculados = await _context.Matriculas
                .Where(m => m.UsuarioId == usuarioId && 
                    (m.Estado == EstadoMatricula.Confirmada || m.Estado == EstadoMatricula.Pendiente))
                .Include(m => m.Curso)
                .Select(m => m.Curso)
                .ToListAsync();

            var horarioSolapa = cursosMatriculados.Any(c => 
                (curso.HorarioInicio < c.HorarioFin && curso.HorarioFin > c.HorarioInicio));

            if (horarioSolapa)
            {
                var cursoConflicto = cursosMatriculados.First(c => 
                    curso.HorarioInicio < c.HorarioFin && curso.HorarioFin > c.HorarioInicio);
                
                TempData["Error"] = $"Conflicto de horario con el curso '{cursoConflicto.Nombre}' " +
                    $"({cursoConflicto.HorarioInicio:hh\\:mm} - {cursoConflicto.HorarioFin:hh\\:mm}).";
                return RedirectToAction("Details", new { id });
            }

            // Crear nueva matrícula en estado PENDIENTE
            var matricula = new Matricula
            {
                CursoId = id,
                UsuarioId = usuarioId,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente // ESTADO PENDIENTE según requerimiento
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tu solicitud de inscripción al curso '{curso.Nombre}' ha sido enviada y está pendiente de aprobación.";
            return RedirectToAction("Details", new { id });
        }

        // Método para invalidar cache (se usaría en create/edit de cursos)
        public async Task<IActionResult> InvalidateCache()
        {
            await _cacheService.InvalidateCacheAsync();
            TempData["Success"] = "Cache de cursos invalidado correctamente.";
            return RedirectToAction("Index");
        }
    }
}