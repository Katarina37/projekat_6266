using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Klase;

namespace Server
{
    public class PokreniAS
    {
        public static void AsocijcijeIgra(List<SesijaIgraca> igraci, int brojIgre)
        {
            Console.WriteLine("----------IGRA ASOCIJACIJE----------\n");

            Asocijacije igra = new Asocijacije();
            igra.UcitajAsocijaciju("asocijacije.txt");

            // bool[] kolonaRijesena = new bool[4];
            //bool konacnoRijeseno = false;
            //int brGresaka = 0;
            //int poeni = 0;
            //int maxGresaka = 5;
            /*
            while (true)
            {
                if (konacnoRijeseno || brGresaka >= maxGresaka)
                {
                    string kraj = $"Igra je zavrsena! Ukupno ste osvojili {poeni} poena!\n";
                    klijentSocket.Send(Encoding.UTF8.GetBytes(kraj));
                    break;
                }

                klijentSocket.Send(Encoding.UTF8.GetBytes(igra.PrikaziStanje()));

                byte[] prijemniBafer = new byte[2048];
                int brBajta = klijentSocket.Receive(prijemniBafer);
                string potez = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                string odgovor = "";

                if (potez.Length == 2 && char.IsLetter(potez[0]) && char.IsDigit(potez[1]))
                {
                    odgovor = igra.OtvoriPolje(potez.ToUpper());
                    odgovor = $"Otvoreno polje {potez.ToUpper()}: {odgovor}\n";
                    Console.WriteLine(odgovor);
                }
                else if (potez.Length > 2 && potez[1] == ':')
                {
                    if (potez.StartsWith("K:"))
                    {
                        string konacni = potez.Substring(2).Trim();
                        if (igra.ProvjeriKonacnoRjesenje(konacni))
                        {
                            konacnoRijeseno = true;
                            for (int i = 0; i < 4; i++)
                            {
                                if (!kolonaRijesena[i])
                                {
                                    kolonaRijesena[i] = true;
                                    poeni += igra.IzracunajPoeneKolona(i);
                                }
                            }

                            poeni += igra.PoeniKonacno;
                            igra.OtvoriSve();
                            odgovor = $" {igra.PrikaziStanje()} \nTacno! Osvojili ste {igra.PoeniKonacno} poena za konacno rjesenje!\n";
                            Console.WriteLine("Igrac je pogodio konacno rjesenje.");

                        }
                        else
                        {
                            brGresaka++;
                            odgovor = "Netacno konacno rjesenje!\n";
                            Console.WriteLine(odgovor);
                        }
                    }
                    else
                    {
                        char kolona = potez[0];
                        string kolonaOdg = potez.Substring(2).Trim();
                        int kolonaIdx = kolona - 'A';

                        if (!kolonaRijesena[kolonaIdx])
                        {
                            if (igra.ProvjeriRjesenjeKolone(kolona, kolonaOdg))
                            {
                                int poeniKolona = igra.IzracunajPoeneKolona(kolonaIdx);
                                poeni += poeniKolona;
                                kolonaRijesena[kolonaIdx] = true;
                                igra.OtvoriSvaPoljaKolone(kolonaIdx);
                                odgovor = $"Tacno rjesenje kolone {kolona}! Osvojili ste {poeniKolona} poena.\n";
                                Console.WriteLine($"Igrac: {potez}");
                                Console.WriteLine("Tacno rjesenje kolone.");
                            }
                            else
                            {
                                brGresaka++;
                                odgovor = "Netacno rjesenje kolone.\n";
                                Console.WriteLine(odgovor);
                            }
                        }
                        else
                        {
                            odgovor = "Kolona je vec rijesena!\n";
                            Console.WriteLine(odgovor);
                        }
                    }
                }
                else
                {
                    odgovor = "Neispravan unos.\n";
                }

                klijentSocket.Send(Encoding.UTF8.GetBytes(odgovor));
            }


            Console.WriteLine($"Igra asocijacije je zavrsena. Igrac je osvojio {poeni} poena.");*/
            Console.WriteLine("-----------------------------------------------------------\n");


        }
    }
}
