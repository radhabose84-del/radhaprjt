using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Core.Domain.Common.BaseEntity;

namespace PartyManagement.Infrastructure.Data.Configurations
{
    public class PartyMasterConfiguration : IEntityTypeConfiguration<PartyMaster>
    {
        public void Configure(EntityTypeBuilder<PartyMaster> builder)
        {
            // ValueConverter for Status (enum to bit)
            var statusConverter = new ValueConverter<Status, bool>(
                v => v == Status.Active,                    // Convert to DB (1 for Active)
                v => v ? Status.Active : Status.Inactive    // Convert to Entity
            );

            // ValueConverter for IsDelete (enum to bit)
            var isDeleteConverter = new ValueConverter<IsDelete, bool>(
                v => v == IsDelete.Deleted,                 // Convert to DB (1 for Deleted)
                v => v ? IsDelete.Deleted : IsDelete.NotDeleted // Convert to Entity
            );

            builder.ToTable("PartyMaster", "Party");
            // Primary Key
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id)
                .HasColumnName("Id")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(m => m.PartyCode)
                .HasColumnName("PartyCode")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            builder.Property(m => m.PartyName)
                .HasColumnName("PartyName")
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(m => m.PartyZoneId)  // Foreign Key column
               .HasColumnName("PartyZoneId")
               .HasColumnType("int");  // Set as int


            // Foreign Key Relationship
            builder.HasOne(m => m.ZoneType)
                .WithMany(t => t.PartyZoneType)
                .HasForeignKey(m => m.PartyZoneId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed


            builder.Property(m => m.RegistrationTypeId)  // Foreign Key column
                .HasColumnName("RegistrationTypeId")
                .HasColumnType("int")  // Set as int
                .IsRequired();

            // Foreign Key Relationship
            builder.HasOne(m => m.RegistrationType)
                .WithMany(t => t.PartyRegistrationType)
                .HasForeignKey(m => m.RegistrationTypeId) // Foreign Key property in MiscMaster
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.GSTNumber)
               .HasColumnName("GSTNumber")
               .HasColumnType("nvarchar(20)");


            builder.Property(m => m.GSTStateCode)
               .HasColumnName("GSTStateCode")
               .HasColumnType("int");


            builder.Property(m => m.PAN)
              .HasColumnName("PAN")
              .HasColumnType("nvarchar(20)");


            builder.Property(m => m.Website)
               .HasColumnName("Website")
               .HasColumnType("nvarchar(50)");

            builder.Property(m => m.TAN)
              .HasColumnName("TAN")
              .HasColumnType("nvarchar(50)");

            builder.Property(m => m.TDSCategoryId)
              .HasColumnName("TDSCategoryId")
              .HasColumnType("int");

