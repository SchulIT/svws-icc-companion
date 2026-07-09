using SchulIT.SvwsClient;

namespace SvwsClient.Tool.Unterricht
{
    public class UnterrichtResolver
    {

        private async Task<List<LeistungsdatenIntern>> GetLeistungsdatenAsync(SvwsRestClient client, string schema, long abschnittId)
        {
            var result = new List<LeistungsdatenIntern>();
            var schuelerListe = await client.GetSchuelerFuerAbschnittAsync(schema, abschnittId);
            var schuelerListeAktuellerAbschnitt = schuelerListe.Where(x => x.IdSchuljahresabschnitt == abschnittId).ToArray();

            foreach (var schueler in schuelerListeAktuellerAbschnitt)
            {
                var lernabschnittsdaten = await client.GetSchuelerLernabschnittsdatenAsync(schema, schueler.Id, abschnittId);

                foreach (var leistungsdaten in lernabschnittsdaten.Leistungsdaten)
                {
                    result.Add(new LeistungsdatenIntern(leistungsdaten, lernabschnittsdaten));
                }
            }

            return result;
        }

        public async Task<List<Unterricht>> Resolve(SvwsRestClient client, string schema, long abschnittId)
        {
            var faecher = await client.GetFaecherAsync(schema);
            var faecherDict = faecher.ToDictionary(x => x.Id);

            var klassen = await client.GetKlassenFuerAbschnittAsync(schema, abschnittId);
            var klassenDict = klassen.ToDictionary(x => x.Id);

            var lehrkraefte = await client.GetLehrerAsync(schema);
            var lehrkraefteDict = lehrkraefte.ToDictionary(x => x.Id);

            var leistungsdaten = await GetLeistungsdatenAsync(client, schema, abschnittId);

            var klassenunterrichte = leistungsdaten
                .Where(x => x.Leistungsdaten.KursID == null)
                .GroupBy(x => new { x.Leistungsdaten.FachID, x.Leistungsdaten.LehrerID, x.Leistungsdaten.ZusatzkraftID, x.Lernabschnittsdaten.KlassenID, x.Leistungsdaten.Kursart })
                .Select(x =>
                {
                    var first = x.First();

                    if(!faecherDict.ContainsKey(first.Leistungsdaten.FachID))
                    {
                        return null;
                    }

                    var unterricht = new Unterricht
                    {
                        KursId = null,
                        Fach = new Fach(first.Leistungsdaten.FachID, faecherDict[first.Leistungsdaten.FachID].Kuerzel)
                    };

                    if (first.Leistungsdaten.LehrerID != null && lehrkraefteDict.TryGetValue(first.Leistungsdaten.LehrerID.Value, out LehrerListeEintrag? lehrkraft))
                    {
                        unterricht.Lehrkraefte.Add(new Lehrkraft(first.Leistungsdaten.LehrerID.Value, lehrkraft.Kuerzel, lehrkraft.IstSichtbar));
                    }

                    if (first.Leistungsdaten.ZusatzkraftID != null && lehrkraefteDict.TryGetValue(first.Leistungsdaten.ZusatzkraftID.Value, out LehrerListeEintrag? weitereLehrkraft))
                    {
                        unterricht.Lehrkraefte.Add(new Lehrkraft(first.Leistungsdaten.ZusatzkraftID.Value, weitereLehrkraft.Kuerzel, weitereLehrkraft.IstSichtbar));
                    }

                    foreach(var ld in x)
                    {
                        if (ld.Lernabschnittsdaten.KlassenID != null)
                        {
                            var klasse = ld.Lernabschnittsdaten.KlassenID.Value;

                            if(!unterricht.Klassen.Where(x => x.Id == ld.Lernabschnittsdaten.KlassenID).Any() && klassenDict.TryGetValue(ld.Lernabschnittsdaten.KlassenID.Value, out KlassenDaten? klassenDaten))
                            {
                                unterricht.Klassen.Add(new Klasse(ld.Lernabschnittsdaten.KlassenID.Value, klassenDaten.Kuerzel));
                            }
                        }

                        unterricht.Kinder.Add(new Mitgliedschaft
                        {
                            Art = ld.Leistungsdaten.Kursart,
                            KindId = ld.Lernabschnittsdaten.SchuelerID
                        });
                    }

                    return unterricht;
                })
                .Where(x => x != null)
                .ToList();

            var kursunterrichte = new List<Unterricht>();
            var kursListe = await client.GetKurseFuerAbschnittAsync(schema, abschnittId);

            foreach(var kurs in kursListe)
            {
                if (!faecherDict.ContainsKey(kurs.IdFach))
                {
                    continue;
                }

                var unterricht = new Unterricht
                {
                    KursId = kurs.Id,
                    KursBezeichnung = kurs.BezeichnungZeugnis,
                    IstSichtbar = kurs.IstSichtbar,
                    Fach = new Fach(kurs.IdFach, faecherDict[kurs.IdFach].Kuerzel),
                    Lehrkraefte = kurs.WeitereLehrer.Select(x => x.IdLehrer != null && lehrkraefteDict.ContainsKey(x.IdLehrer.Value) ? lehrkraefteDict[x.IdLehrer.Value] : null).Where(x => x != null).Select(x => new Lehrkraft(x.Id, x.Kuerzel, x.IstSichtbar)).ToList()
                };

                if (kurs.Lehrer != null && lehrkraefteDict.ContainsKey(kurs.Lehrer.Value) && !unterricht.Lehrkraefte.Any(x => x.Id == kurs.Lehrer.Value))
                {
                    unterricht.Lehrkraefte.Add(new Lehrkraft(kurs.Lehrer.Value, lehrkraefteDict[kurs.Lehrer.Value].Kuerzel, lehrkraefteDict[kurs.Lehrer.Value].IstSichtbar));
                }

                foreach(var kind in leistungsdaten.Where(x => x.Leistungsdaten.KursID == kurs.Id))
                {
                    unterricht.Kinder.Add(new Mitgliedschaft
                    {
                        KindId = kind.Lernabschnittsdaten.SchuelerID,
                        Art = kind.Leistungsdaten.Kursart
                    });
                }

                kursunterrichte.Add(unterricht);
            }

            return klassenunterrichte.Union(kursunterrichte).ToList();
        }
    }
}
