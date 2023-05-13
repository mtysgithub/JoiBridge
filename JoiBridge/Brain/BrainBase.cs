using JoiBridge.Speak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoiBridge.Brain
{
    internal class BrainBase
    {
        protected SpeakerBase Speaker = null;

        public virtual async Task Build(SpeakerBase InSpeaker)
        {
            Speaker = InSpeaker;
        }
    }
}
