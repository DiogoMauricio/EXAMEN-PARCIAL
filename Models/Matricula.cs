using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace examen_parcial.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public DateTime FechaMatricula { get; set; } = DateTime.Now;

        [Required]
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;

        // Relaciones
        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual IdentityUser Usuario { get; set; } = null!;

        // Propiedades calculadas para facilitar el acceso en las vistas
        public string NombreEstudiante => Usuario?.UserName ?? "Usuario desconocido";
        public string EmailEstudiante => Usuario?.Email ?? "Email no disponible";
    }
}