using System.Runtime.InteropServices;

namespace SvwsClient.Tool.Unterricht
{
    public class Unterricht
    {
        public Fach Fach { get; set; }

        public List<Lehrkraft> Lehrkraefte { get; set; } = new List<Lehrkraft>();

        public List<Klasse> Klassen { get; set; } = new List<Klasse>();

        public long? KursId { get; set; }

        public string KursBezeichnung { get; set; } = string.Empty;

        public List<Mitgliedschaft> Kinder { get; set; } = new List<Mitgliedschaft>();

        public bool IstSichtbar { get; set; } = true;
    }
}
