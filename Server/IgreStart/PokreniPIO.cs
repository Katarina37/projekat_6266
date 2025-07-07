using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Klase;

namespace Server.IgreStart
{
    public class PokreniPIO
    {
        public static void PitanjaOdgovoriIgra(List<SesijaIgraca> igraci, int brojIgre)
        {
            Console.WriteLine("----------IGRA PITANJA I ODGOVORI----------");
            //PitanjaOdgovori pitanjaOdgovori = new PitanjaOdgovori();

            /* while (true)
             {
                 string pitanje = pitanjaOdgovori.PostaviSljedecePitanje();
                 if (pitanje == "Igra je zavrsena! Nema vise pitanja.")
                 {
                     klijentSocket.Send(Encoding.UTF8.GetBytes("KRAJ"));
                     break;
                 }

                 byte[] pitanjeBajti = Encoding.UTF8.GetBytes($"Pitanje: {pitanje} \n Odgovorite sa'A' za tacno ili 'B' za netacno");
                 klijentSocket.Send(pitanjeBajti);

                 byte[] prijemniBafer = new byte[1024];
                 int brBajta = klijentSocket.Receive(prijemniBafer);
                 string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                 string rezultat = pitanjaOdgovori.ProvjeriOdgovor(odgovor[0]);
                 Console.WriteLine(rezultat);
                 klijentSocket.Send(Encoding.UTF8.GetBytes(rezultat));
             }

             int ukupniPoeni = pitanjaOdgovori.UkupnoPoena();
             klijentSocket.Send(Encoding.UTF8.GetBytes($"Osvojili ste ukupno {ukupniPoeni} poena"));

             Console.WriteLine($"Igra pitanja i odgovori je zavrsena. Igrac je osvojio {ukupniPoeni} poena.");*/
            Console.WriteLine("-----------------------------------------------------------\n");


        }
    }
}
