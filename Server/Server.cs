using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Klase;
using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;

namespace Server
{
    public class Server
    {
        static void Main(string[] args)
        {
            Socket serverUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50009);
            serverUDP.Bind(serverEP);

            Console.WriteLine("Server je pokrenut i ceka prijavu igraca...\n");
            EndPoint klijentEP = new IPEndPoint(IPAddress.Any, 0);

            string imeIgraca;
            string listaIgara;
            string[] igre;
            int brojIgara;
            Igrac igrac = null;


            while (true)
            {
                byte[] prijemniBafer = new byte[1024];

                try
                {
                    int brBajta = serverUDP.ReceiveFrom(prijemniBafer, ref klijentEP);
                    string primljenaPoruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                    Console.WriteLine("Prijava je stigla. Provjera u toku...\n");
                    //validacija prijave igraca
                    if (primljenaPoruka.StartsWith("PRIJAVA: "))
                    {
                        string[] dijelovi = primljenaPoruka.Substring(9).Split(new char[] { ',' }, 2);


                        if (dijelovi.Length != 2)
                        {
                            Console.WriteLine("Neispravan format poruke za prijavu!");
                        }

                        imeIgraca = dijelovi[0].Trim();
                        listaIgara = dijelovi[1].Trim();

                        igre = listaIgara.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
                        string[] validneIgre = { "an", "po", "as" };
                        bool ispravnaListaIgara = igre.All(igra => validneIgre.Contains(igra.Trim()));

                        if (!ispravnaListaIgara)
                        {
                            byte[] odgovorIgre = Encoding.UTF8.GetBytes("Neispravna lista igara.Pokusajte ponovo!\n");
                            serverUDP.SendTo(odgovorIgre, klijentEP);
                            continue;
                        }
                        else
                        {
                            brojIgara = igre.Length;

                            igrac = new Igrac(imeIgraca, brojIgara);

                            string prijavaOdgovor = "Prijava je uspjesna.Uspostavljanje TCP konekcije...........\n";
                            Console.WriteLine(prijavaOdgovor);
                            byte[] prijavaBajt = Encoding.UTF8.GetBytes(prijavaOdgovor);
                            serverUDP.SendTo(prijavaBajt, klijentEP);
                            break;
                        }

                    }
                    else
                    {
                        byte[] prijava = Encoding.UTF8.GetBytes("Neuspjesna prijava!Pokusajte ponovo!\n");
                        serverUDP.SendTo(prijava, klijentEP);
                    }


                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske prilikom prijema poruke: {ex}\n");
                }
            }

            try
            {
                //TCP konekcija
                Socket serverTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverTCP.Bind(new IPEndPoint(IPAddress.Any, 50019));
                serverTCP.Listen(10);

                Socket klijentSocket = serverTCP.Accept();
                Console.WriteLine("Uspostavljena je TCP konekcija sa igracem.");

                string dobrodoslica = $"Dobrodosli u trening igru kviza Kviskoteka. Danasnji takmicar je: {imeIgraca}";
                byte[] dobrodoslicaBajti = Encoding.UTF8.GetBytes(dobrodoslica);
                klijentSocket.Send(dobrodoslicaBajti);

                Console.WriteLine("Poruka dobrodoslice poslata.Igra uskoro pocinje.\n");

                byte[] start = new byte[1024];
                int brBajta = klijentSocket.Receive(start);
                string startStr = Encoding.UTF8.GetString(start, 0, brBajta);
                int brOdigranihIgara = 0;

                if (startStr.Equals("START", StringComparison.OrdinalIgnoreCase)) //igra pocinje ako je korisnik unio START
                {
                    //Pocetak igre
                    Console.WriteLine($"Zapocinjemo igre: {listaIgara} za igraca {imeIgraca}!");

                    foreach (string i in igre)
                    {
                        byte[] baferKvisko = new byte[1024];
                        int kviskoBajt = klijentSocket.Receive(baferKvisko);
                        string kvisko = Encoding.UTF8.GetString(baferKvisko, 0, kviskoBajt);

                        if (kvisko == "KVISKO")
                        {
                            igrac.UloziKvisko();
                            Console.WriteLine($"Kvisko je ulozen za igru: {i}");
                        }
                        else
                        {
                            Console.WriteLine($"Kvisko nije ulozen za igru: {i}");
                        }

                        string igraTrimovana = i.Trim().ToLower();
                        PokreniIgru(klijentSocket, igraTrimovana, igrac, brOdigranihIgara);
                        brOdigranihIgara++;
                    }

                    if (brOdigranihIgara == brojIgara) //Ako je igrac odigrao sve igre, prikazuju se svi bodovi koje je skupio
                    {
                        string kraj = $"\nKraj igre! Igrac {igrac.Nadimak} je osvojio {igrac.UkupanBrojPoena()} poena.";
                        klijentSocket.Send(Encoding.UTF8.GetBytes(kraj));
                        klijentSocket.Shutdown(SocketShutdown.Both);
                    }
                }
                else
                {
                    Console.WriteLine($"Igrac {imeIgraca} nije poslao START. Konekcija se zatvara");
                    klijentSocket.Close();
                }



            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Doslo je do greske prilikom prijema poruke. {ex}");
            }


