using System;
using System.Net.Sockets;
using System.Text;

namespace Klijent.Igre
{
    public class PokreniAN
    {
        public static void AnagramiIgra(Socket klijentSocket)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-------------IGRA ANAGRAMI -------------\n");
            Console.ResetColor();
            byte[] baferZadatak = new byte[1024];
            int brZadatka = klijentSocket.Receive(baferZadatak);
            string porukaZadatka = Encoding.UTF8.GetString(baferZadatak, 0, brZadatka);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" {porukaZadatka}");
            Console.ResetColor();

            Console.Write("Unesite anagram: ");
            string uneseniAnagram = Console.ReadLine();
            klijentSocket.Send(Encoding.UTF8.GetBytes(uneseniAnagram));

            byte[] baferValidnost = new byte[1024];
            int brValidnost = klijentSocket.Receive(baferValidnost);
            string validnost = Encoding.UTF8.GetString(baferValidnost, 0, brValidnost);

            if(validnost.Equals("Anagram je validan.\n"))
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"\n{validnost}");
                Console.ResetColor();
            } else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\nRezultat: {validnost}");
                Console.ResetColor();
            }
            

            byte[] baferPoeni = new byte[1024];
            int brPoeni = klijentSocket.Receive(baferPoeni);
            string poeni = Encoding.UTF8.GetString(baferPoeni, 0, brPoeni);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[KRAJ] : "+ poeni);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-----------------------------------------------------------\n");
            Console.ResetColor();
        }
    }
}
