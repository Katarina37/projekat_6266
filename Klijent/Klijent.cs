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
                //udp prijava
                using (Socket udpKlijent = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    string prijavaPoruka = "PRIJAVA:ImeIgraca,an,po";
                    byte[] podaci = Encoding.UTF8.GetBytes(prijavaPoruka);
                    udpKlijent.SendTo(podaci, new IPEndPoint(IPAddress.Parse(ServerIP), UdpPort));
                    Console.WriteLine("Poslata UDP prijava.");

                    //cekanje odgovora
                    byte[] prijemniiBafer = new byte[1024];
                    EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int brPrimljenihBajtova = udpKlijent.ReceiveFrom(prijemniiBafer, ref serverEndPoint);
                    string odg = Encoding.UTF8.GetString(prijemniiBafer, 0, brPrimljenihBajtova);
                    Console.WriteLine($"Server odgovorio:{odg}");
                }

                //povezujemo se na server
                Socket klijentSocet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                klijentSocet.Connect(new IPEndPoint(IPAddress.Parse(ServerIP), TcpPort));
                Console.WriteLine("Povezan sa serverom");

                //primamo poruku od servera
                byte[] prijemniBafer = new byte[1024];
                int brojPrimljenihBajtova = klijentSocet.Receive(prijemniBafer);
                string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brojPrimljenihBajtova);
                Console.WriteLine($"Server: {poruka}");

                //saljemo poruku start
                string odgovor = "START";
                byte[] startPodaci = Encoding.UTF8.GetBytes(odgovor);
                klijentSocet.Send(startPodaci);
                Console.WriteLine("Poslata poruka START.");

                klijentSocet.Shutdown(SocketShutdown.Both);
                klijentSocet.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska : {ex.Message}");
                Console.ReadLine();
            }
        }
    }
}
