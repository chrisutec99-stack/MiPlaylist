using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MiPlaylist.Models
{
    public class MiPlaylistContext : DbContext
    {
        public MiPlaylistContext() : base("name=MiPlaylistContext")
        {
            // La base ya existe: EF no debe intentar crearla ni modificarla
            Database.SetInitializer<MiPlaylistContext>(null);
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cancion> Canciones { get; set; }
        public DbSet<UsuarioCancion> UsuarioCanciones { get; set; }
        public DbSet<Lista> Listas { get; set; }
        public DbSet<ListaCancion> ListaCanciones { get; set; }
        public DbSet<RecuperacionClave> RecuperacionesClave { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Nuestras tablas están en singular (Usuario, Cancion...)
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<UsuarioCancion>().HasKey(x => new { x.UsuarioId, x.CancionId });
            modelBuilder.Entity<ListaCancion>().HasKey(x => new { x.ListaId, x.CancionId });
        }
    }
}