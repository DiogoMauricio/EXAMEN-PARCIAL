using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using examen_parcial.Models;

namespace examen_parcial.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar restricciones
            builder.Entity<Curso>(entity =>
            {
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(10);
                entity.Property(e => e.Nombre).HasMaxLength(200);
            });

            builder.Entity<Matricula>(entity =>
            {
                // Un usuario no puede estar matriculado más de una vez en el mismo curso
                entity.HasIndex(e => new { e.CursoId, e.UsuarioId }).IsUnique();
                
                entity.HasOne(m => m.Curso)
                    .WithMany(c => c.Matriculas)
                    .HasForeignKey(m => m.CursoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}