/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:ShoutyCopter"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace ShoutyBird.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MainMenuViewModel>();
            SimpleIoc.Default.Register<AudioViewModel>();
            SimpleIoc.Default.Register<GameViewModel>();
            SimpleIoc.Default.Register<VolumnViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public MainMenuViewModel MainMenu
        {
            get { return ServiceLocator.Current.GetInstance<MainMenuViewModel>(); }
        }

        public AudioViewModel Audio
        {
            get { return ServiceLocator.Current.GetInstance<AudioViewModel>(); }
        }

        public GameViewModel Game
        {
            get { return ServiceLocator.Current.GetInstance<GameViewModel>(); }
        }

        public VolumnViewModel Volumn
        {
            get { return ServiceLocator.Current.GetInstance<VolumnViewModel>(); }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}