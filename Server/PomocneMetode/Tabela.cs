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
                sb.Append($" {PuniNaziviIgara[igra],-9} |");
            }

            sb.Append(" Ukupno\n");
            sb.AppendLine(new string('-', 15 + igre.Length * 14 + 12));

            foreach (var igrac in sviIgraci)
            {
                var igracTren = igrac.Igrac;
                sb.Append($" {igracTren.Nadimak,-11}|");

                foreach (var poeni in igracTren.PoeniPoIgrama)
                {
                    sb.Append($" {poeni,13} |");
                }

                sb.AppendLine($" {igracTren.UkupanBrojPoena(),9}");
            }

            sb.AppendLine("--------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
