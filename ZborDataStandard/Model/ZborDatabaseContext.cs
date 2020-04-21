using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ZborDataStandard.Model
{
    public partial class ZborDatabaseContext : DbContext
    {
        public ZborDatabaseContext()
        {
        }

        public ZborDatabaseContext(DbContextOptions<ZborDatabaseContext> options)
            : base(options)
        {
        }
        public virtual DbSet<AdministratorForuma> AdministratorForuma { get; set; }
        public virtual DbSet<Anketa> Anketa { get; set; }
        public virtual DbSet<ClanNaProjektu> ClanNaProjektu { get; set; }
        public virtual DbSet<ClanZbora> ClanZbora { get; set; }
        public virtual DbSet<Dogadjaj> Dogadjaj { get; set; }
        public virtual DbSet<EvidencijaDolaska> EvidencijaDolaska { get; set; }
        public virtual DbSet<Forum> Forum { get; set; }
        public virtual DbSet<KategorijaForuma> KategorijaForuma { get; set; }
        public virtual DbSet<KomentarObavijesti> KomentarObavijesti { get; set; }
        public virtual DbSet<Korisnik> Korisnik { get; set; }
        public virtual DbSet<KorisnikUrazgovoru> KorisnikUrazgovoru { get; set; }
        public virtual DbSet<LajkKomentara> LajkKomentara { get; set; }
        public virtual DbSet<LajkObavijesti> LajkObavijesti { get; set; }
        public virtual DbSet<ModForum> ModForum { get; set; }

        public virtual DbSet<ModeratorZbora> ModeratorZbora { get; set; }
        public virtual DbSet<NajavaDolaska> NajavaDolaska { get; set; }
        public virtual DbSet<Obavijest> Obavijest { get; set; }
        public virtual DbSet<ObavijestVezanaUzProjekt> ObavijestVezanaUzProjekt { get; set; }
        public virtual DbSet<OdgovorAnkete> OdgovorAnkete { get; set; }
        public virtual DbSet<OdgovorKorisnikaNaAnketu> OdgovorKorisnikaNaAnketu { get; set; }
        public virtual DbSet<OsobneObavijesti> OsobneObavijesti { get; set; }
        public virtual DbSet<Poruka> Poruka { get; set; }
        public virtual DbSet<PozivZaProjekt> PozivZaProjekt { get; set; }
        public virtual DbSet<PozivZaZbor> PozivZaZbor { get; set; }
        public virtual DbSet<ProfilZbor> ProfilZbor { get; set; }
        public virtual DbSet<PretplataNaZbor> PretplataNaZbor { get; set; }
        public virtual DbSet<PretplataNaProjekt> PretplataNaProjekt { get; set; }
        public virtual DbSet<PrijavaZaProjekt> PrijavaZaProjekt { get; set; }
        public virtual DbSet<PrijavaZaZbor> PrijavaZaZbor { get; set; }
        public virtual DbSet<Projekt> Projekt { get; set; }
        public virtual DbSet<Razgovor> Razgovor { get; set; }
        public virtual DbSet<RepozitorijKorisnik> RepozitorijKorisnik { get; set; }
        public virtual DbSet<RepozitorijZbor> RepozitorijZbor { get; set; }

        public virtual DbSet<Tema> Tema { get; set; }
        public virtual DbSet<Trosak> Trosak { get; set; }
        public virtual DbSet<Voditelj> Voditelj { get; set; }
        public virtual DbSet<VrstaDogadjaja> VrstaDogadjaja { get; set; }
        public virtual DbSet<VrstaPodjele> VrstaPodjele { get; set; }
        public virtual DbSet<Zapis> Zapis { get; set; }
        public virtual DbSet<Zbor> Zbor { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=LAP-SOPAR;Initial Catalog=ZborDatabase;User Id=LAP-SOPAR\\\\\\\\Luka;Password=;Trusted_Connection=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdministratorForuma>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();



                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.AdministratorForuma)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AdministratorForuma_Korisnik");
            });
            modelBuilder.Entity<Anketa>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumKraja).HasColumnType("datetime");

                entity.Property(e => e.DatumPostavljanja).HasColumnType("datetime");

                entity.Property(e => e.Pitanje)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.Anketa)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Anketa_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.Anketa)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Anketa_Zbor");
            });

            modelBuilder.Entity<ClanNaProjektu>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Uloga)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.ClanNaProjektu)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClanNaProjektu_Korisnik");

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.ClanNaProjektu)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ClanNaProjektu_Projekt");
            });

            modelBuilder.Entity<ClanZbora>(entity =>
            {
                entity.HasIndex(e => new { e.IdZbor, e.IdKorisnik })
                    .HasName("UK_SamoJednomClan")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPridruzivanja).HasColumnType("datetime");

                entity.Property(e => e.Glas)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.ClanZbora)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClanZbora_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.ClanZbora)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ClanZbora_Zbor");
            });

            modelBuilder.Entity<Dogadjaj>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumIvrijeme)
                    .HasColumnName("DatumIVrijeme")
                    .HasColumnType("datetime");

                entity.Property(e => e.DatumIvrijemeKraja)
                    .HasColumnName("DatumIVrijemeKraja")
                    .HasColumnType("datetime");

                entity.Property(e => e.DodatanOpis).HasMaxLength(400);

                entity.Property(e => e.Lokacija)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.Dogadjaj)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Dogadjaj_Projekt");

                entity.HasOne(d => d.IdProjekt1)
                    .WithMany(p => p.Dogadjaj)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Dogadjaj_VrstaDogadjaja");
            });

            modelBuilder.Entity<EvidencijaDolaska>(entity =>
            {
                entity.HasIndex(e => new { e.IdKorisnik, e.IdDogadjaj })
                    .HasName("UK_SamoJednomEvidencija")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdDogadjajNavigation)
                    .WithMany(p => p.EvidencijaDolaska)
                    .HasForeignKey(d => d.IdDogadjaj)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EvidencijaDolaska_Dogadjaj");

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.EvidencijaDolaska)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EvidencijaDolaska_Korisnik");
            });

            modelBuilder.Entity<Forum>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Opis)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.HasOne(d => d.IdKategorijaForumaNavigation)
                    .WithMany(p => p.Forum)
                    .HasForeignKey(d => d.IdKategorijaForuma)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Forum_IdKategor_1F63A897");
            });
            modelBuilder.Entity<KategorijaForuma>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<KomentarObavijesti>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumObjave).HasColumnType("datetime");

                entity.Property(e => e.Tekst)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.KomentarObavijesti)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KomentarObavijesti_Korisnik");

                entity.HasOne(d => d.IdObavijestNavigation)
                    .WithMany(p => p.KomentarObavijesti)
                    .HasForeignKey(d => d.IdObavijest)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_KomentarObavijesti_Obavijest");
            });

            modelBuilder.Entity<Korisnik>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumRodjenja).HasColumnType("date");

                entity.Property(e => e.Ime)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Opis).HasMaxLength(300);

                entity.Property(e => e.Prezime)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.HasOne(d => d.IdSlikaNavigation)
                    .WithMany(p => p.IdKorisniks)
                    .HasForeignKey(d => d.IdSlika)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Korisnik_RepozitorijKorisnik");
            });

            modelBuilder.Entity<KorisnikUrazgovoru>(entity =>
            {
                entity.ToTable("KorisnikURazgovoru");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.KorisnikUrazgovoru)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KorisnikURazgovoru_Korisnik");

                entity.HasOne(d => d.IdRazgovorNavigation)
                    .WithMany(p => p.KorisnikUrazgovoru)
                    .HasForeignKey(d => d.IdRazgovor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KorisnikURazgovoru_Razgovor");
            });

            modelBuilder.Entity<LajkKomentara>(entity =>
            {
                entity.HasIndex(e => new { e.IdKorisnik, e.IdKomentar })
                    .HasName("UK_SamoJednomLajkKomentar")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdKomentarNavigation)
                    .WithMany(p => p.LajkKomentara)
                    .HasForeignKey(d => d.IdKomentar)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_LajkKomentara_KomentarObavijesti");

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.LajkKomentara)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LajkKomentara_Korisnik");
            });

            modelBuilder.Entity<LajkObavijesti>(entity =>
            {
                entity.HasIndex(e => new { e.IdKorisnik, e.IdObavijest })
                    .HasName("UK_SamoJednomLajkObavijesti")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.LajkObavijesti)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LajkObavijesti_Korisnik");

                entity.HasOne(d => d.IdObavijestNavigation)
                    .WithMany(p => p.LajkObavijesti)
                    .HasForeignKey(d => d.IdObavijest)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_LajkObavijesti_Obavijest");
            });


            modelBuilder.Entity<ModForum>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();



                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.ModForum)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModForum_Korisnik");
            });
            modelBuilder.Entity<ModeratorZbora>(entity =>
            {
                entity.HasIndex(e => new { e.IdZbor, e.IdKorisnik })
                    .HasName("UK_SamoJednomModerator")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.ModeratorZbora)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ModeratorZbora_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.ModeratorZbora)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ModeratorZbora_Zbor");
            });

            modelBuilder.Entity<NajavaDolaska>(entity =>
            {
                entity.HasIndex(e => new { e.IdKorisnik, e.IdDogadjaj })
                    .HasName("UK_SamoJednomNajavar")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdDogadjajNavigation)
                    .WithMany(p => p.NajavaDolaska)
                    .HasForeignKey(d => d.IdDogadjaj)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NajavaDolaska_Dogadjaj");

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.NajavaDolaska)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NajavaDolaska_Korisnik");
            });

            modelBuilder.Entity<Obavijest>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumObjave).HasColumnType("datetime");

                entity.Property(e => e.Naslov)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Tekst)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.Obavijest)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Obavijest_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.Obavijest)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Obavijest_Zbor");
            });

            modelBuilder.Entity<ObavijestVezanaUzProjekt>(entity =>
            {
                entity.HasIndex(e => new { e.IdProjekt, e.IdObavijest })
                    .HasName("UK_SamoJednomObavijestProjekt")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdObavijestNavigation)
                    .WithMany(p => p.ObavijestVezanaUzProjekt)
                    .HasForeignKey(d => d.IdObavijest)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ObavijestVezanaUzProjekt_Obavijest");

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.ObavijestVezanaUzProjekt)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ObavijestVezanaUzProjekt_Projekt");
            });

            modelBuilder.Entity<OdgovorAnkete>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Odgovor)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.IdAnketaNavigation)
                    .WithMany(p => p.OdgovorAnkete)
                    .HasForeignKey(d => d.IdAnketa)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_OdgovorAnkete_Anketa");
            });

            modelBuilder.Entity<OdgovorKorisnikaNaAnketu>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumOdgovora).HasColumnType("datetime");

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.OdgovorKorisnikaNaAnketu)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OdgovorKorisnikaNaAnketu_Korisnik");

                entity.HasOne(d => d.IdOdgovorNavigation)
                    .WithMany(p => p.OdgovorKorisnikaNaAnketu)
                    .HasForeignKey(d => d.IdOdgovor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_OdgovorKorisnikaNaAnketu_OdgovorAnkete");
            });

            modelBuilder.Entity<OsobneObavijesti>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

               

                entity.Property(e => e.Tekst)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.OsobneObavijesti)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OsobneObavijesti_Korisnik");
            });
            modelBuilder.Entity<ProfilZbor>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdZborNavigation)
                    .WithOne(p => p.ProfilZbor)
                    .HasForeignKey<ProfilZbor>(d => d.Id)
                    .HasConstraintName("FK_ProfilZbor_Zbor");

            });
            modelBuilder.Entity<Poruka>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumIvrijeme)
                    .HasColumnName("DatumIVrijeme")
                    .HasColumnType("datetime");

                entity.Property(e => e.Poruka1)
                    .IsRequired()
                    .HasColumnName("Poruka")
                    .HasMaxLength(1000);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.Poruka)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Poruka_Korisnik");

                entity.HasOne(d => d.IdRazgovorNavigation)
                    .WithMany(p => p.Poruka)
                    .HasForeignKey(d => d.IdRazgovor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Poruka_Razgovor");
            });

            modelBuilder.Entity<PozivZaProjekt>(entity =>
            {
                entity.HasIndex(e => new { e.IdProjekt, e.IdKorisnik })
                    .HasName("UK_SamoJednomPozivProjekt")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPoziva).HasColumnType("datetime");

                entity.Property(e => e.Poruka).HasMaxLength(300);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.PozivZaProjekt)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PozivZaProjekt_Korisnik");

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.PozivZaProjekt)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PozivZaProjekt_Projekt");
            });

            modelBuilder.Entity<PozivZaZbor>(entity =>
            {
                entity.HasIndex(e => new { e.IdZbor, e.IdKorisnik })
                    .HasName("UK_SamoJednomPoziv")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPoziva).HasColumnType("datetime");

                entity.Property(e => e.Poruka).HasMaxLength(300);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.PozivZaZbor)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PozivZaZbor_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.PozivZaZbor)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PozivZaZbor_Zbor");
            });

            modelBuilder.Entity<PretplataNaProjekt>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.PretplataNaProjekt)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PretplataNaProjekt_Korisnik");

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.PretplataNaProjekt)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PretplataNaProjekt_Projekt");
            });
            modelBuilder.Entity<PretplataNaZbor>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.PretplataNaZbor)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PretplataNaZbor_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.PretplataNaZbor)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PretplataNaZbor_Zbor");
            });

            modelBuilder.Entity<PrijavaZaProjekt>(entity =>
            {
                entity.HasIndex(e => new { e.IdProjekt, e.IdKorisnik })
                    .HasName("UK_SamoJednomPrijavaProjekt")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPrijave).HasColumnType("datetime");

                entity.Property(e => e.Poruka).HasMaxLength(300);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.PrijavaZaProjekt)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PrijavaZaProjekt_Korisnik");

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.PrijavaZaProjekt)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PrijavaZaProjekt_Projekt");
            });

            modelBuilder.Entity<PrijavaZaZbor>(entity =>
            {
                entity.HasIndex(e => new { e.IdZbor, e.IdKorisnik })
                    .HasName("UK_SamoJednomPrijava")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPrijave).HasColumnType("datetime");

                entity.Property(e => e.Poruka).HasMaxLength(300);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.PrijavaZaZbor)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PrijavaZaZbor_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.PrijavaZaZbor)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PrijavaZaZbor_Zbor");
            });

            modelBuilder.Entity<Projekt>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPocetka).HasColumnType("date");

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Opis)
                    .IsRequired()
                    .HasMaxLength(800);

                entity.HasOne(d => d.IdVrstePodjeleNavigation)
                    .WithMany(p => p.Projekt)
                    .HasForeignKey(d => d.IdVrstePodjele)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Projekt_VrstaPodjele");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.Projekt)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Projekt_Zbor");
            });

            modelBuilder.Entity<Razgovor>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumZadnjePoruke).HasColumnType("datetime");

                entity.Property(e => e.Naslov)
                    .IsRequired()
                    .HasMaxLength(50);
            });
            modelBuilder.Entity<RepozitorijKorisnik>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPostavljanja).HasColumnType("datetime");

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(400);
                entity.Property(e => e.Url)
                                    .IsRequired()
                                    .HasMaxLength(400);
                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.RepozitorijKorisnik)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RepozitorijKorisnik_Korisnik");

            });
            modelBuilder.Entity<RepozitorijZbor>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPostavljanja).HasColumnType("datetime");

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(400);
                entity.Property(e => e.Url)
                                    .IsRequired()
                                    .HasMaxLength(400);
                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.RepozitorijZbor)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RepozitorijZbor_Korisnik");
                entity.HasOne(d => d.IdZborNavigation)
                   .WithMany(p => p.RepozitorijZbor)
                   .HasForeignKey(d => d.IdZbor)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_RepozitorijZbor_Zbor");

            });

            modelBuilder.Entity<Tema>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPocetka).HasColumnType("datetime");

                entity.Property(e => e.Naslov)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.Tema)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tema_IdKorisnik_3587F3E0");
                entity.HasOne(d => d.IdForumNavigation)
                   .WithMany(p => p.Tema)
                   .HasForeignKey(d => d.IdForum)
                   .OnDelete(DeleteBehavior.ClientSetNull)
                   .HasConstraintName("FK_Tema_IdForum_2057CCD0");
            });

            modelBuilder.Entity<Trosak>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Naslov)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Opis)
                    .IsRequired()
                    .HasMaxLength(800);

                entity.HasOne(d => d.IdProjektNavigation)
                    .WithMany(p => p.Trosak)
                    .HasForeignKey(d => d.IdProjekt)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Trosak_Projekt");
            });

            modelBuilder.Entity<Voditelj>(entity =>
            {
                entity.HasIndex(e => new { e.IdZbor, e.IdKorisnik })
                    .HasName("UK_SamoJednomVoditelj")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumPostanka).HasColumnType("date");

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.Voditelj)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Voditelj_Korisnik");

                entity.HasOne(d => d.IdZborNavigation)
                    .WithMany(p => p.Voditelj)
                    .HasForeignKey(d => d.IdZbor)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Voditelj_Zbor");
            });

            modelBuilder.Entity<VrstaDogadjaja>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.Opis)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            modelBuilder.Entity<VrstaPodjele>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Podjela)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Zapis>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DatumIvrijeme)
                    .HasColumnName("DatumIVrijeme")
                    .HasColumnType("datetime");

                entity.Property(e => e.Tekst)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.HasOne(d => d.IdKorisnikNavigation)
                    .WithMany(p => p.Zapis)
                    .HasForeignKey(d => d.IdKorisnik)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Zapis_Korisnik");

                entity.HasOne(d => d.IdTemaNavigation)
                    .WithMany(p => p.Zapis)
                    .HasForeignKey(d => d.IdTema)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Zapis_IdTema_214BF109");
            });

            modelBuilder.Entity<Zbor>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Adresa)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.DatumOsnutka).HasColumnType("date");

                entity.Property(e => e.Naziv)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.Opis).HasMaxLength(1000);
                entity.HasOne(d => d.IdSlikaNavigation)
                    .WithMany(p => p.IdZbors)
                    .HasForeignKey(d => d.IdSlika)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Zbor_RepozitorijZbor");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
