namespace Server.Klase
{
    public class Igrac
    {
        private static int sljedeciID = 1;

        public int ID { get; private set; }
        public string Nadimak { get; set; }
        public int[] PoeniPoIgrama { get; set; }
        public bool Kvisko { get; set; }

        public Igrac()
        {

        }
        public Igrac(string ime, int brojIgara)
        {
            ID = sljedeciID++;
            Nadimak = ime;
            PoeniPoIgrama = new int[brojIgara];
            Kvisko = false;
        }

        public void UloziKvisko()
        {
            Kvisko = true;
        }

        public void DodajPoene(int index, int poeni)
        {
            if (Kvisko)
            {
                PoeniPoIgrama[index] += poeni * 2;
                Kvisko = false;
            }
            else
            {
                PoeniPoIgrama[index] += poeni;
            }
        }

        public int UkupanBrojPoena()
        {
            int ukupno = 0;

            foreach (var p in PoeniPoIgrama)
            {
                ukupno += p;
            }

            return ukupno;
        }
    }
}
