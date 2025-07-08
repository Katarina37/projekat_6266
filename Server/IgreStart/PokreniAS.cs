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

            bool[] kolonaRijesena = new bool[4];
            bool konacnoRijeseno = false;
            int maxGresaka = 5;
            Dictionary<SesijaIgraca, int> greske = igraci.ToDictionary(i => i, i => 0);
            Dictionary<SesijaIgraca, int> poeniIgraca = igraci.ToDictionary(i => i, i => 0);

            int igracNaPotezu = 0;

            while (!konacnoRijeseno && greske.All(g => g.Value < maxGresaka))
            {
                for (int i = 0; i < igraci.Count; i++)
                {
                    var igrac = igraci[i];
                    string stanje = igra.PrikaziStanje();
                    string drugo = "Protivnik je na potezu.\n";

                    if (i == igracNaPotezu)
                    {
                        stanje += "\n [TI SI NA POTEZU]: ";
                        igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(stanje));
                    } else
                    {
                        igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(drugo));
                    }

                }

                var trenutniIgrac = igraci[igracNaPotezu];


                if (trenutniIgrac.KlijentSocket.Poll(30 * 1000 * 1000, System.Net.Sockets.SelectMode.SelectRead))
                {
                    byte[] prijemniBafer = new byte[2048];
                    int brBajta = trenutniIgrac.KlijentSocket.Receive(prijemniBafer);
                    string potez = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta).Trim();

                    string odgovor;

                    if (potez.Length == 2 && char.IsLetter(potez[0]) && char.IsDigit(potez[1]))
                    {
                        string rez = igra.OtvoriPolje(potez.ToUpper());
                        odgovor = $"Otvoreno polje {potez.ToUpper()}: {rez}\n";
                        Console.ForegroundColor = trenutniIgrac.Boja;
                        Console.WriteLine($"Igrac {trenutniIgrac.Igrac.Nadimak} je otvorio polje {potez.ToUpper()} => {rez} \n");
                        Console.ResetColor();
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
                                        poeniIgraca[trenutniIgrac] += igra.IzracunajPoeneKolona(i);
                                    }
                                }

                                poeniIgraca[trenutniIgrac] += igra.PoeniKonacno;
                                igra.OtvoriSve();
                                odgovor = $" {igra.PrikaziStanje()} \nTacno! Osvojili ste {igra.PoeniKonacno} poena za konacno rjesenje!\n";
                                Console.ForegroundColor = trenutniIgrac.Boja;
                                Console.WriteLine($"Igrac {trenutniIgrac.Igrac.Nadimak} : {potez}");
                                Console.WriteLine("Igrac je pogodio konacno rjesenje.");
                                Console.ResetColor();

                            }
                            else
                            {
                                greske[trenutniIgrac]++;
                                odgovor = "Netacno konacno rjesenje!\n";
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(odgovor);
                                Console.ResetColor();
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
                                    poeniIgraca[trenutniIgrac] += poeniKolona;
                                    kolonaRijesena[kolonaIdx] = true;
                                    igra.OtvoriSvaPoljaKolone(kolonaIdx);
                                    odgovor = $"Tacno rjesenje kolone {kolona}! Osvojili ste {poeniKolona} poena.\n";
                                    Console.ForegroundColor = trenutniIgrac.Boja;
                                    Console.WriteLine($"Igrac {trenutniIgrac.Igrac.Nadimak}: {potez}");
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"Tacno rjesenje kolone {kolona}.");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    greske[trenutniIgrac]++;
                                    odgovor = $"Netacno rjesenje kolone {kolona}.\n";
                                    Console.ForegroundColor = trenutniIgrac.Boja;
                                    Console.WriteLine($"Igrac {trenutniIgrac.Igrac.Nadimak}: {potez}");
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine(odgovor);
                                    Console.ResetColor();

                                }
                            }
                            else
                            {
                                odgovor = "Kolona je vec rijesena!\n";
                                Console.ForegroundColor = trenutniIgrac.Boja;
                                Console.WriteLine($"Igrac {trenutniIgrac.Igrac.Nadimak}: {potez}");
                                Console.ResetColor();
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(odgovor);
                                Console.ResetColor();
                            }
                        }
                    }
                    else
                    {
                        odgovor = "Neispravan unos.\n";
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(odgovor);
                        Console.ResetColor();
                    }

                    byte[] poruka = Encoding.UTF8.GetBytes(odgovor);
                    trenutniIgrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(odgovor));

                    if (greske[trenutniIgrac] >= maxGresaka)
                    {
                        trenutniIgrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("Dostigli ste maksimalan broj gresaka.\n"));
                    }

                    igracNaPotezu = (igracNaPotezu + 1) % igraci.Count;

                } else
                {
                    trenutniIgrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("Isteklo Vam je vrijeme. Protivnik je na potezu.\n"));
                    igracNaPotezu = (igracNaPotezu + 1) % igraci.Count;
                }


            }

            Console.WriteLine("Igra asocijacije je zavrsena!");

            for(int i = 0; i < igraci.Count; i++)
            {
                igraci[i].Igrac.DodajPoene(brojIgre, poeniIgraca[igraci[i]]);
                string kraj = $"Igra Asocijacije je zavrsena! Ukupno ste osvojili {poeniIgraca[igraci[i]]} poena.\n";
                igraci[i].KlijentSocket.Send(Encoding.UTF8.GetBytes(kraj));

                Console.ForegroundColor = igraci[i].Boja;
                Console.WriteLine($"Igrac {igraci[i].Igrac.Nadimak} osvojio je {poeniIgraca[igraci[i]]} poena.");
                Console.ResetColor();
                igraci[i].ZavrsioIgru = true;
            }

        }
    }
}
