using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Server.Klase;

namespace Server
{
    public class Server
    {
        private const int UdpPort = 9000;
        private const int TcpPort = 9001;
        static void Main(string[] args)
        {

            Socket udpUticnica = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, UdpPort);
            udpUticnica.Bind(endPoint);
            Console.WriteLine($"Udp server slusa na portu {UdpPort}\n");

            while (true)
            {
                byte[] prijemniBafer = new byte[1024];
                EndPoint klijentEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int brojPrimljenihBajtova = udpUticnica.ReceiveFrom(prijemniBafer, ref klijentEndPoint);

                string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                Console.WriteLine("--------------------------------------------------------------------------------\n");
                Console.WriteLine($"Primljena poruka za prijavu: {poruka} od {klijentEndPoint}\n");

                if (poruka.StartsWith("PRIJAVA:"))
                {
                    ObradiPrijavuIgraca(poruka, klijentEndPoint, udpUticnica);
                }

            }
        }

        private static void ObradiPrijavuIgraca(string poruka, EndPoint klijentEndPoint, Socket udpUticnica)
        {
            string[] dijeloviPoruke = poruka.Substring(8).Split(',');
            if (!poruka.StartsWith("PRIJAVA:") || dijeloviPoruke.Length < 2)
            {
                Console.WriteLine("Neispravan format poruke za prijavu.\n");
                return;
            }

            string imeIgraca = dijeloviPoruke[0].Trim();
            string listaIgara = string.Join(",", dijeloviPoruke.Skip(1));

            string[] validneIgre = { "an", "po", "as" };
            bool ispravnaListaIgara = listaIgara.Split(',').All(igra => validneIgre.Contains(igra.Trim()));

            if (!ispravnaListaIgara)
            {
                Console.WriteLine("Neispravna lista igara.\n");
                return;
            }

            Console.WriteLine($"Igrac {imeIgraca} zeli da igra igre: {listaIgara}\n");

            string tcpInformacije = $"TCP: 192.168.1.105:{TcpPort}";
            byte[] odgovorPodaci = Encoding.UTF8.GetBytes(tcpInformacije);
            udpUticnica.SendTo(odgovorPodaci, klijentEndPoint);

            PokreniTcpKomunikaciju(imeIgraca, listaIgara);
        }

        private static void PokreniTcpKomunikaciju(string imeIgraca, string listaIgara)
        {
            Socket tcpUticnica = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpUticnica.Bind(new IPEndPoint(IPAddress.Any, TcpPort));
            tcpUticnica.Listen(10);

            Console.WriteLine($"Tcp server slusa na portu {TcpPort}\n");

            while (true)
            {
                Socket klijentSocket = tcpUticnica.Accept();
                Console.WriteLine($"Uspostavljena TCP konekcija sa igracem.\n");

                string porukaDobrodoslice = $"Dobrodosli u trening igru kviza Kviskoteke, danasnji tackmicar je {imeIgraca}.";
                byte[] dobrodosliPodaci = Encoding.UTF8.GetBytes(porukaDobrodoslice);
                klijentSocket.Send(dobrodosliPodaci);

                Console.WriteLine("Poruka dobrodoslice poslata.\n");

                byte[] prijemniBafer = new byte[1024];
                int brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova).Trim();
                
                if(odgovor.Equals("START", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Zapocinjemo igre za igraca {imeIgraca} : {listaIgara}.\n");
                    string[] igre = listaIgara.Split(',');
                    
                    foreach(string igra in igre)
                    {
                        string trimovanaIgra = igra.Trim().ToLower();
                        //pomocna metoda za pokretanje igre
                        PokreniIgru(klijentSocket, trimovanaIgra);
                    }
                }
                else
                {
                    Console.WriteLine($"Igrac {imeIgraca} nije poslao START. \nKonekcija se zatvara.\n");
                    klijentSocket.Close();
                }

                break;
            }
            tcpUticnica.Close();
        }

        //pomocna metoda za pokretanje igre
        private static void PokreniIgru(Socket klijentSocket, string igra)
        {
            switch (igra.ToLower())
            {
                case "an":
                    PokreniAnagramIgru(klijentSocket);
                    break;
                case "po":
                    //poziv implementacije za drugu igru
                    klijentSocket.Send(Encoding.UTF8.GetBytes("Zapocinjemo drugu igru."));
                    break;
                case "as":
                    //poziv implementacije za drugu igru
                    klijentSocket.Send(Encoding.UTF8.GetBytes("Zapocinjemo trecu igru."));
                    break;
                default:
                    klijentSocket.Send(Encoding.UTF8.GetBytes("Nepoznata igra."));
                    break;
            }
        }

        private static void PokreniAnagramIgru(Socket klijentSocket)
        {
            Console.WriteLine("---------------------------------IGRA ANAGRAMI----------------------------------\n");
            Console.WriteLine("Unesite rijec za anagram: \n");
            string unesenaRijec = Console.ReadLine().Trim().ToLower();

            Anagrami anagrami = new Anagrami();
            anagrami.UcitajRijeci(unesenaRijec);

            string poruka = $"Vasa rijec za anagram je: \"{anagrami.OriginalnaRijec}\". Posaljite validan anagram koristeci ista slova.";
            byte[] podaci = Encoding.UTF8.GetBytes(poruka);
            klijentSocket.Send(podaci);

            byte[] prijemniBafer = new byte[1024];
            int brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
            string predlozenAnagram = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);

            bool ispravanAnagram = ValidirajAnagram(unesenaRijec, predlozenAnagram);

            string rezultat = ispravanAnagram ? "Anagram je validan!" :
                                                "Anagram nije validan!";
            byte[] rezultatPodaci = Encoding.UTF8.GetBytes(rezultat);
            klijentSocket.Send(rezultatPodaci);
            klijentSocket.Close();
        }

        //pomocna metoda za PokreniAnagramIgru
        private static bool ValidirajAnagram(string originalnaRijec, string predlozenAnagram)
        {
            char[] originalniKarakteri = originalnaRijec.Replace(" ", "").ToCharArray();
            char[] anagramKarakteri = predlozenAnagram.Replace(" ", "").ToCharArray();

            Array.Sort(originalniKarakteri);
            Array.Sort(anagramKarakteri);
            
            return originalniKarakteri.SequenceEqual(anagramKarakteri);
        }
    }
}
