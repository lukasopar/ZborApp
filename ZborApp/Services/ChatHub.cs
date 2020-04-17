using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ZborApp.Models.JSONModels;
using ZborData.Account;
using ZborData.Model;

namespace ZborApp.Services
{
    public class ChatHub : Hub
    {
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatHub(ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public async Task SendMessage(PorukaModel poruka)
        {
            var razg = _ctx.Razgovor.Where(razg => razg.Id == Guid.Parse(poruka.IdRazg)).Include(razg => razg.KorisnikUrazgovoru).SingleOrDefault();
            poruka.Message=poruka.Message.Replace("\n", "<br />");
            var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
            poruka.Slika = user.IdSlika.ToString();
            poruka.Ime = user.Ime;
            var when = DateTime.ParseExact(poruka.When, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            poruka.When = when.ToString("dd.MM.yyyy. | HH:mm");

            Poruka novaPoruka = new Poruka
            {
                DatumIvrijeme =when,
                Id = Guid.NewGuid(),
                IdKorisnik = Guid.Parse(poruka.IdUser),
                IdRazgovor = Guid.Parse(poruka.IdRazg),
                Poruka1 = poruka.Message
            };
            razg.DatumZadnjePoruke = when;
            _ctx.Add(novaPoruka);
            foreach (KorisnikUrazgovoru k in razg.KorisnikUrazgovoru)
            {
                if (k.IdKorisnik != novaPoruka.IdKorisnik)
                    k.Procitano = false;
                else
                    k.Procitano = true;
               

            }
            _ctx.SaveChanges();
            foreach (KorisnikUrazgovoru k in razg.KorisnikUrazgovoru)
            {
            
                await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessage", k.IdKorisnik, poruka);
                await Clients.User(k.IdKorisnik.ToString()).SendAsync("ChangeHeader", new
                {
                    Id = novaPoruka.IdRazgovor,
                    Naziv = razg.Naslov + " (" + user.ImeIPrezime() + ")",
                    Datum = novaPoruka.DatumIvrijeme.ToString("dd.MM.yyyy. hh:mm"),
                    Slika = user.IdSlika,
                    Poruka = novaPoruka.Poruka1,
                    Procitano = k.Procitano
                });

            }
        }

        public async Task NewConversation(PorukaModel poruka)
        {
            var listaId = poruka.Kontakti.Select(p => Guid.Parse(p)).ToList();
            listaId.Add(Guid.Parse(poruka.IdUser));
            //glupi uvjet, treba bolji napisat
            var razg = _ctx.Razgovor.Where(razg => razg.KorisnikUrazgovoru.Select(k => k.IdKorisnik).All(id => listaId.Contains(id)) && razg.KorisnikUrazgovoru.Count() == listaId.Count()).Include(k=>k.KorisnikUrazgovoru).SingleOrDefault();
            var when = DateTime.ParseExact(poruka.When, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            poruka.Message = poruka.Message.Replace("\n", "<br />");
            poruka.When = when.ToString("dd.MM.yyyy. | HH:mm");

            if (razg != null)
            {
                var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
                poruka.Slika = user.IdSlika.ToString();
                poruka.Ime = user.Ime;
                poruka.IdRazg = razg.Id.ToString();
                Poruka novaPoruka = new Poruka
                {
                    DatumIvrijeme = when,
                    Id = Guid.NewGuid(),
                    IdKorisnik = Guid.Parse(poruka.IdUser),
                    IdRazgovor = Guid.Parse(poruka.IdRazg),
                    Poruka1 = poruka.Message
                };
                razg.DatumZadnjePoruke = when;
                _ctx.Add(novaPoruka);
                foreach (KorisnikUrazgovoru k in razg.KorisnikUrazgovoru)
                {
                    if (k.IdKorisnik != novaPoruka.IdKorisnik)
                        k.Procitano = false;
                    else
                        k.Procitano = true;
                    await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessage", k.IdKorisnik, poruka);
                    await Clients.User(k.IdKorisnik.ToString()).SendAsync("ChangeHeader", new
                    {
                        Id = novaPoruka.IdRazgovor,
                        Naziv = razg.Naslov + " (" + user.ImeIPrezime() + ")",
                        Datum = novaPoruka.DatumIvrijeme.ToString("dd.MM.yyyy. | HH:mm"),
                        Slika = user.IdSlika,
                        Poruka = novaPoruka.Poruka1,
                        Procitano = k.Procitano
                    });
                }
                _ctx.SaveChanges();

            }
            else
            {
                //Treba napraviti novi;
                var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
                poruka.Slika = user.IdSlika.ToString();
                poruka.Ime = user.Ime;
                Poruka novaPoruka = new Poruka
                {
                    DatumIvrijeme = when,
                    Id = Guid.NewGuid(),
                    IdKorisnik = Guid.Parse(poruka.IdUser),
                    Poruka1 = poruka.Message
                };
                Razgovor noviRazg = new Razgovor
                {
                    Id = Guid.NewGuid(),
                    Naslov = "",
                    DatumZadnjePoruke = when,

                };
                novaPoruka.IdRazgovor = noviRazg.Id;
                noviRazg.Poruka.Add(novaPoruka);
                poruka.IdRazg = noviRazg.Id.ToString();
                poruka.ImeRazgovora = "";
               
                foreach (var id in listaId)
                {
                    KorisnikUrazgovoru k = new KorisnikUrazgovoru
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = id,
                        IdRazgovor = noviRazg.Id,
                        IdKorisnikNavigation = _ctx.Korisnik.Find(id)
                    };
                    if (k.IdKorisnik != novaPoruka.IdKorisnik)
                        k.Procitano = false;
                    else
                        k.Procitano = true;
                    noviRazg.KorisnikUrazgovoru.Add(k);

                }
                poruka.Popis = noviRazg.GetPopisKorisnika(novaPoruka.IdKorisnik);
                foreach (var id in listaId)
                {
                    await Clients.User(id.ToString()).SendAsync("ReceiveNewConversation", id, poruka);
                    bool flag = false;
                    if (id == user.Id) flag = true;
                    await Clients.User(id.ToString()).SendAsync("ChangeHeader", new
                    {
                        Id = novaPoruka.IdRazgovor,
                        Naziv = noviRazg.Naslov + " (" + user.ImeIPrezime() + ")",
                        Datum = novaPoruka.DatumIvrijeme.ToString("dd.MM.yyyy. | HH:mm"),
                        Slika = user.IdSlika,
                        Poruka = novaPoruka.Poruka1,
                        Procitano = flag
                    });
                }
                _ctx.Add(noviRazg);
                _ctx.SaveChanges();
                int g = 0;
            }
        }
    }
}