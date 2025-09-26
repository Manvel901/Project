using Microsoft.EntityFrameworkCore;

namespace Diplom.Models
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

            public virtual DbSet<User> Users { get; set; }
            public virtual DbSet<Book> Books { get; set; }
            public virtual DbSet<Reservation> Reserv { get; set; }

            public virtual DbSet<Genres> Genres { get; set; }
            public virtual DbSet<Autors> Authors { get; set; }
            public virtual DbSet<Penalties> Penalties { get; set; }
           
           

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql(_conectionstring);
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


                    
                    entity.HasMany(res => res.Reservations).WithOne(e => e.User).HasForeignKey(x=> x.UserId);
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

                    entity.HasMany(rv => rv.Reservations).WithOne(b => b.Book).HasForeignKey(x => x.BookId);

                   

                    entity.HasOne(g => g.Genre).WithMany(b => b.Books).HasForeignKey(x=> x.GenreId);

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
                    

                    entity.HasOne(r => r.Penalty).WithOne(p => p.Reservation).HasForeignKey<Penalties>(p => p.ReservationId);
                    entity.HasOne(p => p.Book).WithMany(rev => rev.Reservations).HasForeignKey(x=>x.BookId);
                    entity.HasOne(p => p.User).WithMany(rev => rev.Reservations).HasForeignKey(x => x.UserId);

                });

                modelBuilder.Entity<Autors>(entity =>
                {
                    entity.ToTable("Autors");
                    entity.HasKey(a => a.Id).HasName("autor_key");
                    entity.HasIndex(a => a.FullName).IsUnique();
                    
                    entity.Property(a => a.Id).HasColumnName("AutorId");
                    entity.Property(a => a.FullName).HasColumnName("AutorName");
                   
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

                    entity.HasMany(b => b.Books).WithOne(g => g.Genre).HasForeignKey(x => x.GenreId); 
   
                });

   

                modelBuilder.Entity<Penalties>(entity =>
                {
                    entity.ToTable("Penalties");
                    entity.HasKey(p => p.Id).HasName("PenaltiesId");

                    entity.Property(p => p.Id).HasColumnName("Penaltiesid");
                    entity.Property(p => p.Amount).HasColumnName("Amount");
                    entity.Property(p => p.IssueDate).HasColumnName("IssueDate");
                    entity.Property(p => p.AmountPaid).HasColumnName("AmountPaid");
                    entity.Property(p => p.IsCancelled).HasColumnName("IsCancelled");
                    entity.Property(p => p.PaidAtUtc).HasColumnName("PaidAtUtc");

                    entity.HasOne(rev => rev.Reservation).WithOne(p => p.Penalty).HasForeignKey<Reservation>(x=>x.PenaltyId);
                });

                 base.OnModelCreating(modelBuilder);

        }
       
        
        }
    }

