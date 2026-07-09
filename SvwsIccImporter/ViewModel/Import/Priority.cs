using System;

namespace SvwsIccImporter.ViewModel.Import
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Priority : Attribute
    {
        public int Value;

        public Priority(int value)
        {
            this.Value = value;
        }
    }
}
