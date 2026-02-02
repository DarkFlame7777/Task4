using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Task4.Data;

#nullable disable

namespace Task4.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250202_AddIsEmailVerifiedToUser")]
    partial class AddIsEmailVerifiedToUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Task4.Models.User", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                b.Property<bool>("IsDeleted")
                    .HasColumnType("tinyint(1)");

                b.Property<bool>("IsEmailVerified")
                    .HasColumnType("tinyint(1)");

                b.Property<DateTime?>("LastActivityTime")
                    .HasColumnType("datetime(6)");

                b.Property<DateTime?>("LastLoginTime")
                    .HasColumnType("datetime(6)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("longtext");

                b.Property<DateTime>("RegistrationTime")
                    .HasColumnType("datetime(6)");

                b.Property<int>("Status")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.ToTable("Users");
            });
#pragma warning restore 612, 618
        }
    }
}