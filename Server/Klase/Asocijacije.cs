using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Klase
{
    public class Asocijacije
    {
        public string[][] Polja { get; set; }
        public string[] RjesenjaKolona { get; set; }
        public bool[][] OtvorenaPolja { get; set; }
        public string KonacnoRjesenje { get; set; }
        public bool KonacnoRijeseno { get; set; }
        public bool[] KoloneRijesene { get; set; }
        public int PoeniKonacno => 10;

        public Asocijacije()
        {
            Polja = new string[4][];
            OtvorenaPolja = new bool[4][];
            RjesenjaKolona = new string[4];
            KoloneRijesene = new bool[4];

        }

        public void UcitajAsocijaciju(string putanja)
        {
            var linije = File.ReadAllLines(putanja);

            if (linije.Length < 5)
            {
                Console.WriteLine("Nedovoljan broj linija u fajlu.\n");
                return;
            }

            Polja = new string[4][];
            RjesenjaKolona = new string[4];
            OtvorenaPolja = new bool[4][];

            for (int kolona = 0; kolona < 4; kolona++)
            {
                var dijelovi = linije[kolona].Split(',');

                if (dijelovi.Length != 5)
                {
                    Console.WriteLine($"Linija {kolona + 1} mora imati 5 elemenata.");
                    return;
                }

                Polja[kolona] = new string[4];
                for (int red = 0; red < 4; red++)
                {
                    Polja[kolona][red] = dijelovi[red].Trim();
                }

                RjesenjaKolona[kolona] = dijelovi[4].Trim();
                OtvorenaPolja[kolona] = new bool[4];
            }

            if (linije[4].StartsWith("KONACNO: ", StringComparison.OrdinalIgnoreCase))
            {
                KonacnoRjesenje = linije[4].Substring("KONACNO: ".Length).Trim();
            }
            else
            {
                Console.WriteLine("Greska!Nema konacnog rjesenja.");
            }

            KonacnoRijeseno = false;
        }

        public string OtvoriPolje(string oznaka)
        {
            int kolona = oznaka[0] - 'A';
            int red = int.Parse(oznaka[1].ToString()) - 1;

            if (kolona < 0 || kolona > 3 || red < 0 || red > 3)
                Console.WriteLine("Nepravilno unesena oznaka polja!");

            OtvorenaPolja[kolona][red] = true;

            return Polja[kolona][red];
        }

        public bool ProvjeriRjesenjeKolone(char kolonaOznaka, string odgovor)
        {
            int kolona = kolonaOznaka - 'A';
            if (kolona < 0 || kolona > 3)
            {
                Console.WriteLine("Nevazeca oznaka kolone!");
                return false;
            }

            string ocekivano = RjesenjaKolona[kolona]?.Trim().ToUpper() ?? "";
            string unos = odgovor.Trim().ToUpper();


            return ocekivano == unos;
        }

        public void OtvoriSvaPoljaKolone(int kolonaIndex)
        {
            for (int i = 0; i < OtvorenaPolja[kolonaIndex].Length; i++)
            {
                OtvorenaPolja[kolonaIndex][i] = true;
            }
            KoloneRijesene[kolonaIndex] = true;
        }

        public void OtvoriSve()
        {
            for (int i = 0; i < OtvorenaPolja.Length; i++)
            {
                for (int j = 0; j < OtvorenaPolja[i].Length; j++)
                {
                    OtvorenaPolja[i][j] = true;
                }
            }

            for (int i = 0; i < KoloneRijesene.Length; i++)
            {
                KoloneRijesene[i] = true;
            }

            KonacnoRijeseno = true;
        }

        public bool ProvjeriKonacnoRjesenje(string odgovor)
        {
            return (odgovor.Equals(KonacnoRjesenje, StringComparison.OrdinalIgnoreCase));
        }

        public string PrikaziStanje()
        {
            StringBuilder sb = new StringBuilder();

            for(int red = 0; red < 4; red++)
            {
                for(int kol = 0; kol < 4; kol++)
                {
                    string oznaka = $"{(char)('A' + kol)}{red + 1}";
                    string sadrzaj = OtvorenaPolja[kol][red] ? Polja[kol][red] : "???";
                    sb.Append($"{oznaka}: {sadrzaj}".PadRight(15));
                }

                sb.AppendLine();
            }

            for (int kol = 0; kol < 4; kol++)
            {
                string rjesenje = KoloneRijesene[kol] ? RjesenjaKolona[kol] : "???";
                sb.Append($"{(char)('A' + kol)}:{rjesenje}".PadRight(15));
            }
            sb.AppendLine();
            sb.AppendLine(new string('-', 60));

            sb.AppendLine($"KONACNO: {(KonacnoRijeseno ? KonacnoRjesenje : "???")}");

            return sb.ToString();

        }

        public int IzracunajPoeneKolona(int kolonaIndex)
        {
            if (kolonaIndex < 0 || kolonaIndex > 3)
                Console.WriteLine("Nevazeca oznaka kolone!");

            int neotvorena = OtvorenaPolja[kolonaIndex].Count(o => !o);
            return neotvorena + 2;
        }
    }
}
