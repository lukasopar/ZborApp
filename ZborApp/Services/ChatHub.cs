using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.ViewModels.JSONModels;
using ZborDataStandard.Account;
using ZborDataStandard.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ZborApp.Services
{
    [Authorize(AuthenticationSchemes = "Identity.Application" + ","+JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatHub(ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }
      
     
        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        public async Task NeprocitanePoruke()
        {
            var user = new  { Id = Guid.Parse(Context.User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value) };
            var neprocitane = _ctx.Razgovor.Where(r => r.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id && k.Procitano == false).Count() > 0).Select(r => r.Id.GetHashCode()).ToList();
            await Clients.User(user.Id.ToString()).SendAsync("Neprocitane", neprocitane );
            //poslati poruku da je procitano
        }
        public async Task NeprocitaneObavijesti()
        {
            var user = new { Id = Guid.Parse(Context.User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value) };
            var neprocitane = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id && r.Procitano == false).Select(r => r.Id.GetHashCode()).ToList();
            await Clients.User(user.Id.ToString()).SendAsync("NeprocitaneObavijesti", neprocitane);
            //poslati poruku da je procitano
        }
        public async Task Procitano(Guid idRazg)
        {
            var user = new { Id = Guid.Parse(Context.User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value) };
            KorisnikUrazgovoru k = _ctx.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id && k.IdRazgovor == idRazg).SingleOrDefault();
            k.Procitano = true;
            _ctx.SaveChanges();
            await NeprocitanePoruke();
            await Clients.User(user.Id.ToString()).SendAsync("ProcitanaPoruka", idRazg);


        }
        public async Task SendMessage(PorukaModel poruka)
        {
            string name = Context.User.Identity.Name;

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
                await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessageMob", new { Id = novaPoruka.Id, IdKorisnik = novaPoruka.IdKorisnik, IdRazgovor=novaPoruka.IdRazgovor, Poruka1 = novaPoruka.Poruka1.Replace("<br />", "\n"), DatumIvrijeme = novaPoruka.DatumIvrijeme }, user.ImeIPrezime()) ;
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
                    await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessageMob", new { Id = novaPoruka.Id, IdKorisnik = novaPoruka.IdKorisnik, IdRazgovor = novaPoruka.IdRazgovor, Poruka1 = novaPoruka.Poruka1.Replace("<br />", "\n"), DatumIvrijeme = novaPoruka.DatumIvrijeme }, user.ImeIPrezime());
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

                var list = new List<Poruka>();
                list.Add(new Poruka
                {
                    DatumIvrijeme = novaPoruka.DatumIvrijeme,
                    Id = novaPoruka.Id,
                    IdKorisnik = novaPoruka.IdKorisnik,
                    IdRazgovor = novaPoruka.IdRazgovor,
                    Poruka1 = novaPoruka.Poruka1.Replace("<br />", "\n")
                });
                var copyRazg = new Razgovor
                {
                    Id = noviRazg.Id,
                    Naslov = noviRazg.Naslov,
                    DatumZadnjePoruke = noviRazg.DatumZadnjePoruke,
                    KorisnikUrazgovoru = noviRazg.KorisnikUrazgovoru,
                    Poruka = list
                };
                foreach (var id in listaId)
                {
                    await Clients.User(id.ToString()).SendAsync("ReceiveNewConversationMob", copyRazg, user.ImeIPrezime());

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
            }
        }
    }
}