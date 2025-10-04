using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using examen_parcial.Data;
using examen_parcial.Models;
using Microsoft.AspNetCore.Authorization;

namespace examen_parcial.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cursos - Catálogo de cursos con filtros
        public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, 
                                              TimeSpan? horarioInicio, TimeSpan? horarioFin)
        {
            ViewBag.NombreFiltro = nombre;
            ViewBag.CreditosMinFiltro = creditosMin;
            ViewBag.CreditosMaxFiltro = creditosMax;
            ViewBag.HorarioInicioFiltro = horarioInicio?.ToString(@"hh\:mm");
            ViewBag.HorarioFinFiltro = horarioFin?.ToString(@"hh\:mm");

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

            var cursos = await cursosQuery.OrderBy(c => c.Nombre).ToListAsync();

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

            // Verificar si el usuario actual ya está matriculado
            ViewBag.YaMatriculado = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                ViewBag.YaMatriculado = await _context.Matriculas
                    .AnyAsync(m => m.CursoId == id && m.UsuarioId == usuarioId);
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
                return RedirectToAction(nameof(Index));
            }

            var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (usuarioId == null)
            {
                return Unauthorized();
            }

            // Verificar si ya está matriculado
            var matriculaExistente = await _context.Matriculas
                .AnyAsync(m => m.CursoId == id && m.UsuarioId == usuarioId);

            if (matriculaExistente)
            {
                TempData["Warning"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Details", new { id });
            }

            // Verificar cupo disponible
            var matriculasConfirmadas = await _context.Matriculas
                .CountAsync(m => m.CursoId == id && m.Estado == EstadoMatricula.Confirmada);

            if (matriculasConfirmadas >= curso.CupoMaximo)
            {
                TempData["Error"] = "No hay cupos disponibles para este curso.";
                return RedirectToAction("Details", new { id });
            }

            // Crear nueva matrícula
            var matricula = new Matricula
            {
                CursoId = id,
                UsuarioId = usuarioId,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Confirmada
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Te has inscrito exitosamente en el curso '{curso.Nombre}'.";
            return RedirectToAction("Details", new { id });
        }
    }
}