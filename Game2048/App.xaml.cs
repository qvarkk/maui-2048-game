using Microsoft.Maui.Platform;

namespace Game2048
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int width = 516;
            const int height = 840;
            
            window.Width = width;
            window.Height = height;

            return window;
        }
    }
}
