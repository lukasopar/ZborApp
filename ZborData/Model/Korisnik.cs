using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Korisnik
    {
        public Korisnik()
        {
            AdministratorForuma = new HashSet<AdministratorForuma>();
            Anketa = new HashSet<Anketa>();
            ClanNaProjektu = new HashSet<ClanNaProjektu>();
            ClanZbora = new HashSet<ClanZbora>();
            EvidencijaDolaska = new HashSet<EvidencijaDolaska>();
            KomentarObavijesti = new HashSet<KomentarObavijesti>();
            KorisnikUrazgovoru = new HashSet<KorisnikUrazgovoru>();
            LajkKomentara = new HashSet<LajkKomentara>();
            LajkObavijesti = new HashSet<LajkObavijesti>();
            ModForum = new HashSet<ModForum>();
            ModeratorZbora = new HashSet<ModeratorZbora>();
            NajavaDolaska = new HashSet<NajavaDolaska>();
            Obavijest = new HashSet<Obavijest>();
            OdgovorKorisnikaNaAnketu = new HashSet<OdgovorKorisnikaNaAnketu>();
            OsobneObavijesti = new HashSet<OsobneObavijesti>();
            Poruka = new HashSet<Poruka>();
            PozivZaProjekt = new HashSet<PozivZaProjekt>();
            PozivZaZbor = new HashSet<PozivZaZbor>();
            PretplataNaProjekt = new HashSet<PretplataNaProjekt>();
            PretplataNaZbor = new HashSet<PretplataNaZbor>();
            PrijavaZaProjekt = new HashSet<PrijavaZaProjekt>();
            PrijavaZaZbor = new HashSet<PrijavaZaZbor>();
            RepozitorijKorisnik = new HashSet<RepozitorijKorisnik>();
            RepozitorijZbor = new HashSet<RepozitorijZbor>();
            Tema = new HashSet<Tema>();
            Voditelj = new HashSet<Voditelj>();
            Zapis = new HashSet<Zapis>();
        }

        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Slika { get; set; }

        public Guid Id { get; set; }
        public string Opis { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public virtual ICollection<AdministratorForuma> AdministratorForuma { get; set; }
        public virtual ICollection<Anketa> Anketa { get; set; }
        public virtual ICollection<ClanNaProjektu> ClanNaProjektu { get; set; }
        public virtual ICollection<ClanZbora> ClanZbora { get; set; }
        public virtual ICollection<EvidencijaDolaska> EvidencijaDolaska { get; set; }
        public virtual ICollection<KomentarObavijesti> KomentarObavijesti { get; set; }
        public virtual ICollection<KorisnikUrazgovoru> KorisnikUrazgovoru { get; set; }
        public virtual ICollection<LajkKomentara> LajkKomentara { get; set; }
        public virtual ICollection<LajkObavijesti> LajkObavijesti { get; set; }
        public virtual ICollection<ModForum> ModForum { get; set; }
        public virtual ICollection<ModeratorZbora> ModeratorZbora { get; set; }
        public virtual ICollection<NajavaDolaska> NajavaDolaska { get; set; }
        public virtual ICollection<Obavijest> Obavijest { get; set; }
        public virtual ICollection<OdgovorKorisnikaNaAnketu> OdgovorKorisnikaNaAnketu { get; set; }
        public virtual ICollection<OsobneObavijesti> OsobneObavijesti { get; set; }
        public virtual ICollection<Poruka> Poruka { get; set; }
        public virtual ICollection<PozivZaProjekt> PozivZaProjekt { get; set; }
        public virtual ICollection<PozivZaZbor> PozivZaZbor { get; set; }
        public virtual ICollection<PretplataNaProjekt> PretplataNaProjekt { get; set; }
        public virtual ICollection<PretplataNaZbor> PretplataNaZbor { get; set; }
        public virtual ICollection<PrijavaZaProjekt> PrijavaZaProjekt { get; set; }
        public virtual ICollection<PrijavaZaZbor> PrijavaZaZbor { get; set; }
        public virtual ICollection<RepozitorijKorisnik> RepozitorijKorisnik { get; set; }
        public virtual ICollection<RepozitorijZbor> RepozitorijZbor { get; set; }

        public virtual ICollection<Tema> Tema { get; set; }
        public virtual ICollection<Voditelj> Voditelj { get; set; }
        public virtual ICollection<Zapis> Zapis { get; set; }

        public string ImeIPrezime()
        {
            return Ime + " " + Prezime;
        }
    }
}
