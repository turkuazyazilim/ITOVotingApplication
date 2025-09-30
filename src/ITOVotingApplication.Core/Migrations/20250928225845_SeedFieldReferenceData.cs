using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITOVotingApplication.Core.Migrations
{
    /// <inheritdoc />
    public partial class SeedFieldReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Level 1 Categories
            migrationBuilder.InsertData(
                table: "prFieldReferenceCategories",
                columns: new[] { "CategoryName", "Description", "IsActive" },
                values: new object[,]
                {
                    { "Parti", "Siyasi Partiler", true },
                    { "STK", "Sivil Toplum Kuruluşları", true },
                    { "Dernek", "Meslek Odaları ve Dernekler", true },
                    { "Kamu", "Kamu Kurumları", true }
                });

            // Insert Level 2 SubCategories for Parti
            migrationBuilder.InsertData(
                table: "prFieldReferenceSubCategories",
                columns: new[] { "CategoryId", "SubCategoryName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 1, "CHP", "Cumhuriyet Halk Partisi", true },
                    { 1, "AK Parti", "Adalet ve Kalkınma Partisi", true },
                    { 1, "İYİ Parti", "İYİ Parti", true },
                    { 1, "MHP", "Milliyetçi Hareket Partisi", true },
                    { 1, "HDP", "Halkların Demokratik Partisi", true }
                });

            // Insert Level 2 SubCategories for STK
            migrationBuilder.InsertData(
                table: "prFieldReferenceSubCategories",
                columns: new[] { "CategoryId", "SubCategoryName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 2, "MÜSİAD", "Müstakil Sanayici ve İşadamları Derneği", true },
                    { 2, "TÜSİAD", "Türk Sanayicileri ve İşadamları Derneği", true },
                    { 2, "TOBB", "Türkiye Odalar ve Borsalar Birliği", true },
                    { 2, "TÜRKONFED", "Türk Girişim ve İş Dünyası Konfederasyonu", true },
                    { 2, "ASİDER", "Anadolu Sanayicileri ve İşadamları Derneği", true }
                });

            // Insert Level 2 SubCategories for Dernek
            migrationBuilder.InsertData(
                table: "prFieldReferenceSubCategories",
                columns: new[] { "CategoryId", "SubCategoryName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 3, "İTO", "İstanbul Ticaret Odası", true },
                    { 3, "İSO", "İstanbul Sanayi Odası", true },
                    { 3, "TMMOB", "Türk Mühendis ve Mimar Odaları Birliği", true },
                    { 3, "TBB", "Türkiye Barolar Birliği", true },
                    { 3, "TTB", "Türk Tabipleri Birliği", true }
                });

            // Insert Level 2 SubCategories for Kamu
            migrationBuilder.InsertData(
                table: "prFieldReferenceSubCategories",
                columns: new[] { "CategoryId", "SubCategoryName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 4, "Belediye", "İlçe/İl Belediyeleri", true },
                    { 4, "Valilik", "İl/İlçe Valilikleri", true },
                    { 4, "Bakanlık", "Merkezi Bakanlıklar", true },
                    { 4, "Meclis", "Türkiye Büyük Millet Meclisi", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data
            migrationBuilder.DeleteData(
                table: "prFieldReferenceSubCategories",
                keyColumn: "CategoryId",
                keyValues: new object[] { 1, 2, 3, 4 });

            migrationBuilder.DeleteData(
                table: "prFieldReferenceCategories",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4 });
        }
    }
}
