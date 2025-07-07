using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Klase
{
    public class SesijaIgraca
    {
        public Socket KlijentSocket { get; set; }
        public Igrac Igrac { get; set; }
        public string[] Igre { get; set; }
        public bool[] KviskoPoIgrama { get; set; }
        public bool[] KviskoPrimljen { get; set; }
        public bool Startovan { get; set; }
        public ConsoleColor Boja { get; set; }
        public bool ZavrsioIgru { get; set; }

        public SesijaIgraca(Socket socket, Igrac igrac, string[] igre, ConsoleColor boja)
        {
            KlijentSocket = socket;
            Igrac = igrac;
            Igre = igre;
            KviskoPoIgrama = new bool[igre.Length];
            KviskoPrimljen = new bool[igre.Length];
            Startovan = false;
            Boja = boja;
            ZavrsioIgru = false;
        }

        public int PoeniKvisko(bool[] kviskoPoIgrama)
        {
            int poeni = 0;
            for(int i = 0; i < Igrac.PoeniPoIgrama.Length; i++)
            {
                if (kviskoPoIgrama[i])
                {
                    poeni += Igrac.PoeniPoIgrama[i] / 2;
                }
            }

            return poeni;
        }
    }
}
