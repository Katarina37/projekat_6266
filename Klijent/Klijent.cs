using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Klijent
{
    public class Klijent
    {
        private const string ServerIP = "192.168.1.105";
        private const int UdpPort = 9000;
        private const int TcpPort = 9001;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Unesite ime/nadimak igraca: \n");
                string imeIgraca = Console.ReadLine();

                Console.WriteLine("\nUnesite igre koje zelite da igrate (an, po, as) odvojene zarezom: \n");
                string igre = Console.ReadLine();

                string prijavaPoruka = $"PRIJAVA:{imeIgraca},{igre}";

                //udp prijava
                using (Socket udpKlijent = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    byte[] podaci = Encoding.UTF8.GetBytes(prijavaPoruka);
                    udpKlijent.SendTo(podaci, new IPEndPoint(IPAddress.Parse(ServerIP), UdpPort));
                    Console.WriteLine("\nPoslata UDP prijava.\n");

                    byte[] prijemniBafer = new byte[1024];
                    EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int brPrimljenihBajtova = udpKlijent.ReceiveFrom(prijemniBafer, ref serverEndPoint);
                    string odg = Encoding.UTF8.GetString(prijemniBafer, 0, brPrimljenihBajtova);
                    Console.WriteLine($"Server odgovorio: {odg}\n");
                }

                using (Socket klijentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    klijentSocket.Connect(new IPEndPoint(IPAddress.Parse(ServerIP), TcpPort));
                    Console.WriteLine("Povezan sa serverom.\n");

                    byte[] prijemniBafer = new byte[1024];
                    int brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                    Console.WriteLine($"Server : {poruka}\n");

                    Console.WriteLine("Unesite START za pocetak igre: \n");
                    string odgovor = Console.ReadLine();
                    byte[] startPodaci = Encoding.UTF8.GetBytes(odgovor);
                    klijentSocket.Send(startPodaci);
                    Console.WriteLine("\nPoslata poruka START.\n");

                    //igra anagrami
                    while (true)
                    {
                        brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                        poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                        Console.WriteLine($"\nServer: {poruka}\n");

                        Console.WriteLine("Unesite anagram: \n");
                        string uneseniAnagram = Console.ReadLine();
                        byte[] anagramPodaci = Encoding.UTF8.GetBytes(uneseniAnagram);
                        klijentSocket.Send(anagramPodaci);
                        Console.WriteLine("\nPoslata poruka sa anagramom.\n");

                        brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                        string rezultat = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                        Console.WriteLine($"Rezultat: {rezultat}\n");

                        if(rezultat == "Anagram je validan!")
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Pokusajte ponovo.");
                        }
                    }

                    klijentSocket.Shutdown(SocketShutdown.Both);
                    klijentSocket.Close();
                }   
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska : {ex.Message}");
                Console.ReadLine();
            }
            Console.ReadLine();
        }
    }
}
