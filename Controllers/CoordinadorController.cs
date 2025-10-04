using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using examen_parcial.Data;
using examen_parcial.Models;
using examen_parcial.Services;

namespace examen_parcial.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CursoCacheService _cacheService;

        public CoordinadorController(ApplicationDbContext context, CursoCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        // GET: /Coordinador - Panel principal
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCursos = await _context.Cursos.CountAsync();
            ViewBag.CursosActivos = await _context.Cursos.CountAsync(c => c.Activo);
            ViewBag.MatriculasPendientes = await _context.Matriculas.CountAsync(m => m.Estado == EstadoMatricula.Pendiente);
            ViewBag.MatriculasConfirmadas = await _context.Matriculas.CountAsync(m => m.Estado == EstadoMatricula.Confirmada);

            return View();
        }

        // GET: /Coordinador/Cursos - Lista de todos los cursos
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Matriculas)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(cursos);
        }

        // GET: /Coordinador/CrearCurso
        public IActionResult CrearCurso()
        {
            return View();
        }

        // POST: /Coordinador/CrearCurso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                // Verificar que el código no exista
                var existeCodigo = await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo);
                if (existeCodigo)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                    return View(curso);
                }

                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();

                // Invalidar cache
                await _cacheService.InvalidateCacheAsync();

                TempData["Success"] = $"Curso '{curso.Nombre}' creado exitosamente.";
                return RedirectToAction(nameof(Cursos));
            }

            return View(curso);
        }

        // GET: /Coordinador/EditarCurso/5
        public async Task<IActionResult> EditarCurso(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // POST: /Coordinador/EditarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id, Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verificar que el código no exista en otro curso
                var existeCodigo = await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo && c.Id != curso.Id);
                if (existeCodigo)
                {
                    ModelState.AddModelError("Codigo", "Ya existe otro curso con este código.");
                    return View(curso);
                }

                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();

                    // Invalidar cache
                    await _cacheService.InvalidateCacheAsync();

                    TempData["Success"] = $"Curso '{curso.Nombre}' actualizado exitosamente.";
                    return RedirectToAction(nameof(Cursos));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(curso);
        }

        // POST: /Coordinador/DesactivarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = !curso.Activo;
            await _context.SaveChangesAsync();

            // Invalidar cache
            await _cacheService.InvalidateCacheAsync();

            var estado = curso.Activo ? "activado" : "desactivado";
            TempData["Success"] = $"Curso '{curso.Nombre}' {estado} exitosamente.";

            return RedirectToAction(nameof(Cursos));
        }

        // GET: /Coordinador/Matriculas - Lista de todas las matrículas
        public async Task<IActionResult> Matriculas(int? cursoId)
        {
            var matriculasQuery = _context.Matriculas
                .Include(m => m.Curso)
                .Include(m => m.Usuario)
                .AsQueryable();

            if (cursoId.HasValue)
            {
                matriculasQuery = matriculasQuery.Where(m => m.CursoId == cursoId.Value);
                var curso = await _context.Cursos.FindAsync(cursoId.Value);
                ViewBag.Curso = curso;
                ViewBag.TituloSeccion = $"Matrículas del Curso: {curso?.Nombre}";
            }
            else
            {
                ViewBag.TituloSeccion = "Todas las Matrículas del Sistema";
            }

            var matriculas = await matriculasQuery
                .OrderByDescending(m => m.FechaMatricula)
                .ToListAsync();

            ViewBag.Cursos = await _context.Cursos.ToListAsync();

            return View(matriculas);
        }

        // POST: /Coordinador/CambiarEstadoMatricula
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoMatricula(int matriculaId, EstadoMatricula nuevoEstado)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == matriculaId);

            if (matricula == null)
            {
                return NotFound();
            }

            // Validar cupo si se está confirmando
            if (nuevoEstado == EstadoMatricula.Confirmada && matricula.Estado != EstadoMatricula.Confirmada)
            {
                var matriculasConfirmadas = await _context.Matriculas
                    .CountAsync(m => m.CursoId == matricula.CursoId && 
                        m.Estado == EstadoMatricula.Confirmada && m.Id != matricula.Id);

                if (matriculasConfirmadas >= matricula.Curso.CupoMaximo)
                {
                    TempData["Error"] = "No se puede confirmar: el curso ha alcanzado su cupo máximo.";
                    return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
                }
            }

            matricula.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            var accion = nuevoEstado switch
            {
                EstadoMatricula.Confirmada => "confirmada",
                EstadoMatricula.Cancelada => "cancelada",
                EstadoMatricula.Pendiente => "puesta en estado pendiente",
                _ => "actualizada"
            };

            TempData["Success"] = $"Matrícula de {matricula.Usuario.Email} {accion} exitosamente.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
        }

        private async Task<bool> CursoExists(int id)
        {
            return await _context.Cursos.AnyAsync(e => e.Id == id);
        }
    }
}