            Console.WriteLine("Server zavrsava sa radom.");
            serverUDP.Close();

            Console.ReadKey();
        }

        private static void PokreniIgru(Socket klijentSocket, string igra, Igrac igrac, int brIgre)
        {
            int poeni = 0;

            switch (igra)
            {
                case "an":
                    poeni = AnagramiIgra(klijentSocket);
                    break;
                case "po":
                    poeni = PitanjaOdgovoriIgra(klijentSocket);
                    break;
                case "as":
                    poeni = AsocijcijeIgra(klijentSocket);
                    break;
            }

            igrac.DodajPoene(brIgre, poeni);
        }

        private static int AnagramiIgra(Socket klijentSocket)
        {
            Console.WriteLine("----------IGRA ANAGRAMI ----------\n");

            List<string> listaRijeci = new List<string> { "programiranje", "racunar", "sto", "univerzitet", "mis", "elektrotehnika" };
            Random random = new Random();
            string unesenaRijec = listaRijeci[random.Next(listaRijeci.Count)].ToLower();

            Console.WriteLine($"Izgenerisana rijec za anagram je: {unesenaRijec}");
            Anagrami anagrami = new Anagrami();
            anagrami.UcitajRijeci(unesenaRijec);

            string poruka = $"Vasa rijec za anagram je: \"{anagrami.PocetnaRijec}\".Posaljite validan anagram koristeci ista slova.";
            klijentSocket.Send(Encoding.UTF8.GetBytes(poruka));

            byte[] prijemniBafer = new byte[1024];
            int brBajta = klijentSocket.Receive(prijemniBafer);
            string predlozenAnagram = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
            Console.WriteLine($"Uneseni anagram: {predlozenAnagram}.");
            Console.WriteLine("Provjera u toku.");

            anagrami.PostaviPredlozenAnagram(predlozenAnagram);

            bool ispravanAnagram = anagrami.ProvjeriAnagram();

            string rezultat = ispravanAnagram ? "Anagram je validan!" : "Anagram nije validan!";
            klijentSocket.Send(Encoding.UTF8.GetBytes(rezultat));
            Console.WriteLine(rezultat);

            int poeni = ispravanAnagram ? anagrami.IzracunajPoene() : 0;
            klijentSocket.Send(Encoding.UTF8.GetBytes(poeni.ToString()));

            Console.WriteLine($"Igra anagrami je zavrsena. Igrac je osvojio {poeni} poena.");
            Console.WriteLine("-----------------------------------------------------------\n");

            return poeni;
        }

        private static int PitanjaOdgovoriIgra(Socket klijentSocket)
        {
            Console.WriteLine("----------IGRA PITANJA I ODGOVORI----------");
            PitanjaOdgovori pitanjaOdgovori = new PitanjaOdgovori();

            while (true)
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

            Console.WriteLine($"Igra pitanja i odgovori je zavrsena. Igrac je osvojio {ukupniPoeni} poena.");
            Console.WriteLine("-----------------------------------------------------------\n");

            return ukupniPoeni;
        }

        private static int AsocijcijeIgra(Socket klijentSocket)
        {
            Console.WriteLine("----------IGRA ASOCIJACIJE----------\n");

            Asocijacije igra = new Asocijacije();
            igra.UcitajAsocijaciju("asocijacije.txt");

            bool[] kolonaRijesena = new bool[4];
            bool konacnoRijeseno = false;
            int brGresaka = 0;
            int poeni = 0;
            int maxGresaka = 5;

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


            Console.WriteLine($"Igra asocijacije je zavrsena. Igrac je osvojio {poeni} poena.");
            Console.WriteLine("-----------------------------------------------------------\n");

            return poeni;
        }
    }
}
