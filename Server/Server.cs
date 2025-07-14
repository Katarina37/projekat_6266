using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Klase;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Pomocne_metode;
using Server.IgreStart;


namespace Server
{
    public class Server
    {
        static readonly Dictionary<EndPoint, (string, string[])> prijavljeniIgraci = new Dictionary<EndPoint, (string, string[])>();
        static readonly Dictionary<string, string> PuniNaziviIgara = new Dictionary<string, string>
        {
            {"an", "Anagrami" },
            {"po", "Pitanja i odgovori" },
            {"as", "Asocijacije" }
        };
        static void Main(string[] args)
        {
            Socket serverUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50009);
            serverUDP.Bind(serverEP);
            serverUDP.Blocking = false;

            Socket serverTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverTCP.Bind(new IPEndPoint(IPAddress.Any, 50019));
            serverTCP.Listen(10);
            serverTCP.Blocking = false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=========================================");
            Console.WriteLine("      DOBRODOŠLI U KVISKOTEKA KVIZ       ");
            Console.WriteLine("=========================================");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("Server je pokrenut i čeka prijavu igrača...");
            Console.WriteLine();

            EndPoint klijentEP = new IPEndPoint(IPAddress.Any, 0);

            DateTime? prvaPrijava = null;
            TimeSpan cekanjeDrugePrijave = TimeSpan.FromSeconds(10);

            #region UDP PRIJAVA
            while (true)
            {

               if(serverUDP.Poll(1000*1000, SelectMode.SelectRead))
                {
                    byte[] prijemniBafer = new byte[1024];

                    try
                    {
                        if(serverUDP.Available > 0)
                        {
                            int brBajta = serverUDP.ReceiveFrom(prijemniBafer, ref klijentEP);
                            string primljenaPoruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                            Console.WriteLine("Prijava je stigla. Provjera u toku...\n");
                            
                            if (primljenaPoruka.StartsWith("PRIJAVA: "))
                            {
                                string[] dijelovi = primljenaPoruka.Substring(9).Split(new char[] { ',' }, 2);


                                if (dijelovi.Length != 2)
                                {
                                    Console.WriteLine("Neispravan format poruke za prijavu!");
                                }

                                string imeIgraca = dijelovi[0].Trim();
                                string listaIgara = dijelovi[1].Trim();

                                string[] igre = listaIgara.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
                                string[] validneIgre = { "an", "po", "as" };
                                bool ispravnaListaIgara = igre.All(igra => validneIgre.Contains(igra.Trim()));

                                if (igre.All(i => validneIgre.Contains(i)))
                                {
                                    prijavljeniIgraci.Add(klijentEP, (imeIgraca, igre));

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"\nIgrač '{imeIgraca}' je uspešno prijavljen!\n");
                                    Console.ResetColor();

                                    Console.WriteLine("Čeka se početak igre...");
                                    Console.WriteLine("-----------------------------------------");
                                    string prijavaOdgovor = "Prijava je uspjesna.Uspostavljanje TCP konekcije...........\n";
                                    byte[] prijavaBajt = Encoding.UTF8.GetBytes(prijavaOdgovor);
                                    serverUDP.SendTo(prijavaBajt, klijentEP);
                                    Thread.Sleep(20);

                                    if (prvaPrijava == null)
                                    {
                                        prvaPrijava = DateTime.Now;
                                    }


                                }
                                else
                                {
                                    byte[] odgovorIgre = Encoding.UTF8.GetBytes("Neispravna lista igara.Pokusajte ponovo!\n");
                                    serverUDP.SendTo(odgovorIgre, klijentEP);
                                }

                            }
                            else
                            {
                                byte[] prijava = Encoding.UTF8.GetBytes("Neuspjesna prijava!Pokusajte ponovo!\n");
                                serverUDP.SendTo(prijava, klijentEP);
                            }
                        }


                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Doslo je do greske prilikom prijema poruke: {ex}\n");
                    }
                }

                if (prijavljeniIgraci.Count == 2)
                {
                    Console.WriteLine("Oba igraca su se prijavila.Pocinje takmicenje.");
                    break;
                }

                if (prijavljeniIgraci.Count == 1 && prvaPrijava != null && DateTime.Now - prvaPrijava > cekanjeDrugePrijave)
                {
                    Console.WriteLine("Prijavio se samo jedan igrac. Pocinje trening.");

                    break;
                }
            }
            #endregion 


