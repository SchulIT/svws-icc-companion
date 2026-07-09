using System.Windows;
using System.Windows.Controls;

namespace SvwsIccImporter.View
{
    /// <summary>
    /// Interaktionslogik für ImportTask.xaml
    /// </summary>
    public partial class ImportTask : UserControl
    {
        public static readonly DependencyProperty CheckboxLabelProperty = DependencyProperty.Register("CheckboxLabel", typeof(string), typeof(ImportTask));

        public string CheckboxLabel
        {
            get { return (string)GetValue(CheckboxLabelProperty); }
            set { SetValue(CheckboxLabelProperty, value); }
        }

        public ImportTask()
        {
            InitializeComponent();
        }
    }
}
