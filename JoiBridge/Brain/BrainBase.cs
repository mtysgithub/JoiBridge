﻿using JoiBridge.Speak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoiBridge.Brain
{
    internal class BrainBase
    {
        public SpeakerBase Speaker = null;

        public virtual async Task Build(SpeakerBase InSpeaker)
        {
            Speaker = InSpeaker;
        }

        public virtual void OutputHistoricalMessages()
        {

        }

        public virtual void SetMask(string MaskFileName)
        {

        }

        public virtual bool ParseGM(string HumanInputString)
        {
            return false;
        }
    }
}