            try
            {                
                Console.WriteLine("Prijava je prihvacena. Igra uskoro pocinje.");

                bool trening = prijavljeniIgraci.Count == 1;
                int ocekivani = trening ? 1 : 2;
                List<SesijaIgraca> sviIgraci = new List<SesijaIgraca>();
                ConsoleColor[] boje = { ConsoleColor.Blue, ConsoleColor.Red };

                #region Prihvatanje Igraca
                while (sviIgraci.Count < ocekivani)
                {
                    if(serverTCP.Poll(1000*1000, SelectMode.SelectRead))
                    {
                        Socket klijent = serverTCP.Accept();

                        var (ime, igre) = prijavljeniIgraci.Values.ElementAt(sviIgraci.Count);
                        Igrac igrac = new Igrac(ime, igre.Length);
                        var sesija = new SesijaIgraca(klijent, igrac, igre, boje[sviIgraci.Count % boje.Length]);
                        sviIgraci.Add(sesija);

                        Console.ForegroundColor = sesija.Boja;
                        Console.WriteLine($">>Igrac {ime} je povezan.\n");
                        Console.ResetColor();

                        string dobrodoslica = sviIgraci.Count == 1 && prijavljeniIgraci.Count == 1
                            ? $"Dobrodosli u trening igru Kviskoteka! Danasnji takmicar je {ime}."
                            : "Dobrodosli u Kviskoteka takmicenje! Vas protivnik ce uskoro biti spreman.";
                        klijent.Send(Encoding.UTF8.GetBytes(dobrodoslica));
                        
                        
                    }
                }
                Console.WriteLine(">>Poruka dobrodoslice je poslata igracima.");

                Thread.Sleep(50);

                #endregion


                #region START
                Console.WriteLine("Ceka se START kako bi se igra pokrenula.\n");
                foreach (var igrac in sviIgraci)
                {
                    igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("Unesite START za pocetak igre: \n"));
                }

                int startPrimljen = 0;
                while (startPrimljen < sviIgraci.Count)
                {
                    foreach (var igrac in sviIgraci)
                    {
                        if (!igrac.Startovan && igrac.KlijentSocket.Poll(1000 * 1000, SelectMode.SelectRead))
                        {
                            byte[] startBf = new byte[1024];
                            int br = igrac.KlijentSocket.Receive(startBf);
                            string poruka = Encoding.UTF8.GetString(startBf, 0, br).Trim().ToUpper();

                            if (poruka == "START")
                            {
                                igrac.Startovan = true;
                                Console.ForegroundColor = igrac.Boja;
                                Console.WriteLine($">>Igrac {igrac.Igrac.Nadimak} je poslao START.\n");
                                Console.ResetColor();
                                startPrimljen++;
                            }
                        }
                    }

                    Thread.Sleep(50);
                }
                #endregion


                int brojIgara = sviIgraci[0].Igre.Length;
                int brojOdigranihIgara = 0;
                

                for ( int i = 0; i < brojIgara; i++)
                {
                    Console.WriteLine(">>KVISKO<<");
                    string igra = sviIgraci[0].Igre[i];
                    bool ponoviIgru = false;

                    do
                    {
                        foreach (var igrac in sviIgraci)
                        {
                            igrac.KviskoPrimljen[i] = igrac.Igrac.KviskoIskoristen;
                        }
                        int kviskoPrimljen = sviIgraci.Count(igrac => igrac.KviskoPrimljen[i]);

                        
                        foreach (var igrac in sviIgraci)
                        {
                            if (!igrac.KviskoPrimljen[i])
                            {
                                string kviskoPitanje = $"Da li zelite da ulozite KVISKO za igru {PuniNaziviIgara[igra]}?";
                                igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(kviskoPitanje + "\n"));
                            } else {
                                igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes("KVISKO je vec iskoristen.\n"));
                            }
                        }

                        while (kviskoPrimljen < sviIgraci.Count)
                        {
                            foreach (var igrac in sviIgraci)
                            {
                                if (!igrac.KviskoPrimljen[i] && igrac.KlijentSocket.Poll(1000 * 1000, SelectMode.SelectRead))
                                {
                                    byte[] kviskoBf = new byte[1024];
                                    int br = igrac.KlijentSocket.Receive(kviskoBf);
                                    string kvisko = Encoding.UTF8.GetString(kviskoBf, 0, br).Trim().ToUpper();

                                    if (kvisko == "KVISKO")
                                    {
                                        igrac.Igrac.UloziKvisko();
                                        Console.WriteLine($">>{igrac.Igrac.Nadimak} ulaze KVISKO za igru {PuniNaziviIgara[igra]}.\n");
                                        igrac.KviskoPoIgrama[i] = true;
                                        
                                    }
                                    else
                                    {
                                        Console.WriteLine($">>{igrac.Igrac.Nadimak} ne ulaze KVISKO za igru {PuniNaziviIgara[igra]}.\n");
                                        igrac.KviskoPoIgrama[i] = false;
                                        
                                    }

                                    igrac.KviskoPrimljen[i] = true;
                                    kviskoPrimljen++;
                                }
                            }

                            Thread.Sleep(50);
                        }

