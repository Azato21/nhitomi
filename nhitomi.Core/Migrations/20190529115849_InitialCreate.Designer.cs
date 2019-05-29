﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using nhitomi.Core;

namespace nhitomi.Core.Migrations
{
    [DbContext(typeof(nhitomiDbContext))]
    [Migration("20190529115849_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("nhitomi.Core.Collection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<ulong>("OwnerId");

                    b.Property<int>("Sort");

                    b.Property<bool>("SortDescending");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("OwnerId");

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("nhitomi.Core.CollectionRef", b =>
                {
                    b.Property<int>("CollectionId");

                    b.Property<int>("DoujinId");

                    b.HasKey("CollectionId", "DoujinId");

                    b.HasIndex("DoujinId");

                    b.ToTable("CollectionRef");
                });

            modelBuilder.Entity("nhitomi.Core.Doujin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GalleryUrl")
                        .IsRequired();

                    b.Property<string>("OriginalName")
                        .IsRequired();

                    b.Property<int>("PageCount");

                    b.Property<string>("PrettyName")
                        .IsRequired();

                    b.Property<DateTime>("ProcessTime")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("Source")
                        .IsRequired();

                    b.Property<string>("SourceId")
                        .IsRequired();

                    b.Property<DateTime>("UploadTime");

                    b.HasKey("Id");

                    b.HasIndex("OriginalName");

                    b.HasIndex("PrettyName");

                    b.HasIndex("Source");

                    b.HasIndex("SourceId");

                    b.ToTable("Doujins");
                });

            modelBuilder.Entity("nhitomi.Core.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Language");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("nhitomi.Core.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Type");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.HasKey("Id", "Type");

                    b.HasIndex("Value")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("nhitomi.Core.TagRef", b =>
                {
                    b.Property<int>("DoujinId");

                    b.Property<int>("TagId");

                    b.Property<int>("TagType");

                    b.HasKey("DoujinId", "TagId");

                    b.HasIndex("TagId", "TagType");

                    b.ToTable("TagRef");
                });

            modelBuilder.Entity("nhitomi.Core.User", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd();

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("nhitomi.Core.Collection", b =>
                {
                    b.HasOne("nhitomi.Core.User", "Owner")
                        .WithMany("Collections")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("nhitomi.Core.CollectionRef", b =>
                {
                    b.HasOne("nhitomi.Core.Collection", "Collection")
                        .WithMany("Doujins")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("nhitomi.Core.Doujin", "Doujin")
                        .WithMany("Collections")
                        .HasForeignKey("DoujinId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("nhitomi.Core.TagRef", b =>
                {
                    b.HasOne("nhitomi.Core.Doujin", "Doujin")
                        .WithMany("Tags")
                        .HasForeignKey("DoujinId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("nhitomi.Core.Tag", "Tag")
                        .WithMany("Doujins")
                        .HasForeignKey("TagId", "TagType")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
