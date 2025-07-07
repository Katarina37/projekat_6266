using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Klijent.Igre;

namespace Klijent
{
    public class Klijent
    {
        const string SERVER_IP = "192.168.0.4";
        static readonly Dictionary<string, string> PuniNaziviIgara = new Dictionary<string, string>
        {
            {"an", "Anagrami" },
            {"po", "Pitanja i odgovori" },
            {"as", "Asocijacije" }
        };
        static void Main(string[] args)
        {
            Socket klijentTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket klijentUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(SERVER_IP), 50009);
            EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== DOBRODOŠLI U KVISKOTEKA KVIZ  ===");
            Console.ResetColor();

            string[] igre = Array.Empty<string>();
            

            while (true)
            {
                try
                {
                    Console.Write("Unesite Vase ime/nadimak: \n");
                    string imeIgraca = Console.ReadLine().Trim();

                    Console.WriteLine("Unesite igre koje zelite da igrate, odvojene zarezima [an, po, as]:");
                    string listaIgara = Console.ReadLine().Trim();

                    igre = listaIgara.Split(',').Select(s => s.Trim()).ToArray();

                    string poruka = $"PRIJAVA: {imeIgraca}, {listaIgara}";
                    byte[] prijavaBajtovi = Encoding.UTF8.GetBytes(poruka + "\n");
                    klijentUDP.SendTo(prijavaBajtovi, serverEP);

                    byte[] prijemniBafer = new byte[1024];
                    int brBajta = klijentUDP.ReceiveFrom(prijemniBafer, ref serverEndPoint);
                    string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    Console.WriteLine($"Poruka od servera: {odgovor}");
                    if (odgovor.StartsWith("Prijava je uspjesna"))
                    {
                        
                        Console.WriteLine("Uspostavljanje konekcije");
                        Thread.Sleep(10);
                        break;
                    }

                    else
                        Console.WriteLine("Neuspjela prijava, pokusajte ponovo.\n");
                }
                catch(SocketException ex)
                {
                    Console.WriteLine($"Greska: {ex.Message}");
                }
            }
            klijentUDP.Close();

            try
            {
                klijentTCP.Connect(new IPEndPoint(IPAddress.Parse(SERVER_IP), 50019));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Povezan sa serverom.");
                Console.ResetColor();

                int brojIgraca = 0;


                byte[] bafer = new byte[1024];
                int len = klijentTCP.Receive(bafer);
                string poruka = Encoding.UTF8.GetString(bafer, 0, len);
                Console.WriteLine($"\n~~~~~~ {poruka} ~~~~~~\n");

                if (poruka.Contains("trening"))
                {
                    brojIgraca = 1;
                } else
                {
                    brojIgraca = 2;
                }
                


                if (klijentTCP.Poll(1000 * 1000, SelectMode.SelectRead))
                {
                    

                    byte[] baferStart = new byte[1024];
                    int startBr = klijentTCP.Receive(baferStart);
                    string startPoruka = Encoding.UTF8.GetString(baferStart, 0, startBr);

                    string unosStart;
                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(startPoruka);
                        Console.ResetColor();
                        unosStart = Console.ReadLine().Trim().ToUpper();
                        if (unosStart != "START")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Morate unijeti START da bi igra pocela.");
                            Console.ResetColor();
                        }
                    } while (unosStart != "START");

                    klijentTCP.Send(Encoding.UTF8.GetBytes(unosStart));
                    Console.WriteLine("START poslat.\n");


                }

                bool kviskoIskoristen = false;

                

