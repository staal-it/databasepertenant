﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabasePerTenant.Data.Catalog.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20201009092609_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DatabasePerTenant.Data.Catalog.Model.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("DatabasePerTenant.Data.Catalog.Model.ElasticPool", b =>
                {
                    b.Property<int>("ElasticPoolId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ElasticPoolName")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("ServerId")
                        .HasColumnType("int");

                    b.HasKey("ElasticPoolId");

                    b.HasIndex("ServerId");

                    b.ToTable("ElasticPools");
                });

            modelBuilder.Entity("DatabasePerTenant.Data.Catalog.Model.Server", b =>
                {
                    b.Property<int>("ServerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("ServerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.HasKey("ServerId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("DatabasePerTenant.Data.Catalog.Model.Tenant", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnType("int")
                        .HasMaxLength(128);

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("DatabaseName")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<int>("ElasticPoolId")
                        .HasColumnType("int");

                    b.Property<byte[]>("HashedTenantId")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("TenantName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TenantId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ElasticPoolId");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("DatabasePerTenant.Data.Catalog.Model.ElasticPool", b =>
                {
                    b.HasOne("DatabasePerTenant.Data.Catalog.Model.Server", "Server")
                        .WithMany("ElasticPools")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DatabasePerTenant.Data.Catalog.Model.Tenant", b =>
                {
                    b.HasOne("DatabasePerTenant.Data.Catalog.Model.Customer", "Customer")
                        .WithMany("Tenants")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DatabasePerTenant.Data.Catalog.Model.ElasticPool", "ElasticPool")
                        .WithMany()
                        .HasForeignKey("ElasticPoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
