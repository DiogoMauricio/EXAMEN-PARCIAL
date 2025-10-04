using System.ComponentModel.DataAnnotations;

namespace examen_parcial.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "Los créditos deben ser mayor a 0")]
        public int Creditos { get; set; }

        [Range(1, 100, ErrorMessage = "El cupo máximo debe ser mayor a 0")]
        public int CupoMaximo { get; set; }

        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

        // Propiedad calculada para cupos disponibles (excluyendo pendientes y confirmadas)
        public int CuposDisponibles => CupoMaximo - Matriculas.Count(m => 
            m.Estado == EstadoMatricula.Confirmada || m.Estado == EstadoMatricula.Pendiente);
    }
}