                for (int i = 0; i < igre.Length; i++)
                {
                   string igra = igre[i];
                   bool ponoviIgru = false;

                    do
                    {
                        bool kviskoStigao = false;

                        while (!kviskoStigao)
                        {
                            if (klijentTCP.Poll(1000 * 1000, SelectMode.SelectRead))
                            {
                                byte[] baferKvisko = new byte[1024];
                                int br = klijentTCP.Receive(baferKvisko);
                                string porukaKvisko = Encoding.UTF8.GetString(baferKvisko, 0, br).Trim();



                                if (porukaKvisko.StartsWith("Da li zelite da ulozite KVISKO"))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(porukaKvisko);
                                    Console.ResetColor();

                                    string odgovor = Console.ReadLine().Trim().ToLower();

                                    if (!kviskoIskoristen && odgovor == "da")
                                    {
                                        Console.WriteLine("Kvisko je ulozen.\n");
                                        klijentTCP.Send(Encoding.UTF8.GetBytes("KVISKO"));
                                        kviskoIskoristen = true;

                                    }
                                    else if (!kviskoIskoristen && odgovor == "ne")
                                    {
                                        Console.WriteLine("Kvisko nije ulozen.\n");
                                        klijentTCP.Send(Encoding.UTF8.GetBytes("NO_KVISKO"));

                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.DarkRed;
                                        Console.WriteLine("KVISKO je vec ulozen!\n");
                                        Console.ResetColor();
                                        klijentTCP.Send(Encoding.UTF8.GetBytes("NO_KVISKO"));
                                    }

                                    kviskoStigao = true;
                                }
                                else
                                {
                                    break;
                                }
                            }


                        }

                        switch (igra)
                        {
                            case "an":
                                PokreniAN.AnagramiIgra(klijentTCP);
                                break;
                            case "po":
                                PokreniPiO.PitanjaOdgovoriIgra(klijentTCP);
                                break;
                            case "as":
                                AsocijacijeIgra(klijentTCP);
                                break;

                        }

                        if (brojIgraca == 1)
                        {

                            bool cekanjeOdgovora = true;
                            while (cekanjeOdgovora)
                            {
                                if (klijentTCP.Poll(1000 * 1000, SelectMode.SelectRead))
                                {
                                    byte[] ponovoBafer = new byte[1024];
                                    int ponovoBr = klijentTCP.Receive(ponovoBafer);
                                    string ponovoPoruka = Encoding.UTF8.GetString(ponovoBafer, 0, ponovoBr).Trim();

                                    if (ponovoPoruka.StartsWith("Da li zelite da ponovite"))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine(ponovoPoruka);
                                        Console.ResetColor();

                                        string odgovor = Console.ReadLine().Trim().ToLower();

                                        if (odgovor == "da")
                                        {
                                            Console.ForegroundColor = ConsoleColor.Cyan;
                                            Console.WriteLine("Igraće se ponovo...\n");
                                            Console.ResetColor();
                                            klijentTCP.Send(Encoding.UTF8.GetBytes("PONOVO"));
                                            ponoviIgru = true;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Cyan;
                                            Console.WriteLine("Ne igra se ponovo.");
                                            Console.ResetColor();
                                            klijentTCP.Send(Encoding.UTF8.GetBytes("NE_PONOVO"));
                                            ponoviIgru = false;
                                        }

                                        cekanjeOdgovora = false;
                                    }
                                    else if (ponovoPoruka.StartsWith("Kraj igre!"))
                                    {
                                        break;
                                    }
                                }

                                Thread.Sleep(50);
                            }
                        }
                        else
                        {
                            ponoviIgru = false;
                        }
                    } while (ponoviIgru);



                }

                if (klijentTCP.Poll(1000 * 1000, SelectMode.SelectRead))
                {
                    byte[] krajBafer = new byte[1024];
                    int krajBr = klijentTCP.Receive(krajBafer);
                    string kraj = Encoding.UTF8.GetString(krajBafer, 0, krajBr);
                    Console.WriteLine(kraj);
                }

                if(brojIgraca > 1)
                {
                    byte[] tabelaBafer = new byte[1024];
                    int tabelaBr = klijentTCP.Receive(tabelaBafer);
                    string tabela = Encoding.UTF8.GetString(tabelaBafer, 0, tabelaBr);
                    Console.WriteLine($"Pregled igre: \n {tabela}");
                }


                klijentTCP.Shutdown(SocketShutdown.Both);
                klijentTCP.Close();


            }
            catch(SocketException ex)
            {
                Console.WriteLine($"Greska: {ex.Message}");
            }


            Console.WriteLine("Klijent zavrsava sa radom.");
            Console.ReadKey();
        }

        
        


        private static void AsocijacijeIgra(Socket klijentSocket)
        {
            while (true)
            {
                byte[] prijemniBafer = new byte[2048];
                int brBajta = klijentSocket.Receive(prijemniBafer);

                string stanje = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                Console.WriteLine(stanje);

                if (stanje.Contains("Igra je zavrsena! "))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("\nUnesite polje koje zelite da otvorite (npr A1) ili rjesenje(npr. A:odgovor ili K:odgovor): ");
                    string unos = Console.ReadLine();

                    klijentSocket.Send(Encoding.UTF8.GetBytes(unos.ToUpper()));

                    brBajta = klijentSocket.Receive(prijemniBafer);
                    string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                    Console.WriteLine(odgovor);

                    if (odgovor.StartsWith("Igra je zavrsena! "))
                    {
                        break;
                    }
                }

            }
        }
    }
}
