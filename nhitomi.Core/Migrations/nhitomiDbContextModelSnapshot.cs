﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace nhitomi.Core.Migrations
{
    [DbContext(typeof(nhitomiDbContext))]
    partial class nhitomiDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("nhitomi.Core.Artist", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Artists");
            });

            modelBuilder.Entity("nhitomi.Core.Category", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Categories");
            });

            modelBuilder.Entity("nhitomi.Core.Category+Reference", b =>
            {
                b.Property<int>("DoujinId");

                b.Property<int>("TagId");

                b.HasKey("DoujinId", "TagId");

                b.HasIndex("TagId");

                b.ToTable("CategoryRefs");
            });

            modelBuilder.Entity("nhitomi.Core.Character", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Characters");
            });

            modelBuilder.Entity("nhitomi.Core.Character+Reference", b =>
            {
                b.Property<int>("DoujinId");

                b.Property<int>("TagId");

                b.HasKey("DoujinId", "TagId");

                b.HasIndex("TagId");

                b.ToTable("CharacterRefs");
            });

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

            modelBuilder.Entity("nhitomi.Core.Doujin", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<int?>("ArtistId");

                b.Property<string>("GalleryUrl")
                    .IsRequired();

                b.Property<int?>("GroupId");

                b.Property<int?>("LanguageId");

                b.Property<string>("OriginalName")
                    .IsRequired();

                b.Property<int>("PageCount");

                b.Property<int?>("ParodyOfId");

                b.Property<string>("PrettyName")
                    .IsRequired();

                b.Property<DateTime>("ProcessTime")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                b.Property<int?>("ScanlatorId");

                b.Property<string>("Source")
                    .IsRequired();

                b.Property<string>("SourceId")
                    .IsRequired();

                b.Property<DateTime>("UploadTime");

                b.HasKey("Id");

                b.HasIndex("ArtistId");

                b.HasIndex("GroupId");

                b.HasIndex("LanguageId");

                b.HasIndex("OriginalName");

                b.HasIndex("ParodyOfId");

                b.HasIndex("PrettyName");

                b.HasIndex("ScanlatorId");

                b.HasIndex("Source");

                b.HasIndex("SourceId");

                b.ToTable("Doujins");
            });

            modelBuilder.Entity("nhitomi.Core.DoujinCollection", b =>
            {
                b.Property<int>("CollectionId");

                b.Property<int>("DoujinId");

                b.HasKey("CollectionId", "DoujinId");

                b.HasIndex("DoujinId");

                b.ToTable("CollectionRefs");
            });

            modelBuilder.Entity("nhitomi.Core.Group", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Groups");
            });

            modelBuilder.Entity("nhitomi.Core.Guild", b =>
            {
                b.Property<ulong>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Language");

                b.HasKey("Id");

                b.ToTable("Guilds");
            });

            modelBuilder.Entity("nhitomi.Core.Language", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Languages");
            });

            modelBuilder.Entity("nhitomi.Core.ParodyOf", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Parodies");
            });

            modelBuilder.Entity("nhitomi.Core.Scanlator", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Scanlators");
            });

            modelBuilder.Entity("nhitomi.Core.Tag", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Value")
                    .IsRequired()
                    .HasMaxLength(32);

                b.HasKey("Id");

                b.HasIndex("Value")
                    .IsUnique();

                b.ToTable("Tags");
            });

            modelBuilder.Entity("nhitomi.Core.Tag+Reference", b =>
            {
                b.Property<int>("DoujinId");

                b.Property<int>("TagId");

                b.HasKey("DoujinId", "TagId");

                b.HasIndex("TagId");

                b.ToTable("TagRefs");
            });

            modelBuilder.Entity("nhitomi.Core.User", b =>
            {
                b.Property<ulong>("Id")
                    .ValueGeneratedOnAdd();

                b.HasKey("Id");

                b.ToTable("Users");
            });

            modelBuilder.Entity("nhitomi.Core.Category+Reference", b =>
            {
                b.HasOne("nhitomi.Core.Doujin", "Doujin")
                    .WithMany("Categories")
                    .HasForeignKey("DoujinId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("nhitomi.Core.Category", "Tag")
                    .WithMany("Doujins")
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("nhitomi.Core.Character+Reference", b =>
            {
                b.HasOne("nhitomi.Core.Doujin", "Doujin")
                    .WithMany("Characters")
                    .HasForeignKey("DoujinId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("nhitomi.Core.Character", "Tag")
                    .WithMany("Doujins")
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("nhitomi.Core.Collection", b =>
            {
                b.HasOne("nhitomi.Core.User", "Owner")
                    .WithMany("Collections")
                    .HasForeignKey("OwnerId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity("nhitomi.Core.Doujin", b =>
            {
                b.HasOne("nhitomi.Core.Artist", "Artist")
                    .WithMany("Doujins")
                    .HasForeignKey("ArtistId");

                b.HasOne("nhitomi.Core.Group", "Group")
                    .WithMany("Doujins")
                    .HasForeignKey("GroupId");

                b.HasOne("nhitomi.Core.Language", "Language")
                    .WithMany("Doujins")
                    .HasForeignKey("LanguageId");

                b.HasOne("nhitomi.Core.ParodyOf", "ParodyOf")
                    .WithMany("Doujins")
                    .HasForeignKey("ParodyOfId");

                b.HasOne("nhitomi.Core.Scanlator", "Scanlator")
                    .WithMany("Doujins")
                    .HasForeignKey("ScanlatorId");
            });

            modelBuilder.Entity("nhitomi.Core.DoujinCollection", b =>
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

            modelBuilder.Entity("nhitomi.Core.Tag+Reference", b =>
            {
                b.HasOne("nhitomi.Core.Doujin", "Doujin")
                    .WithMany("Tags")
                    .HasForeignKey("DoujinId")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("nhitomi.Core.Tag", "Tag")
                    .WithMany("Doujins")
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
#pragma warning restore 612, 618
        }
    }
}