using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SvwsIccImporter.View
{
    public partial class ImportTask : UserControl
    {
        public static readonly StyledProperty<string> CheckboxLabelProperty = AvaloniaProperty.Register<ImportTask, string>(nameof(CheckboxLabel));

        public string CheckboxLabel
        {
            get { return (string)GetValue(CheckboxLabelProperty); }
            set { SetValue(CheckboxLabelProperty, value); }
        }

        public static readonly StyledProperty<bool> IsImportEnabledProperty = AvaloniaProperty.Register<ImportTask, bool>(nameof(IsImportEnabled));

        public bool IsImportEnabled
        {
            get { return GetValue(IsImportEnabledProperty); }
            set { SetValue(IsImportEnabledProperty, value); }
        }

        public static readonly StyledProperty<string?> BusyTextProperty = AvaloniaProperty.Register<ImportTask, string?>(nameof(BusyText));

        public string? BusyText
        {
            get { return GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        public ImportTask()
        {
            InitializeComponent();
        }
    }
}