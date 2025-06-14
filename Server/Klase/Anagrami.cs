using System;


namespace Server.Klase
{
    public class Anagrami
    {
        public string PocetnaRijec { get; set; }
        public string PredlozenaRijec { get; set; }

        public void UcitajRijeci(string rijec)
        {
            if (rijec.Length < 7 || string.IsNullOrWhiteSpace(rijec))
            {
                Console.WriteLine("Rijec mora imati najmanje 7 karaktera. Pokusajte ponovo!\n");
            }
            else
            {
                PocetnaRijec = rijec.ToLower().Trim();
            }
        }

        public void PostaviPredlozenAnagram(string anagram)
        {
            PredlozenaRijec = anagram.ToLower().Trim();
        }

        public bool ProvjeriAnagram()
        {
            if (string.IsNullOrWhiteSpace(PocetnaRijec) || string.IsNullOrWhiteSpace(PredlozenaRijec))
                return false;

            if (PocetnaRijec.Length != PredlozenaRijec.Length)
                return false;

            char[] originalniNiz = PocetnaRijec.ToCharArray();
            char[] anagramNiz = PredlozenaRijec.ToCharArray();

            Array.Sort(originalniNiz);
            Array.Sort(anagramNiz);

            return new string(originalniNiz) == new string(anagramNiz);
        }

        public int IzracunajPoene()
        {
            if (ProvjeriAnagram())
            {
                return PocetnaRijec.Length;
            }
            else
            {
                return 0;
            }
        }
    }
}
