using SvwsIccImporter.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace SvwsIccImporter.View
{
    public partial class SplashScreen : Window
    {
        public SplashScreenViewModel ViewModel { get; }

        public SplashScreen(SplashScreenViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
