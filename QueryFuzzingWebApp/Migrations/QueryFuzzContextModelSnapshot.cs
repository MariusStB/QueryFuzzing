﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QueryFuzzingWebApp.Database;

#nullable disable

namespace QueryFuzzingWebApp.Migrations
{
    [DbContext(typeof(QueryFuzzContext))]
    partial class QueryFuzzContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("QueryFuzzingWebApp.Database.Models.Executable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Executables");
                });

            modelBuilder.Entity("QueryFuzzingWebApp.Database.Models.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Language")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Tag")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("QueryFuzzingWebApp.Database.Models.Target", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("File")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Line")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ProjectId");

                    b.ToTable("Targets");
                });

            modelBuilder.Entity("QueryFuzzingWebApp.Database.Models.Executable", b =>
                {
                    b.HasOne("QueryFuzzingWebApp.Database.Models.Project", "Project")
                        .WithMany("Executables")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("QueryFuzzingWebApp.Database.Models.Target", b =>
                {
                    b.HasOne("QueryFuzzingWebApp.Database.Models.Project", "Project")
                        .WithMany("Targets")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");
                });

            modelBuilder.Entity("QueryFuzzingWebApp.Database.Models.Project", b =>
                {
                    b.Navigation("Executables");

                    b.Navigation("Targets");
                });
#pragma warning restore 612, 618
        }
    }
}