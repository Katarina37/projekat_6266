using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Klijent
{
    public class Klijent
    {
        static void Main(string[] args)
        {
            Socket klijentUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint klijentEP = new IPEndPoint(IPAddress.Parse("192.168.0.37"), 50009);
            EndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);

            string[] igre = Array.Empty<string>();

            while (true)
            {
                try
                {
                    string imeIgraca = "";

                    do
                    {
                        Console.Write("Unesite Vase ime/nadimak:");
                        imeIgraca = Console.ReadLine();

                        if (string.IsNullOrEmpty(imeIgraca))
                        {
                            Console.WriteLine("Neuspjesno! Morate unijeti ime.\n");
                        }
                    } while (string.IsNullOrEmpty(imeIgraca));
                    //prijava igraca
                    Console.WriteLine("Unesite igre koje zelite da igrate, odvojene zarezima [an, po, as]: ");
                    string listaIgara = Console.ReadLine()?.Trim();
                    igre = listaIgara.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
                    int brojIgara = igre.Length;

                    string poruka = $"PRIJAVA: {imeIgraca}, {listaIgara}";
                    byte[] binarnaPrijava = Encoding.UTF8.GetBytes(poruka);

                    klijentUDP.SendTo(binarnaPrijava, klijentEP);
                    Console.WriteLine("Uspjesno poslata prijava!");
                    Console.WriteLine("\n----------OBRADA PRIJAVE----------\n");

                    byte[] prijemniBafer = new byte[1024];
                    int brBajta = klijentUDP.ReceiveFrom(prijemniBafer, ref serverEP);
                    string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                    Console.WriteLine($"Server je odgovorio: {odgovor}");
                    if (odgovor.StartsWith("Prijava je uspjesna."))
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }



                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Doslo je do greske prilikom slanja poruke. {ex}\n");
                }
            }

            try
            {
                Socket klijentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                klijentSocket.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.37"), 50019));
                Console.WriteLine("Povezan sa serverom.\n");

                byte[] bafer = new byte[1024];
                int brBajta = klijentSocket.Receive(bafer);
                string poruka = Encoding.UTF8.GetString(bafer, 0, brBajta);

                Console.WriteLine($"Server: {poruka}");
                Console.WriteLine("-------------------------");
                //start za pocetak igre
                string start;
                do
                {
                    Console.WriteLine("Unesite START za pocetak igre: ");
                    start = Console.ReadLine()?.Trim().ToUpper();

                    if (start != "START")
                    {
                        Console.WriteLine("Morate unijeti START da bi igra pocela!\n");
                    }
                } while (start != "START");

                byte[] startBajt = Encoding.UTF8.GetBytes(start);
                klijentSocket.Send(startBajt);
                Console.WriteLine("Poruka START je poslata. Igra uskoro pocinje.");
                Console.WriteLine("-----------------------------------------");

                bool kviskoIskoristen = false;

                for (int i = 0; i < igre.Length; i++)
                {
                    string igra = igre[i];
                    bool ponovi;

                    do
                    {
                        if (!kviskoIskoristen)
                        {
                            Console.WriteLine($"Da li zelite da ulozite KVISKO za igru {igra}? da/ne");
                            string kvisko = Console.ReadLine()?.Trim().ToLower();

                            if (kvisko == "da")
                            {
                                klijentSocket.Send(Encoding.UTF8.GetBytes("KVISKO"));
                                Console.WriteLine($"Kvisko je ulozen za igru {igra}.");
                                kviskoIskoristen = true;
                            }
                            else
                            {
                                klijentSocket.Send(Encoding.UTF8.GetBytes("NO_KVISKO"));
                                Console.WriteLine("Kvisko nije ulozen za ovu igru.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Kvisko je vec iskoristen. Igra {igra} se nastavlja bez ulozenog kviska.\n");
                            klijentSocket.Send(Encoding.UTF8.GetBytes("NO_KVISKO"));
                        }

                        switch (igra.ToLower())
                        {
                            case "an":
                                AnagramiIgra(klijentSocket);
                                break;
                            case "po":
                                PitanjaOdgovoriIgra(klijentSocket);
                                break;
                            case "as":
                                AsocijacijeIgra(klijentSocket);
                                break;
                            default:
                                Console.WriteLine("Nepoznata igra.");
                                break;
                        }

                        //ponavljanje igre
                        Console.WriteLine($"Da li zelite da ponovite igru {igra}? da/ne");
                        string unos = Console.ReadLine()?.Trim().ToLower();
                        if (unos == "da")
                        {
                            klijentSocket.Send(Encoding.UTF8.GetBytes("AGAIN"));
                            Console.WriteLine($"Igrac ponovo igra igru {igra}");
                            ponovi = true;
                            continue;
                        }
                        else
                        {
                            klijentSocket.Send(Encoding.UTF8.GetBytes("NOT_AGAIN"));
                            ponovi = false;
                        }
                    } while (ponovi);
                }

                byte[] kraj = new byte[1024];
                int krajBr = klijentSocket.Receive(kraj);
                string krajOdg = Encoding.UTF8.GetString(kraj, 0, krajBr);
                Console.WriteLine(krajOdg);
                klijentSocket.Shutdown(SocketShutdown.Both);


            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Doslo je do greske prilikom slanja poruke. {ex}");
            }




            Console.WriteLine("Klijent zavrsava sa radom.");
            klijentUDP.Close();
            Console.ReadKey();
        }

        private static void AnagramiIgra(Socket klijentSocket)
        {
            byte[] prijemniBafer = new byte[1024];
            int brBajta = klijentSocket.Receive(prijemniBafer);
            string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
            Console.WriteLine($"Server: {poruka}");

            Console.WriteLine("Unesite anagram: ");
            string uneseniAnagram = Console.ReadLine();
            klijentSocket.Send(Encoding.UTF8.GetBytes(uneseniAnagram));

            brBajta = klijentSocket.Receive(prijemniBafer);
            string rezultat = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
            Console.WriteLine($"Rezultat: {rezultat}");

            brBajta = klijentSocket.Receive(prijemniBafer);
            string bodovi = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
            Console.WriteLine($"Osvojeni poeni: {bodovi}\n");

        }

        private static void PitanjaOdgovoriIgra(Socket klijentSocket)
        {
            byte[] prijemniBafer = new byte[1024];

            while (true)
            {
                int brBajta = klijentSocket.Receive(prijemniBafer);
                string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                if (poruka.StartsWith("KRAJ"))
                {
                    string preostalo = poruka.Substring(4);
                    if (!string.IsNullOrWhiteSpace(preostalo))
                    {
                        Console.WriteLine($"\n{preostalo.Trim()}");
                    }
                    else
                    {
                        int brBajtaPoeni = klijentSocket.Receive(prijemniBafer);
                        string bodovi = Encoding.UTF8.GetString(prijemniBafer, 0, brBajtaPoeni);
                        Console.WriteLine($"Ukupni poeni: {bodovi}\n");
                    }

                    break;
                }

                Console.WriteLine("-----PITANJE-----\n");
                Console.WriteLine(poruka);

                string odgovor = Console.ReadLine();
                klijentSocket.Send(Encoding.UTF8.GetBytes(odgovor));

                brBajta = klijentSocket.Receive(prijemniBafer);
                string rezultat = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                Console.WriteLine($"\nRezultat: {rezultat}");

            }


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
