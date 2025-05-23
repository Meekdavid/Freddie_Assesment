﻿// <auto-generated />
using System;
using Freddie.Helpers.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Freddie.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("Freddie.Helpers.Services.CandidateEvaluation", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("CandidateId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EvaluationDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EvaluationNotes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CandidateId")
                        .IsUnique();

                    b.ToTable("Evaluations");
                });

            modelBuilder.Entity("Freddie.Models.Candidate", b =>
                {
                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AvailableImmediately")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<string>("BiggestWeakness")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool?>("Contacted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ContactedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("KeyStrengths")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ModifiedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("ResumeText")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ResumeUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Email");

                    b.ToTable("Candidates");
                });

            modelBuilder.Entity("Freddie.Helpers.Services.CandidateEvaluation", b =>
                {
                    b.HasOne("Freddie.Models.Candidate", null)
                        .WithOne("AIEvaluation")
                        .HasForeignKey("Freddie.Helpers.Services.CandidateEvaluation", "CandidateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Freddie.Models.Candidate", b =>
                {
                    b.Navigation("AIEvaluation")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
