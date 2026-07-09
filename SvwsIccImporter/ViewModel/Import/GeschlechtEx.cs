namespace SvwsIccImporter.ViewModel.Import
{
    public static class GeschlechtEx
    {
        public static string ToIccGender(this int geschlecht)
        {
            switch(geschlecht)
            {
                case 3:
                    return "male";

                case 4:
                    return "female";

                default:
                    return "x";
            }
        }
    }
}
