namespace Server.Klase
{
    public class Igrac
    {
        public int Id { get; set; }

        public string Ime { get; set; }

        public int[] PoeniPoIgrama { get; set; }

        public bool Kvisko { get; set; }

        public Igrac()
        {
        }

        public Igrac(int id, string ime, int[] poeniPoIgrama, bool kvisko)
        {
            Id = id;
            Ime = ime;
            PoeniPoIgrama = poeniPoIgrama;
            Kvisko = kvisko;
        }

        public void UloziKvisko()
        {
            Kvisko = true;
        }

        public void ResetujKvisko()
        {
            Kvisko = false;
        }

        public void DodajPoene(int indeksIgre, int poeni)
        {
            if (Kvisko)
            {
                PoeniPoIgrama[indeksIgre] += poeni * 2;
            }
            else
            {
                PoeniPoIgrama[indeksIgre] += poeni;
            }
        }
    }
}
