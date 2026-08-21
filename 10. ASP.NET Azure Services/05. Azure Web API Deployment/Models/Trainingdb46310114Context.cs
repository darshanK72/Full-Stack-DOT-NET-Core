using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LocalServerWebApiApplication.Models;

public partial class Trainingdb46310114Context : DbContext
{
    public Trainingdb46310114Context()
    {
    }

    public Trainingdb46310114Context(DbContextOptions<Trainingdb46310114Context> options)
        : base(options)
    {

    }

    public virtual DbSet<Department> Departments { get; set; }

 //   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
 //=> optionsBuilder.UseSqlServer("Server = tcp:localserverwebapiapplicationdbserver.database.windows.net, 1433; Initial Catalog = LocalServerWebApiApplication_db; Persist Security Info=False;User ID = rootuser; Password=Darshan123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__F9B8346D00761E68");

            entity.ToTable("Department");

            entity.Property(e => e.DepartmentId)
                .ValueGeneratedNever()
                .HasColumnName("departmentId");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("departmentName");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
