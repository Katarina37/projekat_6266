using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Klase
{
    public class Anagrami
    {
        public string OriginalnaRijec { get; set; }
        public string PredlozenAnagram { get; set; }

        public void UcitajRijeci(string rijec)
        {
            if (rijec.Length >= 7)
            {
                OriginalnaRijec = rijec;
            }
            else
            {
                Console.WriteLine("Rijec mora imati bar 7 karaktera.\n");
            }
        }

        public bool ProvjeriAnagram(string anagram)
        {
            //uporedjujemo da li su iste duzine anagram i org rijec
            if (anagram.Length != OriginalnaRijec.Length)
            {
                return false;
            }

            //sortiramo i uporedjujemo
            char[] originalniNiz = OriginalnaRijec.ToCharArray();
            char[] nizAnagrama = anagram.ToCharArray();
            Array.Sort(originalniNiz);
            Array.Sort(nizAnagrama);

            //uporedjujemo da li su isti
            return new string(originalniNiz) == new string(nizAnagrama);
        }

        //ako je anagram tacan, igrac dobija broj poena kolika je duzina rijeci
        public int IzracunajPoene()
        {
            return OriginalnaRijec.Length;
        }
    }
}
