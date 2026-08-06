using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DbFirstOperations.Models;

public partial class SchoolDbContext : DbContext
{
    public SchoolDbContext()
    {
    }

    public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAddress> StudentAddresses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=INBLRVM26590142;Database=SchoolDB;Trusted_Connection=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.RollNo).HasName("PK__Student__9560EEE157F79A30");

            entity.ToTable("Student");

            entity.Property(e => e.RollNo)
                .ValueGeneratedNever()
                .HasColumnName("roll_no");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.Division)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("division");
            entity.Property(e => e.Marks)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("marks");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.Address).WithMany(p => p.Students)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("fk_address");
        });

        modelBuilder.Entity<StudentAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__StudentA__CAA247C8E24CE77A");

            entity.ToTable("StudentAddress");

            entity.Property(e => e.AddressId)
                .ValueGeneratedNever()
                .HasColumnName("address_id");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Locality)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("locality");
            entity.Property(e => e.Pincode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("pincode");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
