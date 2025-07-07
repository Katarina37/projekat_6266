using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klijent.Igre
{
    public class PokreniPiO
    {
        public static void PitanjaOdgovoriIgra(Socket klijentSocket)
        {
            Console.WriteLine("----------IGRA PITANJA I ODGOVORI ----------\n");
            byte[] prijemniBafer = new byte[2048];

            while (true)
            {
                if (klijentSocket.Poll(20 * 1000 * 1000, SelectMode.SelectRead))
                {
                    int brBajta = klijentSocket.Receive(prijemniBafer);
                    string poruka = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta).Trim();

                    if (poruka.StartsWith("[KRAJ] :"))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(poruka);
                        Console.ResetColor();
                        break;
                    }

                    Console.WriteLine("\n---------------------------");
                    Console.WriteLine(poruka);

                    string odgovor;
                    do
                    {
                        odgovor = Console.ReadLine().Trim().ToUpper();
                        if (odgovor != "A" && odgovor != "B")
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Morate unijeti 'A' ili 'B'!\n");
                            Console.ResetColor();
                        }
                    } while (odgovor != "A" && odgovor != "B");

                    klijentSocket.Send(Encoding.UTF8.GetBytes(odgovor));

                    if (klijentSocket.Poll(10 * 1000 * 1000, SelectMode.SelectRead))
                    {
                        byte[] rezBafer = new byte[1024];
                        int brRez = klijentSocket.Receive(rezBafer);
                        string rezultat = Encoding.UTF8.GetString(rezBafer, 0, brRez);

                        if(rezultat.Contains("Tacan odgovor!"))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n" + rezultat);
                            Console.ResetColor();
                        } else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n" + rezultat);
                            Console.ResetColor();
                        }

                        
                    }
                }
                else
                {
                    Console.WriteLine("Pitanje nije stiglo.");
                    break;
                }

                Thread.Sleep(100);
            }

            Console.WriteLine("-----------------------------------------------------------\n");
        }
    }
}
