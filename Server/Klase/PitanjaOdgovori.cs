using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Klase
{
    public class PitanjaOdgovori
    {
        private string TekucePitanje;

        private bool TacanOdgovor;

        private Dictionary<string, bool> SvaPitanja;

        private int trenutniIndex = 0;

        private int Poeni;

        public PitanjaOdgovori()
        {
            SvaPitanja = new Dictionary<string, bool>();
            Poeni = 0;
            UcitajPitanja();
        }

        private void UcitajPitanja()
        {
            SvaPitanja.Add("Bajt ima 8 bita:", true);
            SvaPitanja.Add("Osnovna jedinica za skladistenje podataka u racunarima je bajt:", false);
            SvaPitanja.Add("Git je programski jezik:", false);
            SvaPitanja.Add("Broj sistema koji koristi binarni sistem je osam", false);
            SvaPitanja.Add("Najveca vrijednost koju moze imati unsigned byte je 255:", true);
            SvaPitanja.Add("C# je programski jezik koji je razvio Microsoft:", true);
            SvaPitanja.Add("Metoda main u C# uvijek mora imati povratni tip void", false);
            SvaPitanja.Add("SQL je jezik za rad sa bazama podataka", true);
            SvaPitanja.Add("U objektno orijentisanom programiranju, polimorfizam omogucava razlicite implementacije iste metode", true);
            SvaPitanja.Add("Bool je tip podatka koji moze imati 3 razlicite vrijednosti", false);
        }

        public string PostaviSljedecePitanje()
        {
            if(trenutniIndex < SvaPitanja.Count)
            {
                TekucePitanje = new List<string>(SvaPitanja.Keys)[trenutniIndex];
                TacanOdgovor = SvaPitanja[TekucePitanje];
                trenutniIndex++;
                return TekucePitanje;
            }
            return "Igra je zavrsena";
        }

        public string ProvjeriOdgovor(char odgovor)
        {
            bool odgovorBool = (odgovor == 'A' || odgovor == 'a');
            if(odgovorBool == TacanOdgovor)
            {
                Poeni += 4;
                return "Tacan odgovor! Osvojili ste 4 poena.";
            }
            return "Netacan odgovor";
        }

        public int DajPoene()
        {
            return Poeni;
        }
    }
}
