using System;
using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.Messages
{
    public class NavigationMessage : MessageBase
    {
        public Type DataContextType { get; private set; }
        public NavigationMessage(Type viewModel)
        {
            DataContextType = viewModel;
        }
    }
}
