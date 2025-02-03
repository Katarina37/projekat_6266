using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            string listaIgara = dijeloviPoruke[1].Trim();

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

            PokreniTcpKomunikaciju(imeIgraca);
        }

        private static void PokreniTcpKomunikaciju(string imeIgraca)
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
                    Console.WriteLine($"Zapocinjem kviz za igraca {imeIgraca}.\n");
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
    }
}
