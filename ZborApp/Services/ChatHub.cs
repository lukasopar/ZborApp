using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
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
            var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
            poruka.Slika = user.Slika;
            poruka.Ime = user.Ime;
            Poruka novaPoruka = new Poruka
            {
                DatumIvrijeme = poruka.When,
                Id = Guid.NewGuid(),
                IdKorisnik = Guid.Parse(poruka.IdUser),
                IdRazgovor = Guid.Parse(poruka.IdRazg),
                Poruka1 = poruka.Message
            };
            razg.DatumZadnjePoruke = poruka.When;
            _ctx.Add(novaPoruka);
            foreach (KorisnikUrazgovoru k in razg.KorisnikUrazgovoru)
            {
                if (k.IdKorisnik != novaPoruka.IdKorisnik)
                    k.Procitano = false;
                else
                    k.Procitano = true;
                await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessage", k.IdKorisnik, poruka);
            }
            _ctx.SaveChanges();
        }

        public async Task NewConversation(PorukaModel poruka)
        {
            var listaId = poruka.Kontakti.Select(p => Guid.Parse(p)).ToList();
            listaId.Add(Guid.Parse(poruka.IdUser));
            //glupi uvjet, treba bolji napisat
            var razg = _ctx.Razgovor.Where(razg => razg.KorisnikUrazgovoru.Select(k => k.IdKorisnik).All(id => listaId.Contains(id)) && razg.KorisnikUrazgovoru.Count() == listaId.Count()).Include(k=>k.KorisnikUrazgovoru).SingleOrDefault();

            if (razg != null)
            {
                var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
                poruka.Slika = user.Slika;
                poruka.Ime = user.Ime;
                poruka.IdRazg = razg.Id.ToString();
                Poruka novaPoruka = new Poruka
                {
                    DatumIvrijeme = poruka.When,
                    Id = Guid.NewGuid(),
                    IdKorisnik = Guid.Parse(poruka.IdUser),
                    IdRazgovor = Guid.Parse(poruka.IdRazg),
                    Poruka1 = poruka.Message
                };
                razg.DatumZadnjePoruke = poruka.When;
                _ctx.Add(novaPoruka);
                foreach (KorisnikUrazgovoru k in razg.KorisnikUrazgovoru)
                {
                    if (k.IdKorisnik != novaPoruka.IdKorisnik)
                        k.Procitano = false;
                    else
                        k.Procitano = true;
                    await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessage", k.IdKorisnik, poruka);
                }
                _ctx.SaveChanges();

            }
            else
            {
                //Treba napraviti novi;
                var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
                poruka.Slika = user.Slika;
                poruka.Ime = user.Ime;
                Poruka novaPoruka = new Poruka
                {
                    DatumIvrijeme = poruka.When,
                    Id = Guid.NewGuid(),
                    IdKorisnik = Guid.Parse(poruka.IdUser),
                    Poruka1 = poruka.Message
                };
                Razgovor noviRazg = new Razgovor
                {
                    Id = Guid.NewGuid(),
                    Naslov = "",
                    DatumZadnjePoruke = poruka.When,

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
                }
                _ctx.Add(noviRazg);
                _ctx.SaveChanges();
                int g = 0;
            }
        }
    }
}