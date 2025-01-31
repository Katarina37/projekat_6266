using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Klijent
{
    public class Klijent
    {
        private const string ServerIP = "127.0.0.1";
        private const int TcpPort = 9001;
        static void Main(string[] args)
        {
            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine($"Greska : {e.Message}");
            }
        }
    }
}
