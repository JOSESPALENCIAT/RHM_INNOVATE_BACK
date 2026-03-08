using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RHM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterPatientIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DivipolaCodes",
                columns: table => new
                {
                    MunCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DeptCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Municipio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MunicipioNormalized = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivipolaCodes", x => x.MunCode);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BiologicalSex = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DivipolaMunCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    DivipolaDeptCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DivipolaCodes",
                columns: new[] { "MunCode", "Departamento", "DeptCode", "Municipio", "MunicipioNormalized" },
                values: new object[,]
                {
                    { "05001", "Antioquia", "05", "Medellín", "MEDELLIN" },
                    { "05045", "Antioquia", "05", "Apartadó", "APARTADO" },
                    { "05088", "Antioquia", "05", "Bello", "BELLO" },
                    { "05129", "Antioquia", "05", "Caldas", "CALDAS" },
                    { "05147", "Antioquia", "05", "Carepa", "CAREPA" },
                    { "05212", "Antioquia", "05", "Copacabana", "COPACABANA" },
                    { "05238", "Antioquia", "05", "El Bagre", "EL BAGRE" },
                    { "05266", "Antioquia", "05", "Envigado", "ENVIGADO" },
                    { "05308", "Antioquia", "05", "Girardota", "GIRARDOTA" },
                    { "05360", "Antioquia", "05", "Itagüí", "ITAGUI" },
                    { "05380", "Antioquia", "05", "La Estrella", "LA ESTRELLA" },
                    { "05604", "Antioquia", "05", "Rionegro", "RIONEGRO" },
                    { "05615", "Antioquia", "05", "Samaná", "SAMANA" },
                    { "05631", "Antioquia", "05", "Sabaneta", "SABANETA" },
                    { "05790", "Antioquia", "05", "Turbo", "TURBO" },
                    { "08001", "Atlántico", "08", "Barranquilla", "BARRANQUILLA" },
                    { "08078", "Atlántico", "08", "Baranoa", "BARANOA" },
                    { "08137", "Atlántico", "08", "Campo de la Cruz", "CAMPO DE LA CRUZ" },
                    { "08296", "Atlántico", "08", "Galapa", "GALAPA" },
                    { "08433", "Atlántico", "08", "Malambo", "MALAMBO" },
                    { "08520", "Atlántico", "08", "Palmar de Varela", "PALMAR DE VARELA" },
                    { "08549", "Atlántico", "08", "Polonuevo", "POLONUEVO" },
                    { "08558", "Atlántico", "08", "Ponedera", "PONEDERA" },
                    { "08634", "Atlántico", "08", "Santo Tomás", "SANTO TOMAS" },
                    { "08675", "Atlántico", "08", "Sabanagrande", "SABANAGRANDE" },
                    { "08685", "Atlántico", "08", "Sabanalarga", "SABANALARGA" },
                    { "08758", "Atlántico", "08", "Soledad", "SOLEDAD" },
                    { "11001", "Bogotá D.C.", "11", "Bogotá D.C.", "BOGOTA" },
                    { "13001", "Bolívar", "13", "Cartagena de Indias", "CARTAGENA DE INDIAS" },
                    { "13140", "Bolívar", "13", "Calamar", "CALAMAR" },
                    { "13430", "Bolívar", "13", "Magangué", "MAGANGUE" },
                    { "13442", "Bolívar", "13", "Mompós", "MOMPOS" },
                    { "13620", "Bolívar", "13", "San Juan Nepomuceno", "SAN JUAN NEPOMUCENO" },
                    { "15001", "Boyacá", "15", "Tunja", "TUNJA" },
                    { "15176", "Boyacá", "15", "Chiquinquirá", "CHIQUINQUIRA" },
                    { "15238", "Boyacá", "15", "Duitama", "DUITAMA" },
                    { "15660", "Boyacá", "15", "Sogamoso", "SOGAMOSO" },
                    { "17001", "Caldas", "17", "Manizales", "MANIZALES" },
                    { "17042", "Caldas", "17", "Anserma", "ANSERMA" },
                    { "17380", "Caldas", "17", "La Dorada", "LA DORADA" },
                    { "17541", "Caldas", "17", "Riosucio", "RIOSUCIO" },
                    { "18001", "Caquetá", "18", "Florencia", "FLORENCIA" },
                    { "19001", "Cauca", "19", "Popayán", "POPAYAN" },
                    { "19698", "Cauca", "19", "Santander de Quilichao", "SANTANDER DE QUILICHAO" },
                    { "20001", "Cesar", "20", "Valledupar", "VALLEDUPAR" },
                    { "20045", "Cesar", "20", "Aguachica", "AGUACHICA" },
                    { "23001", "Córdoba", "23", "Montería", "MONTERIA" },
                    { "23162", "Córdoba", "23", "Cereté", "CERETE" },
                    { "23464", "Córdoba", "23", "Montelíbano", "MONTELIBANO" },
                    { "23672", "Córdoba", "23", "Sahagún", "SAHAGUN" },
                    { "25175", "Cundinamarca", "25", "Chía", "CHIA" },
                    { "25269", "Cundinamarca", "25", "Facatativá", "FACATATIVA" },
                    { "25290", "Cundinamarca", "25", "Fusagasugá", "FUSAGASUGA" },
                    { "25307", "Cundinamarca", "25", "Girardot", "GIRARDOT" },
                    { "25430", "Cundinamarca", "25", "Madrid", "MADRID" },
                    { "25473", "Cundinamarca", "25", "Mosquera", "MOSQUERA" },
                    { "25486", "Cundinamarca", "25", "Nemocón", "NEMOCON" },
                    { "25513", "Cundinamarca", "25", "Pacho", "PACHO" },
                    { "25754", "Cundinamarca", "25", "Soacha", "SOACHA" },
                    { "25758", "Cundinamarca", "25", "Sogamoso", "SOGAMOSO CUND" },
                    { "25769", "Cundinamarca", "25", "Subachoque", "SUBACHOQUE" },
                    { "25785", "Cundinamarca", "25", "Tabio", "TABIO" },
                    { "25817", "Cundinamarca", "25", "Tocancipá", "TOCANCIPA" },
                    { "25899", "Cundinamarca", "25", "Zipaquirá", "ZIPAQUIRA" },
                    { "27001", "Chocó", "27", "Quibdó", "QUIBDO" },
                    { "27413", "Chocó", "27", "Lloró", "LLORO" },
                    { "41001", "Huila", "41", "Neiva", "NEIVA" },
                    { "41006", "Huila", "41", "Acevedo", "ACEVEDO" },
                    { "41298", "Huila", "41", "Garzón", "GARZON" },
                    { "41524", "Huila", "41", "Pitalito", "PITALITO" },
                    { "44001", "La Guajira", "44", "Riohacha", "RIOHACHA" },
                    { "44430", "La Guajira", "44", "Maicao", "MAICAO" },
                    { "44560", "La Guajira", "44", "San Juan del Cesar", "SAN JUAN DEL CESAR" },
                    { "44847", "La Guajira", "44", "Uribia", "URIBIA" },
                    { "47001", "Magdalena", "47", "Santa Marta", "SANTA MARTA" },
                    { "47053", "Magdalena", "47", "Aracataca", "ARACATACA" },
                    { "47170", "Magdalena", "47", "Ciénaga", "CIENAGA" },
                    { "47460", "Magdalena", "47", "Mompós", "MOMPOS MAG" },
                    { "50001", "Meta", "50", "Villavicencio", "VILLAVICENCIO" },
                    { "50006", "Meta", "50", "Acacías", "ACACIAS" },
                    { "50313", "Meta", "50", "Granada", "GRANADA META" },
                    { "52001", "Nariño", "52", "Pasto", "PASTO" },
                    { "52356", "Nariño", "52", "Ipiales", "IPIALES" },
                    { "52480", "Nariño", "52", "La Unión", "LA UNION NARINO" },
                    { "52835", "Nariño", "52", "Tumaco", "TUMACO" },
                    { "54001", "Norte de Santander", "54", "Cúcuta", "CUCUTA" },
                    { "54174", "Norte de Santander", "54", "Chinácota", "CHINACOTA" },
                    { "54206", "Norte de Santander", "54", "Convención", "CONVENCION" },
                    { "54518", "Norte de Santander", "54", "Ocaña", "OCANA" },
                    { "54720", "Norte de Santander", "54", "Tibú", "TIBU" },
                    { "54810", "Norte de Santander", "54", "Villa del Rosario", "VILLA DEL ROSARIO" },
                    { "63001", "Quindío", "63", "Armenia", "ARMENIA" },
                    { "63111", "Quindío", "63", "Buenavista", "BUENAVISTA QUINDIO" },
                    { "63548", "Quindío", "63", "Quimbaya", "QUIMBAYA" },
                    { "66001", "Risaralda", "66", "Pereira", "PEREIRA" },
                    { "66045", "Risaralda", "66", "Apía", "APIA" },
                    { "66170", "Risaralda", "66", "Dosquebradas", "DOSQUEBRADAS" },
                    { "66594", "Risaralda", "66", "Santa Rosa de Cabal", "SANTA ROSA DE CABAL" },
                    { "68001", "Santander", "68", "Bucaramanga", "BUCARAMANGA" },
                    { "68081", "Santander", "68", "Barrancabermeja", "BARRANCABERMEJA" },
                    { "68276", "Santander", "68", "Floridablanca", "FLORIDABLANCA" },
                    { "68307", "Santander", "68", "Girón", "GIRON" },
                    { "68547", "Santander", "68", "Piedecuesta", "PIEDECUESTA" },
                    { "68615", "Santander", "68", "Sabana de Torres", "SABANA DE TORRES" },
                    { "68679", "Santander", "68", "San Gil", "SAN GIL" },
                    { "68755", "Santander", "68", "Socorro", "SOCORRO" },
                    { "68780", "Santander", "68", "Suaita", "SUAITA" },
                    { "68820", "Santander", "68", "Vélez", "VELEZ" },
                    { "70001", "Sucre", "70", "Sincelejo", "SINCELEJO" },
                    { "70215", "Sucre", "70", "Corozal", "COROZAL" },
                    { "70508", "Sucre", "70", "Ovejas", "OVEJAS" },
                    { "73001", "Tolima", "73", "Ibagué", "IBAGUE" },
                    { "73024", "Tolima", "73", "Alpujarra", "ALPUJARRA" },
                    { "73148", "Tolima", "73", "Chaparral", "CHAPARRAL" },
                    { "73268", "Tolima", "73", "Espinal", "ESPINAL" },
                    { "73349", "Tolima", "73", "Honda", "HONDA" },
                    { "73443", "Tolima", "73", "Melgar", "MELGAR" },
                    { "76001", "Valle del Cauca", "76", "Cali", "CALI" },
                    { "76020", "Valle del Cauca", "76", "Alcalá", "ALCALA" },
                    { "76036", "Valle del Cauca", "76", "Andalucía", "ANDALUCIA" },
                    { "76054", "Valle del Cauca", "76", "Armenia", "ARMENIA VALLE" },
                    { "76109", "Valle del Cauca", "76", "Buenaventura", "BUENAVENTURA" },
                    { "76111", "Valle del Cauca", "76", "Guadalajara de Buga", "GUADALAJARA DE BUGA" },
                    { "76122", "Valle del Cauca", "76", "Caicedonia", "CAICEDONIA" },
                    { "76130", "Valle del Cauca", "76", "Calima", "CALIMA" },
                    { "76147", "Valle del Cauca", "76", "Cartago", "CARTAGO" },
                    { "76275", "Valle del Cauca", "76", "Dagua", "DAGUA" },
                    { "76306", "Valle del Cauca", "76", "El Cerrito", "EL CERRITO" },
                    { "76364", "Valle del Cauca", "76", "Jamundí", "JAMUNDI" },
                    { "76377", "Valle del Cauca", "76", "La Cumbre", "LA CUMBRE" },
                    { "76400", "Valle del Cauca", "76", "La Unión", "LA UNION VALLE" },
                    { "76403", "Valle del Cauca", "76", "La Victoria", "LA VICTORIA" },
                    { "76497", "Valle del Cauca", "76", "Obando", "OBANDO" },
                    { "76520", "Valle del Cauca", "76", "Palmira", "PALMIRA" },
                    { "76563", "Valle del Cauca", "76", "Pradera", "PRADERA" },
                    { "76606", "Valle del Cauca", "76", "Roldanillo", "ROLDANILLO" },
                    { "76670", "Valle del Cauca", "76", "San Pedro", "SAN PEDRO VALLE" },
                    { "76736", "Valle del Cauca", "76", "Sevilla", "SEVILLA" },
                    { "76823", "Valle del Cauca", "76", "Toro", "TORO" },
                    { "76828", "Valle del Cauca", "76", "Trujillo", "TRUJILLO" },
                    { "76845", "Valle del Cauca", "76", "Tuluá", "TULUA" },
                    { "76863", "Valle del Cauca", "76", "Ulloa", "ULLOA" },
                    { "76869", "Valle del Cauca", "76", "Versalles", "VERSALLES" },
                    { "76890", "Valle del Cauca", "76", "Vijes", "VIJES" },
                    { "76892", "Valle del Cauca", "76", "Yotoco", "YOTOCO" },
                    { "76895", "Valle del Cauca", "76", "Yumbo", "YUMBO" },
                    { "76897", "Valle del Cauca", "76", "Zarzal", "ZARZAL" },
                    { "81001", "Arauca", "81", "Arauca", "ARAUCA" },
                    { "81736", "Arauca", "81", "Tame", "TAME" },
                    { "85001", "Casanare", "85", "Yopal", "YOPAL" },
                    { "85263", "Casanare", "85", "Paz de Ariporo", "PAZ DE ARIPORO" },
                    { "86001", "Putumayo", "86", "Mocoa", "MOCOA" },
                    { "86320", "Putumayo", "86", "La Hormiga", "LA HORMIGA" },
                    { "88001", "San Andrés", "88", "San Andrés", "SAN ANDRES" },
                    { "91001", "Amazonas", "91", "Leticia", "LETICIA" },
                    { "91263", "Amazonas", "91", "El Encanto", "EL ENCANTO" },
                    { "94001", "Guainía", "94", "Inírida", "INIRIDA" },
                    { "95001", "Guaviare", "95", "San José del Guaviare", "SAN JOSE DEL GUAVIARE" },
                    { "97001", "Vaupés", "97", "Mitú", "MITU" },
                    { "99001", "Vichada", "99", "Puerto Carreño", "PUERTO CARRENO" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DivipolaCodes_DeptCode",
                table: "DivipolaCodes",
                column: "DeptCode");

            migrationBuilder.CreateIndex(
                name: "IX_DivipolaCodes_MunicipioNormalized",
                table: "DivipolaCodes",
                column: "MunicipioNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_TenantId_DocType_DocNumber",
                table: "Patients",
                columns: new[] { "TenantId", "DocType", "DocNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DivipolaCodes");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
