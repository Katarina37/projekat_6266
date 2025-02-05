using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;


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
                    Console.WriteLine("\n------------------------------------------------------------------------------------------");
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

                    Console.WriteLine("------------------------------------------------------------------------------------------\n");
                    Console.WriteLine("Unesite START za pocetak igre: \n");
                    string odgovor = Console.ReadLine();
                    byte[] startPodaci = Encoding.UTF8.GetBytes(odgovor);
                    klijentSocket.Send(startPodaci);
                    Console.WriteLine("\nPoslata poruka START.\n");
                    Console.WriteLine("------------------------------------------------------------------------------------------");

                    foreach(string igra in igre.Split(','))
                    {
                        switch (igra.Trim().ToLower())
                        {
                            case "an":
                                IgraAnagrami(klijentSocket);
                                break;
                            case "po":
                                IgraPitanjaOdgovori(klijentSocket);
                                break;
                            default:
                                Console.WriteLine("Nepoznata igra.");
                                break;
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

        private static void IgraAnagrami(Socket klijentSocket)
        {
            byte[] prijemniBafer = new byte[1024];
            int brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
            string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
            Console.WriteLine($"\nServer: {poruka}");

            Console.WriteLine("Unesite anagram: ");
            string uneseniAnagram = Console.ReadLine();
            klijentSocket.Send(Encoding.UTF8.GetBytes(uneseniAnagram));

            brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
            string rezultat = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
            Console.WriteLine($"Rezultat: {rezultat}\n");

            brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
            string bodoviPoruka = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
            Console.WriteLine($"Osvojeni poeni: {bodoviPoruka}");
        }
        
        private static void IgraPitanjaOdgovori(Socket klijentSocket)
        {
            //ovdje treba da se ispravi da se nakon drugog postavljenog pitanja omoguci unos odgovora, posto trenutno ne funkcionise
            while (true)
            {
                try
                {
                    byte[] prijemniBafer = new byte[1024];
                    int brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                    string pitanje = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);

                    if (pitanje.ToLower().Contains("igra je zavrsena"))
                    {
                        Console.WriteLine("Igra pitanja i odgovori je zavrsena.");
                        break;
                    }

                    Console.WriteLine($"Pitanje: {pitanje}\n\nOdgovorite sa 'A' za tacno ili 'B' za netacno\n");

                    string korisnickiOdgovor = string.Empty;
                    while (string.IsNullOrEmpty(korisnickiOdgovor))
                    {
                        Console.WriteLine("Unesite svoj odgovor: ");
                        korisnickiOdgovor = Console.ReadLine()?.Trim().ToUpper();

                        if (korisnickiOdgovor != "A" && korisnickiOdgovor != "B")
                        {
                            Console.WriteLine("Nevazeci unos. Molimo unesite 'A' za tacno ili 'B' za netacno.");
                            korisnickiOdgovor = string.Empty;
                        }
                    }
                    klijentSocket.Send(Encoding.UTF8.GetBytes(korisnickiOdgovor));

                    brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                    string rezultat = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                    Console.WriteLine($"Rezultat: {rezultat}");

                    brojPrimljenihBajtova = klijentSocket.Receive(prijemniBafer);
                    string poeni = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                    Console.WriteLine($"Osvojeni poeni: {poeni}");
                }catch(Exception ex)
                {
                    Console.WriteLine($"Greska u komunikaciji: {ex.Message}");
                    break;
                }
            }
        }
    }
}
