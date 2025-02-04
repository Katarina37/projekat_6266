using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Klase
{
    public class PitanjaOdgovori
    {
        public string TekucePitanje {  get; set; }
        public bool TacanOdgovor {  get; set; }
        private Dictionary<string, bool> SvaPitanja;

        private int trenutniIndex;

        public PitanjaOdgovori()
        {
            SvaPitanja = new Dictionary<string, bool>();
            trenutniIndex = 0;
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

        public bool PostaviSljedecePitanje()
        {
            if(trenutniIndex < SvaPitanja.Count)
            {
                TekucePitanje = new List<string>(SvaPitanja.Keys)[trenutniIndex];
                TacanOdgovor = new List<bool>(SvaPitanja.Values)[trenutniIndex];
                trenutniIndex++;
                return true;
            }
            return false;
        }

        public bool ProvjeriOdgovor(string korisnickiOdgovor)
        {
            bool odgovor = korisnickiOdgovor.Trim().ToUpper() == "A" ? true : false;
            return odgovor == TacanOdgovor;
        }

        public int PoeniZaOdgovor(bool tacanOdgovor)
        {
            return tacanOdgovor ? 4 : 0;
        }
    }
}