                        Console.WriteLine("-----------------------------------------------------------\n");
                        


                        foreach (var igrac in sviIgraci)
                        {
                            Console.ForegroundColor = igrac.Boja;
                            Console.WriteLine($">> {igrac.Igrac.Nadimak} igra igru {PuniNaziviIgara[igra]}\n\n");
                            Console.ResetColor();
                        }

                        PokreniIgru(igra, i, sviIgraci);

                        
                        foreach (var igrac in sviIgraci)
                        {
                            igrac.ZavrsioIgru = false;
                        }


                        if (trening)
                        {
                            SesijaIgraca igrac = sviIgraci[0];
                            igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes($"Da li zelite da ponovite igru {PuniNaziviIgara[igra]}? (da/ne)\n"));
                            

                            if (igrac.KlijentSocket.Poll(10 * 1000 * 1000, SelectMode.SelectRead))
                            {
                                byte[] ponovoBafer = new byte[1024];
                                int br = igrac.KlijentSocket.Receive(ponovoBafer);
                                string odgovor = Encoding.UTF8.GetString(ponovoBafer, 0, br).Trim().ToUpper();

                                ponoviIgru = false;
                                if (odgovor == "PONOVO")
                                {
                                    Console.WriteLine($"Igrac {igrac.Igrac.Nadimak} zeli da ponovi igru {PuniNaziviIgara[igra]}");
                                    ponoviIgru = true; 
                                }
                                else
                                {
                                    ponoviIgru = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine(">> Nije stigao odgovor za ponavljanje — Igra se ne ponavlja.");
                                
                                ponoviIgru = false;
                            }
                        } else
                        {
                            ponoviIgru = false;
                        }

                        if (!ponoviIgru)
                        {
                            brojOdigranihIgara++;
                        }

                    } while (ponoviIgru);

                    

                    if (brojOdigranihIgara == brojIgara * sviIgraci.Count)
                    {
                        Console.WriteLine(">> SVE IGRE SU ZAVRSENE <<\n Slijede rezultati");
                        break;
                    }

                    Thread.Sleep(50);

                }

                
                if (trening)
                {
                    Console.WriteLine($"Kraj trening igre! Igrac {sviIgraci[0].Igrac.Nadimak} je osvojio {sviIgraci[0].Igrac.UkupanBrojPoena()} poena.\n");
                    string kraj = $"Kraj trening igre! Osvojili ste ukupno {sviIgraci[0].Igrac.UkupanBrojPoena()} poena. Cestitamo! \n ";
                    sviIgraci[0].KlijentSocket.Send(Encoding.UTF8.GetBytes(kraj));
                    sviIgraci[0].KlijentSocket.Shutdown(SocketShutdown.Both);
                    sviIgraci[0].KlijentSocket.Close();
                }
                else
                {
                    string porukaPobjednik = Pobjednik.ProglasiPobjednika(sviIgraci);
                    string tabela = Tabela.IspisiKonacnuTabelu(sviIgraci, sviIgraci[0].Igre);

                    Console.WriteLine(porukaPobjednik);
                    Console.WriteLine($"Pregled igre: \n {tabela}");

                    foreach (var igrac in sviIgraci)
                    {
                        igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(porukaPobjednik));
                        Thread.Sleep(100); 
                        igrac.KlijentSocket.Send(Encoding.UTF8.GetBytes(tabela));
                        igrac.KlijentSocket.Shutdown(SocketShutdown.Both);
                        igrac.KlijentSocket.Close();
                    }

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

        #region Pokretanje Igara
        private static void PokreniIgru(string igra,int brIgre, List<SesijaIgraca> igraci)
        {
            switch (igra)
            {
                case "an":
                    PokreniAnagrami.AnagramiIgra(igraci, brIgre);
                    break;
                case "po":
                     PokreniPIO.PitanjaOdgovoriIgra(igraci, brIgre);
                    break;
                case "as":
                    PokreniAS.AsocijcijeIgra(igraci, brIgre);
                    break;
            }

        }

        #endregion 

    }
}
