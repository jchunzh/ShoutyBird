using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.ViewModel
{
    public class RemovePipeMessage : MessageBase
    {
        public BaseUnitViewModel Pipe { get; private set; }

        public RemovePipeMessage(PipeViewModel pipe)
        {
            Pipe = pipe;
        }
    }
}
