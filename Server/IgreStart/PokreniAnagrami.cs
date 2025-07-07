using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Server.Klase;

namespace Server.IgreStart
{
    public class PokreniAnagrami
    {
        public static void AnagramiIgra(List<SesijaIgraca> igraci, int brojIgre)
        {
            Console.WriteLine("----------IGRA ANAGRAMI ----------\n");

            List<string> listaRijeci = new List<string> { "programiranje", "racunar", "fakultet", "univerzitet", "tastatura", "elektrotehnika" };
            Random random = new Random();
            string unesenaRijec = listaRijeci[random.Next(listaRijeci.Count)].ToLower();

            Console.WriteLine($"Izgenerisana rijec za anagram je: {unesenaRijec}");
            Anagrami anagrami = new Anagrami();
            anagrami.UcitajRijeci(unesenaRijec);

            string poruka = $"Vasa rijec za anagram je: \"{anagrami.PocetnaRijec}\".Posaljite validan anagram koristeci ista slova.\n";
            foreach (var igrac in igraci)
            {
                igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(poruka));

            }

            List<(SesijaIgraca igrac, string odgovor)> odgovori = new List<(SesijaIgraca igrac, string odgovor)>();
            foreach (var igrac in igraci)
            {
                string predlozenAnagram;

                while (true)
                {
                    if (igrac.KlijentSocket.Poll(1000 * 1000, SelectMode.SelectRead))
                    {
                        byte[] prijemniBafer = new byte[1024];
                        int brBajta = igrac.KlijentSocket.Receive(prijemniBafer);
                        predlozenAnagram = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta).Trim();
                        break;
                    }

                    Thread.Sleep(50);
                }

                Console.WriteLine($"Uneseni anagram: {predlozenAnagram}.");
                Console.WriteLine("Provjera u toku.");
                odgovori.Add((igrac, predlozenAnagram));
            }

            int rang = 0;


            foreach (var (igrac, odgovor) in odgovori)
            {
                anagrami.PostaviPredlozenAnagram(odgovor);
                bool ispravanAnagram = anagrami.ProvjeriAnagram();

                int poeniIgraca = 0;

                if (ispravanAnagram)
                {
                    poeniIgraca = (rang == 0) ? anagrami.IzracunajPoene() : (int)(0.85 * anagrami.IzracunajPoene());
                    rang++;
                    igrac.Igrac.DodajPoene(brojIgre, poeniIgraca);
                    igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("Anagram je validan.\n"));
                }
                else
                {
                    igrac.Igrac.DodajPoene(brojIgre, 0);
                    igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("Anagram nije validan.\n"));
                }


                igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes($"Osvojeni poeni: {poeniIgraca}\n"));

                Console.ForegroundColor = igrac.Boja;
                Console.WriteLine($"Igra anagrami je zavrsena. Igrac {igrac.Igrac.Nadimak} je osvojio {poeniIgraca} poena.\n");
                Console.ResetColor();
                igrac.ZavrsioIgru = true;
            }
            Console.WriteLine("-----------------------------------------------------------\n");
        }
    }
}
