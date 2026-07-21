
namespace API.Constantes
{
    public static class CardConstants
    {
        public static readonly Dictionary<string, string> codeTarifNom = new Dictionary<string, string>()
        {
            { "CL012", "CARTE HORIZON" },
            { "CL011", "VISA PLATINUM" },
            { "CL014", "VISA FREE" },
            { "CL015", "VISA YOUNG" },
            { "CL006", "VISA PREMIER" },
            { "CL007", "VISA CLASSIC" },
            { "CL016", "VISA INFINITE" },
            { "PR011", "VISA PLATINUM" },
            { "PR012", "CARTE HORIZON" },
            { "PR013", "VISA HORIZON PREPAYEE" },
            { "PR016", "VISA INFINITE" },
            { "PR005", "VISA BUSINESS" },
            { "PR006", "VISA PREMIER" },
            { "PR007", "VISA CLASSIC" },
            { "C3011", "CARTE PLATINUM" },
            { "C3012", "CARTE HORIZON" },
            { "C3016", "CARTE INFINITE" },
            { "C3006", "VISA PREMIER" },
            { "C3007", "VISA CLASSIC" },
            { "EX011", "CARTE PLATINUM" },
            { "EX012", "CARTE HORIZON" },
            { "EX013", "VISA HORIZON PREPAYEE" },
            { "EX016", "CARTE INFINITE" },
            { "EX005", "VISA BUSINNESS" },
            { "EX006", "VISA PREMIER" },
            { "EX007", "VISA CLASSIC" },
        };

        public static readonly Dictionary<string, long> cartePrix = new Dictionary<string, long>
        {
            { "012", 2504 },
            { "013", 2504 },
            { "007", 4770 },
            { "006", 8705 },
            { "011", 14906 },
            { "016", 29813 },
        };

        public static readonly Dictionary<string, List<string>> cartePackage = new()
        {
            { "012", new List<string> { "300001", "300002" } },
            { "013", new List<string> { "300001", "300002" } },
            { "007", new List<string> { "300003" } },
            { "006", new List<string> { "300004" } },
            { "011", new List<string> { "300007" } },
            { "016", new List<string> { "300008" } }
        };

        //public static readonly Dictionary<string, string> cartePackage = new Dictionary<string, string>
        //{
        //    { "012", "300001" }, // Carte visa access/horizon
        //    { "012", "300002" }, // Carte visa access/horizon
        //    { "007", "300003" }, // Carte visa classic
        //    { "006", "300004" }, // Carte visa premier
        //    { "011", "300007" }, // Carte visa platinum
        //    { "016", "300008" }, // Carte visa infinite
        //};

        // exlusion des cartes visa free, young, businnes
        public static readonly string[] cartesExclu = { "014", "015", "005", "013" };

        public static readonly string[] cartesGratuite = { "CL", "EX", "PE", "PR" };

    }
}
