﻿using System;
using System.Net.Sockets;
using System.Text;


namespace Klijent.Igre
{
    public class PokreniAs
    {
        public static void AsocijacijeIgra(Socket klijentSocket)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-------------IGRA ASOCIJACIJE -------------\n");
            Console.ResetColor();
            string stanje = string.Empty;

            while (true)
            {
                if (klijentSocket.Poll(1000 * 1000, SelectMode.SelectRead))
                {
                    byte[] prijemniBafer = new byte[4096];
                    int brBajta = klijentSocket.Receive(prijemniBafer);

                    stanje = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);

                    if (stanje.Contains("[KRAJ]"))
                    {
                        break;
                    }
                    Console.WriteLine(stanje);

                    if (stanje.Contains("[TI SI NA POTEZU]:"))
                    {
                        Console.WriteLine("\nUnesite polje koje zelite da otvorite (npr A1) ili rjesenje(npr. A:odgovor ili K:odgovor): ");
                        string unos = Console.ReadLine();

                        klijentSocket.Send(Encoding.UTF8.GetBytes(unos.ToUpper()));

                        brBajta = klijentSocket.Receive(prijemniBafer);
                        string odgovor = Encoding.UTF8.GetString(prijemniBafer, 0, brBajta);
                        Console.WriteLine(odgovor);

                        if (odgovor.StartsWith("[KRAJ]"))
                        {
                            break;
                        }
                    } 
                }

            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(stanje);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("-----------------------------------------------------------\n");
            Console.ResetColor();
        }
    }
}
