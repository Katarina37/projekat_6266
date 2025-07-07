using System.Collections.Generic;
using System.Text;
using Server.Klase;

namespace Server.Pomocne_metode
{
    public class Tabela
    {
        static readonly Dictionary<string, string> PuniNaziviIgara = new Dictionary<string, string>
        {
            {"an", "Anagrami" },
            {"po", "Pitanja i odgovori" },
            {"as", "Asocijacije" }
        };

        public static string IspisiKonacnuTabelu(List<SesijaIgraca> sviIgraci, string[] igre)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("\n========= KONAČNA TABELA =========\n");
            sb.Append(" Igrac      |");
            foreach (string igra in igre)
            {
                sb.Append($" {PuniNaziviIgara[igra],-10} |");
            }

            sb.Append(" Ukupno\n");
            sb.AppendLine(new string('-', 13 + igre.Length * 14 + 8));

            foreach (var igrac in sviIgraci)
            {
                var igracTren = igrac.Igrac;
                sb.Append($" {igracTren.Nadimak,-11}|");

                foreach (var poeni in igracTren.PoeniPoIgrama)
                {
                    sb.Append($" {poeni,10} |");
                }

                sb.AppendLine($" {igracTren.UkupanBrojPoena(),6}");
            }

            sb.AppendLine("----------------------------------------------");
            return sb.ToString();
        }
    }
}
