﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;

namespace ProgramonSoundEngine
{
    public class SoundPlayer
    {
        public void PlayStaticSound(SoundEffect sound)
        {
            SoundEffectInstance instSound = sound.CreateInstance();

            instSound.Play();
        }

        public void PlayStaticSound(SoundEffect sound, float volume, float pitch, float pan)
        {
            SoundEffectInstance instSound = sound.CreateInstance();

            instSound.Volume = volume;
            instSound.Pitch = pitch;
            instSound.Pan = pan;

            instSound.Play();
        }
    }
}
