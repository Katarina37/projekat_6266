using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Klase;
using System.Threading;
using System.Net.Sockets;

namespace Server.IgreStart
{
    public class PokreniPIO
    {
        public static void PitanjaOdgovoriIgra(List<SesijaIgraca> igraci, int brojIgre)
        {
            Console.WriteLine("----------IGRA PITANJA I ODGOVORI----------");
            PitanjaOdgovori pitanjaOdgovori = new PitanjaOdgovori();

            while (true)
            {
                string pitanje = pitanjaOdgovori.PostaviSljedecePitanje();

                if(pitanje == "Igra je zavrsena! Nema vise pitanja.")
                {
                    break;
                }

                foreach(var igrac in igraci)
                {
                    string tekst = $"[PITANJE] {pitanje}\n Odgovorite sa 'A' za tacno ili 'B' za netacno.\n";
                    igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(tekst));
                }


                foreach(var igrac in igraci)
                {
                    if(igrac.KlijentSocket.Poll(20*1000*1000, SelectMode.SelectRead))
                    {
                        byte[] odgovorBafer = new byte[1024];
                        int odgovorBr = igrac.KlijentSocket.Receive(odgovorBafer);
                        string odgovor = Encoding.UTF8.GetString(odgovorBafer, 0, odgovorBr).Trim();

                        string rezultat = pitanjaOdgovori.ProvjeriOdgovor(igrac.Igrac,odgovor[0]);

                        Console.ForegroundColor = igrac.Boja;
                        Console.WriteLine($"Igrac {igrac.Igrac.Nadimak} je odgovorio: {odgovor} => {rezultat}");
                        Console.ResetColor();

                        igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes($"[REZULTAT] : {rezultat} \n"));
                    }  else
                    {
                        igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("Niste odgovorili na vrijeme\n"));
                        continue;
                    }

                    Thread.Sleep(100);

                }
            }

            foreach(var igrac in igraci)
            {
                int poeni = pitanjaOdgovori.UkupnoPoena(igrac.Igrac);
                igrac.Igrac.DodajPoene(brojIgre, poeni);
                igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes($"[KRAJ] : Osvojili ste ukupno {poeni} poena.\n"));

                Console.ForegroundColor = igrac.Boja;
                Console.WriteLine($"Igra Pitanja i Odgovori je zavrsena. Igrac {igrac.Igrac.Nadimak} je osvojio {poeni} poena.\n");
                Console.ResetColor();
                igrac.ZavrsioIgru = true;
            }
            

            Console.WriteLine("-----------------------------------------------------------\n");
        }

    }
}
