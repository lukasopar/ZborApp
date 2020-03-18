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
            foreach (KorisnikUrazgovoru k in razg.KorisnikUrazgovoru)
                await Clients.User(k.IdKorisnik.ToString()).SendAsync("ReceiveMessage", k.IdKorisnik, poruka);
          
        }

        public async Task NewConversation(PorukaModel poruka)
        {
            var listaId = poruka.Kontakti.Select(p => Guid.Parse(p)).ToList();
            listaId.Add(Guid.Parse(poruka.IdUser));
            //glupi uvjet, treba bolji napisat
            var razg = _ctx.Razgovor.Where(razg => razg.KorisnikUrazgovoru.Select(k => k.IdKorisnik).All(id => listaId.Contains(id)) && razg.KorisnikUrazgovoru.Count() == listaId.Count()).SingleOrDefault();

            if (razg != null)
            {
                var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
                poruka.Slika = user.Slika;
                poruka.Ime = user.Ime;
                poruka.IdRazg = razg.Id.ToString();
                foreach (var id in listaId)
                    await Clients.User(id.ToString()).SendAsync("ReceiveMessage", id, poruka);
            }
            else
            {
                //Treba napraviti novi;
                var user = _ctx.Korisnik.Where(k => k.Id == Guid.Parse(poruka.IdUser)).SingleOrDefault();
                poruka.Slika = user.Slika;
                poruka.Ime = user.Ime;
                poruka.IdRazg = Guid.NewGuid().ToString();
                poruka.ImeRazgovora = "Razgovor";
                poruka.Popis = "Kruno Jurčić";
                foreach (var id in listaId)
                    await Clients.User(id.ToString()).SendAsync("ReceiveNewConversation", id, poruka);
            }
        }
    }
}