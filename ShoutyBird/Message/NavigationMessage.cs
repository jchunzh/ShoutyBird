using System;
using System.Security.Cryptography;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.Message
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
