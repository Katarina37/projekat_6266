using System.Collections.Generic;
using Server.Klase;

namespace Server.Pomocne_metode
{
    public class Pobjednik
    {
        public static string ProglasiPobjednika(List<SesijaIgraca> sviIgraci)
        {
            SesijaIgraca pobjednik = null;
            string pobjednikPoruka = string.Empty;

            foreach (var igrac in sviIgraci)
            {
                var igracTren = igrac;

                if (pobjednik == null)
                {
                    pobjednik = igrac;
                    continue;
                }

                int poeniIgracTren = igracTren.Igrac.UkupanBrojPoena();
                int poeniPobjednik = pobjednik.Igrac.UkupanBrojPoena();

                if (poeniIgracTren > poeniPobjednik)
                {
                    pobjednik = igrac;
                }
                else if (poeniIgracTren == poeniPobjednik)
                {
                    int kviskoIgracTren = igracTren.PoeniKvisko(igrac.KviskoPoIgrama);
                    int kviskoPobjednik = pobjednik.PoeniKvisko(pobjednik.KviskoPoIgrama);

                    if (kviskoIgracTren > kviskoPobjednik)
                        pobjednik = igrac;
                }
            }

            foreach (var igrac in sviIgraci)
            {
                if (igrac == pobjednik)
                {
                    pobjednikPoruka = $"Kraj igre! Pobjednik je >>{igrac.Igrac.Nadimak}<< sa ukupno {igrac.Igrac.UkupanBrojPoena()} poena!";
                }
                else
                {
                    pobjednikPoruka = $"Kraj igre! Pobjednik je >>{pobjednik.Igrac.Nadimak}<< sa ukupno {pobjednik.Igrac.UkupanBrojPoena()} poena!";
                }

            }

            return pobjednikPoruka;
        }
    }
}