            builder.Property(m => m.MSMETypeId)  // Foreign Key column
              .HasColumnName("MSMETypeId")
              .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.MSMETypeMisc)
                .WithMany(t => t.PartyMSMEType)
                .HasForeignKey(m => m.MSMETypeId) // Foreign Key property in Misc
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.MSMENO)
              .HasColumnName("MSMENO")
              .HasColumnType("nvarchar(50)");

            builder.Property(x => x.MSMEValidUpto)
                     .HasColumnName("MSMEValidUpto")
                      .HasColumnType("datetimeoffset")
                    .IsRequired(false); // Allows NULL in DB

            builder.Property(t => t.IsMsmeCompliant)
                .HasColumnName("IsMsmeCompliant")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.IsTDSApplicable)
                .HasColumnName("IsTDSApplicable")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.IsTCSApplicable)
               .HasColumnName("IsTCSApplicable")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(t => t.IsGstReverseCharge)
                .HasColumnName("IsGstReverseCharge")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.Is206AB206CCAApplicable)
                .HasColumnName("Is206AB206CCAApplicable")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(m => m.PayementModeId)  // Foreign Key column
              .HasColumnName("PayementModeId")
              .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.PaymentModeTypeMisc)
                .WithMany(t => t.PartyPaymentModeType)
                .HasForeignKey(m => m.PayementModeId) // Foreign Key property in Misc
                .OnDelete(DeleteBehavior.Restrict); // Use .Cascade if needed

            builder.Property(m => m.FavourOf)
               .HasColumnName("FavourOf")
               .HasColumnType("nvarchar(250)");


            builder.Property(m => m.PreferredCurrencyPurchase)
              .HasColumnName("PreferredCurrencyPurchase")
               .HasColumnType("int");

            builder.Property(m => m.CreditDays)  // Foreign Key column
             .HasColumnName("CreditDays")
             .HasColumnType("int");

            builder.Property(m => m.DueDateTypeId)  // Foreign Key column
             .HasColumnName("DueDateTypeId")
             .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.DueDateTypeMisc)
                .WithMany(t => t.PartyDueDateType)
                .HasForeignKey(m => m.DueDateTypeId) // Foreign Key property in Misc
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(m => m.LeadTime)  // Foreign Key column
              .HasColumnName("LeadTime")
              .HasColumnType("int");


            builder.Property(m => m.PreferredCurrencySale)
               .HasColumnName("PreferredCurrencySale")
               .HasColumnType("int");

            builder.Property(x => x.CreditLimit)
                .HasColumnType("decimal(18,3)")
                .HasDefaultValue(0.000m) // default in DB
                .IsRequired(false);

            builder.Property(m => m.SellingPriceListId)  // Foreign Key column
              .HasColumnName("SellingPriceListId")
              .HasColumnType("int");


            builder.Property(m => m.CustomerTypeId)  // Foreign Key column
             .HasColumnName("CustomerTypeId")
             .HasColumnType("int");  // Set as int

            // Foreign Key Relationship
            builder.HasOne(m => m.CustomerTypeMisc)
                .WithMany(t => t.PartyCustomerType)
                .HasForeignKey(m => m.CustomerTypeId) // Foreign Key property in Misc
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(t => t.IsInternalSupplier)
                .HasColumnName("IsInternalSupplier")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(t => t.IsInternalCustomer)
               .HasColumnName("IsInternalCustomer")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(t => t.IsStopPayment)
               .HasColumnName("IsStopPayment")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(b => b.IsActive)
                .HasColumnName("IsActive")
                .HasColumnType("bit")
                .HasConversion(statusConverter)
                .IsRequired();

            builder.Property(b => b.IsDeleted)
                .HasColumnName("IsDeleted")
                .HasColumnType("bit")
                .HasConversion(isDeleteConverter)
                .IsRequired();

            builder.Property(b => b.CreatedByName)
                .IsRequired()
                .HasColumnType("varchar(50)");


            builder.Property(b => b.CreatedIP)
                .IsRequired()
                .HasColumnType("varchar(255)");

            builder.Property(b => b.ModifiedByName)
                .HasColumnType("varchar(50)");

            builder.Property(b => b.ModifiedIP)
                .HasColumnType("varchar(255)");

            builder.Property(m => m.PartyStatus)
              .HasColumnName("PartyStatus")
              .HasColumnType("nvarchar(20)");

            builder.Property(x => x.GSTRegistrationDate)
                   .HasColumnName("GSTRegistrationDate")
                    .HasColumnType("datetimeoffset")
                  .IsRequired(false); // Allows NULL in DB


            builder.Property(x => x.MSMERegistrationDate)
                   .HasColumnName("MSMERegistrationDate")
                    .HasColumnType("datetimeoffset")
                  .IsRequired(false); // Allows NULL in DB

            builder.Property(m => m.CIN)
              .HasColumnName("CIN")
              .HasColumnType("nvarchar(25)");

            builder.Property(m => m.IECode)
             .HasColumnName("IECode")
             .HasColumnType("nvarchar(25)");

            builder.Property(t => t.IsGroup)
              .HasColumnName("IsGroup")
              .HasColumnType("bit")
              .IsRequired();

            builder.Property(t => t.IsSubsidiary)
                .HasColumnName("IsSubsidiary")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(m => m.UnitId)
               .HasColumnName("UnitId")
               .HasColumnType("int")
               .IsRequired();

            builder.Property(m => m.StatusId)
                .HasColumnName("StatusId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.InsuranceLimit)
               .HasColumnType("decimal(18,3)")
               .HasDefaultValue(0.000m) // default in DB
               .IsRequired(false);

            builder.HasOne(ac => ac.StatusParty)
                .WithMany(am => am.StatusHeader)
                .HasForeignKey(ac => ac.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(t => t.IsPortalAccessEnabled)
                .HasColumnName("IsPortalAccessEnabled")
                .HasColumnType("bit")
                .IsRequired();
                
            builder.Property(t => t.IsUpdate)
                .HasColumnName("IsUpdate")
                .HasColumnType("bit")
                .IsRequired();
                                                
        }
    }
}