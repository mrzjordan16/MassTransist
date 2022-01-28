﻿// <auto-generated />
namespace MassTransit.EntityFrameworkCoreIntegration.Tests.Migrations.SagaWithDependency
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Tests.SagaWithDependency.DataAccess;


    [DbContext(typeof(SagaWithDependencyContext))]
    class SagaWithDependencyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
        #pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.DataAccess.SagaDependency", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<Guid>("SagaInnerDependencyId");

                b.HasKey("Id");

                b.HasIndex("SagaInnerDependencyId");

                b.ToTable("SagaDependency");
            });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.DataAccess.SagaInnerDependency", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd();

                b.Property<string>("Name");

                b.HasKey("Id");

                b.ToTable("SagaInnerDependency");
            });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.SagaWithDependency", b =>
            {
                b.Property<Guid>("CorrelationId");

                b.Property<bool>("Completed");

                b.Property<Guid>("DependencyId");

                b.Property<bool>("Initiated");

                b.Property<string>("Name")
                    .HasMaxLength(40);

                b.HasKey("CorrelationId");

                b.HasIndex("DependencyId");

                b.ToTable("EfCoreSagasWithDepencies");
            });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.DataAccess.SagaDependency", b =>
            {
                b.HasOne("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.DataAccess.SagaInnerDependency", "SagaInnerDependency")
                    .WithMany()
                    .HasForeignKey("SagaInnerDependencyId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.SagaWithDependency", b =>
            {
                b.HasOne("MassTransit.EntityFrameworkCoreIntegration.Tests.SagaWithDependency.DataAccess.SagaDependency", "Dependency")
                    .WithMany()
                    .HasForeignKey("DependencyId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
        #pragma warning restore 612, 618
        }
    }
}
