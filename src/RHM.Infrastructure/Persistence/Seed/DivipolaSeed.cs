using RHM.Domain.Entities;
using System.Text;

namespace RHM.Infrastructure.Persistence.Seed;

/// <summary>
/// Seed del catálogo DIVIPOLA (DANE) — Colombia.
/// Incluye los 32 departamentos + D.C. y sus municipios principales.
/// El catálogo completo (~1122 municipios) se carga vía script SQL en producción.
/// Este seed cubre capitales y municipios de mayor población para el MVP.
/// </summary>
public static class DivipolaSeed
{
    public static IEnumerable<DivipolaCode> GetData() =>
    [
        // AMAZONAS (91)
        new() { MunCode = "91001", DeptCode = "91", Departamento = "Amazonas",         Municipio = "Leticia",              MunicipioNormalized = "LETICIA" },
        new() { MunCode = "91263", DeptCode = "91", Departamento = "Amazonas",         Municipio = "El Encanto",           MunicipioNormalized = "EL ENCANTO" },

        // ANTIOQUIA (05)
        new() { MunCode = "05001", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Medellín",             MunicipioNormalized = "MEDELLIN" },
        new() { MunCode = "05088", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Bello",                MunicipioNormalized = "BELLO" },
        new() { MunCode = "05129", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Caldas",               MunicipioNormalized = "CALDAS" },
        new() { MunCode = "05212", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Copacabana",           MunicipioNormalized = "COPACABANA" },
        new() { MunCode = "05266", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Envigado",             MunicipioNormalized = "ENVIGADO" },
        new() { MunCode = "05308", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Girardota",            MunicipioNormalized = "GIRARDOTA" },
        new() { MunCode = "05360", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Itagüí",               MunicipioNormalized = "ITAGUI" },
        new() { MunCode = "05380", DeptCode = "05", Departamento = "Antioquia",        Municipio = "La Estrella",          MunicipioNormalized = "LA ESTRELLA" },
        new() { MunCode = "05631", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Sabaneta",             MunicipioNormalized = "SABANETA" },
        new() { MunCode = "05045", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Apartadó",             MunicipioNormalized = "APARTADO" },
        new() { MunCode = "05147", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Carepa",               MunicipioNormalized = "CAREPA" },
        new() { MunCode = "05238", DeptCode = "05", Departamento = "Antioquia",        Municipio = "El Bagre",             MunicipioNormalized = "EL BAGRE" },
        new() { MunCode = "05604", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Rionegro",             MunicipioNormalized = "RIONEGRO" },
        new() { MunCode = "05615", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Samaná",               MunicipioNormalized = "SAMANA" },
        new() { MunCode = "05790", DeptCode = "05", Departamento = "Antioquia",        Municipio = "Turbo",                MunicipioNormalized = "TURBO" },

        // ARAUCA (81)
        new() { MunCode = "81001", DeptCode = "81", Departamento = "Arauca",           Municipio = "Arauca",               MunicipioNormalized = "ARAUCA" },
        new() { MunCode = "81736", DeptCode = "81", Departamento = "Arauca",           Municipio = "Tame",                 MunicipioNormalized = "TAME" },

        // ATLÁNTICO (08)
        new() { MunCode = "08001", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Barranquilla",         MunicipioNormalized = "BARRANQUILLA" },
        new() { MunCode = "08078", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Baranoa",              MunicipioNormalized = "BARANOA" },
        new() { MunCode = "08137", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Campo de la Cruz",     MunicipioNormalized = "CAMPO DE LA CRUZ" },
        new() { MunCode = "08296", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Galapa",               MunicipioNormalized = "GALAPA" },
        new() { MunCode = "08433", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Malambo",              MunicipioNormalized = "MALAMBO" },
        new() { MunCode = "08520", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Palmar de Varela",     MunicipioNormalized = "PALMAR DE VARELA" },
        new() { MunCode = "08549", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Polonuevo",            MunicipioNormalized = "POLONUEVO" },
        new() { MunCode = "08558", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Ponedera",             MunicipioNormalized = "PONEDERA" },
        new() { MunCode = "08634", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Santo Tomás",          MunicipioNormalized = "SANTO TOMAS" },
        new() { MunCode = "08675", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Sabanagrande",         MunicipioNormalized = "SABANAGRANDE" },
        new() { MunCode = "08685", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Sabanalarga",          MunicipioNormalized = "SABANALARGA" },
        new() { MunCode = "08758", DeptCode = "08", Departamento = "Atlántico",        Municipio = "Soledad",              MunicipioNormalized = "SOLEDAD" },

        // BOGOTÁ D.C. (11)
        new() { MunCode = "11001", DeptCode = "11", Departamento = "Bogotá D.C.",      Municipio = "Bogotá D.C.",          MunicipioNormalized = "BOGOTA" },

        // BOLÍVAR (13)
        new() { MunCode = "13001", DeptCode = "13", Departamento = "Bolívar",          Municipio = "Cartagena de Indias",  MunicipioNormalized = "CARTAGENA DE INDIAS" },
        new() { MunCode = "13140", DeptCode = "13", Departamento = "Bolívar",          Municipio = "Calamar",              MunicipioNormalized = "CALAMAR" },
        new() { MunCode = "13430", DeptCode = "13", Departamento = "Bolívar",          Municipio = "Magangué",             MunicipioNormalized = "MAGANGUE" },
        new() { MunCode = "13442", DeptCode = "13", Departamento = "Bolívar",          Municipio = "Mompós",               MunicipioNormalized = "MOMPOS" },
        new() { MunCode = "13620", DeptCode = "13", Departamento = "Bolívar",          Municipio = "San Juan Nepomuceno",  MunicipioNormalized = "SAN JUAN NEPOMUCENO" },

        // BOYACÁ (15)
        new() { MunCode = "15001", DeptCode = "15", Departamento = "Boyacá",           Municipio = "Tunja",                MunicipioNormalized = "TUNJA" },
        new() { MunCode = "15176", DeptCode = "15", Departamento = "Boyacá",           Municipio = "Chiquinquirá",         MunicipioNormalized = "CHIQUINQUIRA" },
        new() { MunCode = "15238", DeptCode = "15", Departamento = "Boyacá",           Municipio = "Duitama",              MunicipioNormalized = "DUITAMA" },
        new() { MunCode = "15660", DeptCode = "15", Departamento = "Boyacá",           Municipio = "Sogamoso",             MunicipioNormalized = "SOGAMOSO" },

        // CALDAS (17)
        new() { MunCode = "17001", DeptCode = "17", Departamento = "Caldas",           Municipio = "Manizales",            MunicipioNormalized = "MANIZALES" },
        new() { MunCode = "17042", DeptCode = "17", Departamento = "Caldas",           Municipio = "Anserma",              MunicipioNormalized = "ANSERMA" },
        new() { MunCode = "17380", DeptCode = "17", Departamento = "Caldas",           Municipio = "La Dorada",            MunicipioNormalized = "LA DORADA" },
        new() { MunCode = "17541", DeptCode = "17", Departamento = "Caldas",           Municipio = "Riosucio",             MunicipioNormalized = "RIOSUCIO" },

        // CAQUETÁ (18)
        new() { MunCode = "18001", DeptCode = "18", Departamento = "Caquetá",          Municipio = "Florencia",            MunicipioNormalized = "FLORENCIA" },

        // CASANARE (85)
        new() { MunCode = "85001", DeptCode = "85", Departamento = "Casanare",         Municipio = "Yopal",                MunicipioNormalized = "YOPAL" },
        new() { MunCode = "85263", DeptCode = "85", Departamento = "Casanare",         Municipio = "Paz de Ariporo",       MunicipioNormalized = "PAZ DE ARIPORO" },

        // CAUCA (19)
        new() { MunCode = "19001", DeptCode = "19", Departamento = "Cauca",            Municipio = "Popayán",              MunicipioNormalized = "POPAYAN" },
        new() { MunCode = "19698", DeptCode = "19", Departamento = "Cauca",            Municipio = "Santander de Quilichao", MunicipioNormalized = "SANTANDER DE QUILICHAO" },

        // CESAR (20)
        new() { MunCode = "20001", DeptCode = "20", Departamento = "Cesar",            Municipio = "Valledupar",           MunicipioNormalized = "VALLEDUPAR" },
        new() { MunCode = "20045", DeptCode = "20", Departamento = "Cesar",            Municipio = "Aguachica",            MunicipioNormalized = "AGUACHICA" },

        // CHOCÓ (27)
        new() { MunCode = "27001", DeptCode = "27", Departamento = "Chocó",            Municipio = "Quibdó",               MunicipioNormalized = "QUIBDO" },
        new() { MunCode = "27413", DeptCode = "27", Departamento = "Chocó",            Municipio = "Lloró",                MunicipioNormalized = "LLORO" },

        // CÓRDOBA (23)
        new() { MunCode = "23001", DeptCode = "23", Departamento = "Córdoba",          Municipio = "Montería",             MunicipioNormalized = "MONTERIA" },
        new() { MunCode = "23162", DeptCode = "23", Departamento = "Córdoba",          Municipio = "Cereté",               MunicipioNormalized = "CERETE" },
        new() { MunCode = "23464", DeptCode = "23", Departamento = "Córdoba",          Municipio = "Montelíbano",          MunicipioNormalized = "MONTELIBANO" },
        new() { MunCode = "23672", DeptCode = "23", Departamento = "Córdoba",          Municipio = "Sahagún",              MunicipioNormalized = "SAHAGUN" },

        // CUNDINAMARCA (25)
        new() { MunCode = "25175", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Chía",                 MunicipioNormalized = "CHIA" },
        new() { MunCode = "25269", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Facatativá",           MunicipioNormalized = "FACATATIVA" },
        new() { MunCode = "25290", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Fusagasugá",           MunicipioNormalized = "FUSAGASUGA" },
        new() { MunCode = "25307", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Girardot",             MunicipioNormalized = "GIRARDOT" },
        new() { MunCode = "25430", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Madrid",               MunicipioNormalized = "MADRID" },
        new() { MunCode = "25473", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Mosquera",             MunicipioNormalized = "MOSQUERA" },
        new() { MunCode = "25486", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Nemocón",              MunicipioNormalized = "NEMOCON" },
        new() { MunCode = "25513", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Pacho",                MunicipioNormalized = "PACHO" },
        new() { MunCode = "25754", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Soacha",               MunicipioNormalized = "SOACHA" },
        new() { MunCode = "25758", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Sogamoso",             MunicipioNormalized = "SOGAMOSO CUND" },
        new() { MunCode = "25769", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Subachoque",           MunicipioNormalized = "SUBACHOQUE" },
        new() { MunCode = "25785", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Tabio",                MunicipioNormalized = "TABIO" },
        new() { MunCode = "25817", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Tocancipá",            MunicipioNormalized = "TOCANCIPA" },
        new() { MunCode = "25899", DeptCode = "25", Departamento = "Cundinamarca",     Municipio = "Zipaquirá",            MunicipioNormalized = "ZIPAQUIRA" },

        // GUAINÍA (94)
        new() { MunCode = "94001", DeptCode = "94", Departamento = "Guainía",          Municipio = "Inírida",              MunicipioNormalized = "INIRIDA" },

        // GUAVIARE (95)
        new() { MunCode = "95001", DeptCode = "95", Departamento = "Guaviare",         Municipio = "San José del Guaviare", MunicipioNormalized = "SAN JOSE DEL GUAVIARE" },

        // HUILA (41)
        new() { MunCode = "41001", DeptCode = "41", Departamento = "Huila",            Municipio = "Neiva",                MunicipioNormalized = "NEIVA" },
        new() { MunCode = "41006", DeptCode = "41", Departamento = "Huila",            Municipio = "Acevedo",              MunicipioNormalized = "ACEVEDO" },
        new() { MunCode = "41298", DeptCode = "41", Departamento = "Huila",            Municipio = "Garzón",               MunicipioNormalized = "GARZON" },
        new() { MunCode = "41524", DeptCode = "41", Departamento = "Huila",            Municipio = "Pitalito",             MunicipioNormalized = "PITALITO" },

        // LA GUAJIRA (44)
        new() { MunCode = "44001", DeptCode = "44", Departamento = "La Guajira",       Municipio = "Riohacha",             MunicipioNormalized = "RIOHACHA" },
        new() { MunCode = "44430", DeptCode = "44", Departamento = "La Guajira",       Municipio = "Maicao",               MunicipioNormalized = "MAICAO" },
        new() { MunCode = "44560", DeptCode = "44", Departamento = "La Guajira",       Municipio = "San Juan del Cesar",   MunicipioNormalized = "SAN JUAN DEL CESAR" },
        new() { MunCode = "44847", DeptCode = "44", Departamento = "La Guajira",       Municipio = "Uribia",               MunicipioNormalized = "URIBIA" },

        // MAGDALENA (47)
        new() { MunCode = "47001", DeptCode = "47", Departamento = "Magdalena",        Municipio = "Santa Marta",          MunicipioNormalized = "SANTA MARTA" },
        new() { MunCode = "47053", DeptCode = "47", Departamento = "Magdalena",        Municipio = "Aracataca",            MunicipioNormalized = "ARACATACA" },
        new() { MunCode = "47170", DeptCode = "47", Departamento = "Magdalena",        Municipio = "Ciénaga",              MunicipioNormalized = "CIENAGA" },
        new() { MunCode = "47460", DeptCode = "47", Departamento = "Magdalena",        Municipio = "Mompós",               MunicipioNormalized = "MOMPOS MAG" },

        // META (50)
        new() { MunCode = "50001", DeptCode = "50", Departamento = "Meta",             Municipio = "Villavicencio",        MunicipioNormalized = "VILLAVICENCIO" },
        new() { MunCode = "50006", DeptCode = "50", Departamento = "Meta",             Municipio = "Acacías",              MunicipioNormalized = "ACACIAS" },
        new() { MunCode = "50313", DeptCode = "50", Departamento = "Meta",             Municipio = "Granada",              MunicipioNormalized = "GRANADA META" },

        // NARIÑO (52)
        new() { MunCode = "52001", DeptCode = "52", Departamento = "Nariño",           Municipio = "Pasto",                MunicipioNormalized = "PASTO" },
        new() { MunCode = "52356", DeptCode = "52", Departamento = "Nariño",           Municipio = "Ipiales",              MunicipioNormalized = "IPIALES" },
        new() { MunCode = "52480", DeptCode = "52", Departamento = "Nariño",           Municipio = "La Unión",             MunicipioNormalized = "LA UNION NARINO" },
        new() { MunCode = "52835", DeptCode = "52", Departamento = "Nariño",           Municipio = "Tumaco",               MunicipioNormalized = "TUMACO" },

        // NORTE DE SANTANDER (54)
        new() { MunCode = "54001", DeptCode = "54", Departamento = "Norte de Santander", Municipio = "Cúcuta",             MunicipioNormalized = "CUCUTA" },
        new() { MunCode = "54174", DeptCode = "54", Departamento = "Norte de Santander", Municipio = "Chinácota",          MunicipioNormalized = "CHINACOTA" },
        new() { MunCode = "54206", DeptCode = "54", Departamento = "Norte de Santander", Municipio = "Convención",         MunicipioNormalized = "CONVENCION" },
        new() { MunCode = "54518", DeptCode = "54", Departamento = "Norte de Santander", Municipio = "Ocaña",              MunicipioNormalized = "OCANA" },
        new() { MunCode = "54720", DeptCode = "54", Departamento = "Norte de Santander", Municipio = "Tibú",               MunicipioNormalized = "TIBU" },
        new() { MunCode = "54810", DeptCode = "54", Departamento = "Norte de Santander", Municipio = "Villa del Rosario",  MunicipioNormalized = "VILLA DEL ROSARIO" },

        // PUTUMAYO (86)
        new() { MunCode = "86001", DeptCode = "86", Departamento = "Putumayo",         Municipio = "Mocoa",                MunicipioNormalized = "MOCOA" },
        new() { MunCode = "86320", DeptCode = "86", Departamento = "Putumayo",         Municipio = "La Hormiga",           MunicipioNormalized = "LA HORMIGA" },

        // QUINDÍO (63)
        new() { MunCode = "63001", DeptCode = "63", Departamento = "Quindío",          Municipio = "Armenia",              MunicipioNormalized = "ARMENIA" },
        new() { MunCode = "63111", DeptCode = "63", Departamento = "Quindío",          Municipio = "Buenavista",           MunicipioNormalized = "BUENAVISTA QUINDIO" },
        new() { MunCode = "63548", DeptCode = "63", Departamento = "Quindío",          Municipio = "Quimbaya",             MunicipioNormalized = "QUIMBAYA" },

        // RISARALDA (66)
        new() { MunCode = "66001", DeptCode = "66", Departamento = "Risaralda",        Municipio = "Pereira",              MunicipioNormalized = "PEREIRA" },
        new() { MunCode = "66045", DeptCode = "66", Departamento = "Risaralda",        Municipio = "Apía",                 MunicipioNormalized = "APIA" },
        new() { MunCode = "66170", DeptCode = "66", Departamento = "Risaralda",        Municipio = "Dosquebradas",         MunicipioNormalized = "DOSQUEBRADAS" },
        new() { MunCode = "66594", DeptCode = "66", Departamento = "Risaralda",        Municipio = "Santa Rosa de Cabal",  MunicipioNormalized = "SANTA ROSA DE CABAL" },

        // SAN ANDRÉS (88)
        new() { MunCode = "88001", DeptCode = "88", Departamento = "San Andrés",       Municipio = "San Andrés",           MunicipioNormalized = "SAN ANDRES" },

        // SANTANDER (68)
        new() { MunCode = "68001", DeptCode = "68", Departamento = "Santander",        Municipio = "Bucaramanga",          MunicipioNormalized = "BUCARAMANGA" },
        new() { MunCode = "68081", DeptCode = "68", Departamento = "Santander",        Municipio = "Barrancabermeja",      MunicipioNormalized = "BARRANCABERMEJA" },
        new() { MunCode = "68276", DeptCode = "68", Departamento = "Santander",        Municipio = "Floridablanca",        MunicipioNormalized = "FLORIDABLANCA" },
        new() { MunCode = "68307", DeptCode = "68", Departamento = "Santander",        Municipio = "Girón",                MunicipioNormalized = "GIRON" },
        new() { MunCode = "68547", DeptCode = "68", Departamento = "Santander",        Municipio = "Piedecuesta",          MunicipioNormalized = "PIEDECUESTA" },
        new() { MunCode = "68615", DeptCode = "68", Departamento = "Santander",        Municipio = "Sabana de Torres",     MunicipioNormalized = "SABANA DE TORRES" },
        new() { MunCode = "68679", DeptCode = "68", Departamento = "Santander",        Municipio = "San Gil",              MunicipioNormalized = "SAN GIL" },
        new() { MunCode = "68755", DeptCode = "68", Departamento = "Santander",        Municipio = "Socorro",              MunicipioNormalized = "SOCORRO" },
        new() { MunCode = "68780", DeptCode = "68", Departamento = "Santander",        Municipio = "Suaita",               MunicipioNormalized = "SUAITA" },
        new() { MunCode = "68820", DeptCode = "68", Departamento = "Santander",        Municipio = "Vélez",                MunicipioNormalized = "VELEZ" },

        // SUCRE (70)
        new() { MunCode = "70001", DeptCode = "70", Departamento = "Sucre",            Municipio = "Sincelejo",            MunicipioNormalized = "SINCELEJO" },
        new() { MunCode = "70215", DeptCode = "70", Departamento = "Sucre",            Municipio = "Corozal",              MunicipioNormalized = "COROZAL" },
        new() { MunCode = "70508", DeptCode = "70", Departamento = "Sucre",            Municipio = "Ovejas",               MunicipioNormalized = "OVEJAS" },

        // TOLIMA (73)
        new() { MunCode = "73001", DeptCode = "73", Departamento = "Tolima",           Municipio = "Ibagué",               MunicipioNormalized = "IBAGUE" },
        new() { MunCode = "73024", DeptCode = "73", Departamento = "Tolima",           Municipio = "Alpujarra",            MunicipioNormalized = "ALPUJARRA" },
        new() { MunCode = "73148", DeptCode = "73", Departamento = "Tolima",           Municipio = "Chaparral",            MunicipioNormalized = "CHAPARRAL" },
        new() { MunCode = "73268", DeptCode = "73", Departamento = "Tolima",           Municipio = "Espinal",              MunicipioNormalized = "ESPINAL" },
        new() { MunCode = "73349", DeptCode = "73", Departamento = "Tolima",           Municipio = "Honda",                MunicipioNormalized = "HONDA" },
        new() { MunCode = "73443", DeptCode = "73", Departamento = "Tolima",           Municipio = "Melgar",               MunicipioNormalized = "MELGAR" },

        // VALLE DEL CAUCA (76)
        new() { MunCode = "76001", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Cali",                 MunicipioNormalized = "CALI" },
        new() { MunCode = "76020", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Alcalá",               MunicipioNormalized = "ALCALA" },
        new() { MunCode = "76036", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Andalucía",            MunicipioNormalized = "ANDALUCIA" },
        new() { MunCode = "76054", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Armenia",              MunicipioNormalized = "ARMENIA VALLE" },
        new() { MunCode = "76109", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Buenaventura",         MunicipioNormalized = "BUENAVENTURA" },
        new() { MunCode = "76111", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Guadalajara de Buga",  MunicipioNormalized = "GUADALAJARA DE BUGA" },
        new() { MunCode = "76122", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Caicedonia",           MunicipioNormalized = "CAICEDONIA" },
        new() { MunCode = "76130", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Calima",               MunicipioNormalized = "CALIMA" },
        new() { MunCode = "76147", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Cartago",              MunicipioNormalized = "CARTAGO" },
        new() { MunCode = "76275", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Dagua",                MunicipioNormalized = "DAGUA" },
        new() { MunCode = "76306", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "El Cerrito",           MunicipioNormalized = "EL CERRITO" },
        new() { MunCode = "76364", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Jamundí",              MunicipioNormalized = "JAMUNDI" },
        new() { MunCode = "76377", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "La Cumbre",            MunicipioNormalized = "LA CUMBRE" },
        new() { MunCode = "76400", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "La Unión",             MunicipioNormalized = "LA UNION VALLE" },
        new() { MunCode = "76403", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "La Victoria",         MunicipioNormalized = "LA VICTORIA" },
        new() { MunCode = "76497", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Obando",               MunicipioNormalized = "OBANDO" },
        new() { MunCode = "76520", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Palmira",              MunicipioNormalized = "PALMIRA" },
        new() { MunCode = "76563", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Pradera",              MunicipioNormalized = "PRADERA" },
        new() { MunCode = "76606", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Roldanillo",           MunicipioNormalized = "ROLDANILLO" },
        new() { MunCode = "76670", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "San Pedro",            MunicipioNormalized = "SAN PEDRO VALLE" },
        new() { MunCode = "76736", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Sevilla",              MunicipioNormalized = "SEVILLA" },
        new() { MunCode = "76823", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Toro",                 MunicipioNormalized = "TORO" },
        new() { MunCode = "76828", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Trujillo",             MunicipioNormalized = "TRUJILLO" },
        new() { MunCode = "76845", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Tuluá",                MunicipioNormalized = "TULUA" },
        new() { MunCode = "76863", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Ulloa",                MunicipioNormalized = "ULLOA" },
        new() { MunCode = "76869", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Versalles",            MunicipioNormalized = "VERSALLES" },
        new() { MunCode = "76890", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Vijes",                MunicipioNormalized = "VIJES" },
        new() { MunCode = "76892", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Yotoco",               MunicipioNormalized = "YOTOCO" },
        new() { MunCode = "76895", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Yumbo",                MunicipioNormalized = "YUMBO" },
        new() { MunCode = "76897", DeptCode = "76", Departamento = "Valle del Cauca",  Municipio = "Zarzal",               MunicipioNormalized = "ZARZAL" },

        // VAUPÉS (97)
        new() { MunCode = "97001", DeptCode = "97", Departamento = "Vaupés",           Municipio = "Mitú",                 MunicipioNormalized = "MITU" },

        // VICHADA (99)
        new() { MunCode = "99001", DeptCode = "99", Departamento = "Vichada",          Municipio = "Puerto Carreño",       MunicipioNormalized = "PUERTO CARRENO" },
    ];
}
