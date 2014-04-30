using System;
using System.Security.Cryptography;
using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.Message
{
    public class NavigationMessage : MessageBase
    {
        public Uri DestinationUri { get; private set; }

        public NavigationMessage(Uri destinationUri)
        {
            DestinationUri = destinationUri;
        }
    }
}
