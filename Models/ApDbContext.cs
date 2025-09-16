using Microsoft.EntityFrameworkCore;

namespace Diplom.Models
{
    public class ApDbContext : DbContext
    {
        public class AppDbContext : DbContext
        {
            private string _conectionstring;
            public AppDbContext()
            {

            }

            public AppDbContext(string conectionstring)
            {
                _conectionstring = conectionstring;
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Book> Books { get; set; }
            public DbSet<Reservation> Reserv { get; set; }

            public DbSet<Genres> Genres { get; set; }
            public DbSet<Autors> Authors { get; set; }
            public DbSet<Penalties> Penalties { get; set; }
           
           

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_conectionstring);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<User>(entity =>
                {
                    entity.ToTable("Users");
                    entity.HasKey(e => e.Id).HasName("user_key");
                    entity.HasIndex(e => e.Email).IsUnique();
                    entity.HasIndex(e => e.PasswordHash).IsUnique();

                    entity.Property(e => e.Id).HasColumnName("UserId");
                    entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("UserEmail");
                    entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("UserPassword");
                    entity.Property(e => e.RegistrationDate).HasColumnName("DataRegistration");
                    entity.Property(e => e.Role).HasColumnName("UserRole");
                    entity.Property(e => e.IsBlocked).HasColumnName("UserBlocked");


                    
                    entity.HasMany(res => res.Reservations).WithOne(e => e.User);
                });


                modelBuilder.Entity<Book>(entity =>
                {
                    entity.ToTable("Books");
                    entity.HasKey(b => b.Id).HasName("book_key");
                    entity.HasIndex(b => b.ISBN).IsUnique();

                    entity.Property(b => b.Id).HasColumnName("Bookid");
                    entity.Property(b => b.BookTitle).HasColumnName("BookTitle");
                    entity.Property(b => b.ISBN).HasColumnName("BookISBN");
                    entity.Property(b => b.TotalCopies).HasColumnName("BookTotal");
                    entity.Property(b => b.AvailableCopies).HasColumnName("BookAvailable");
                    entity.Property(b => b.Status).HasColumnName("BookStatus");

                    entity.HasMany(bo => bo.Authors).WithMany(b => b.Books).UsingEntity(x => x.ToTable("AuthorBooks"));

                    entity.HasMany(rv => rv.Reservations).WithOne(b => b.Book);

                   

                    entity.HasOne(g => g.Genre).WithMany(b => b.Books);

                });

                modelBuilder.Entity<Reservation>(entity =>
                {
                    entity.ToTable("Revervation");
                    entity.HasKey(b => b.Id).HasName("rever_key");
                    

                    entity.Property(b => b.Id).HasColumnName("Reverid");
                    entity.Property(b => b.ReservationDate).HasColumnName("ReservDate");
                    entity.Property(b => b.DueDate).HasColumnName("DueDate");
                    entity.Property(b => b.ReturnDate).HasColumnName("ReturnDate");
                    entity.Property(b => b.Status).HasColumnName("ResrvStatusId");
                    entity.Property(b => b.Comment).HasColumnName("Comment");
                    entity.Property(b => b.IsBlocked).HasColumnName("ISblocked");

                    entity.HasOne(p => p.Penalty).WithOne(rev => rev.Reservation);

                });

                modelBuilder.Entity<Autors>(entity =>
                {
                    entity.ToTable("Autors");
                    entity.HasKey(a => a.Id).HasName("autor_key");

                    entity.Property(a => a.Id).HasColumnName("AutorId");
                    entity.Property(a => a.FirstName).HasColumnName("AutorName");
                    entity.Property(a => a.SurName).HasColumnName("AutorSurname");
                    entity.Property(a => a.LastName).HasColumnName("AutorLastname");
                    entity.Property(a => a.Bio).HasColumnName("AutorBio");

                    entity.HasMany(bo => bo.Books).WithMany(b => b.Authors).UsingEntity(x => x.ToTable("AuthorBooks"));
                });

                modelBuilder.Entity<Genres>(entity =>
                {
                    entity.ToTable("Genres");
                    entity.HasKey(g => g.Id).HasName("genres_key");
                    entity.HasIndex(g => g.Name).IsUnique();

                    entity.Property(g => g.Id).HasColumnName("GenresId");
                    entity.Property(g => g.Name).HasColumnName("GenresName");

                    entity.HasMany(b => b.Books).WithOne(g => g.Genre);
   
                });

   

                modelBuilder.Entity<Penalties>(entity =>
                {
                    entity.ToTable("Penalties");
                    entity.HasKey(p => p.Id).HasName("PenaltiesId");

                    entity.Property(p => p.Id).HasColumnName("Penaltiesid");
                    entity.Property(p => p.Amount).HasColumnName("Amount");
                    entity.Property(p => p.IssueDate).HasColumnName("IssueDate");

                    entity.HasOne(rev => rev.Reservation).WithOne(p => p.Penalty);
                });
            }
        }
    }
}
