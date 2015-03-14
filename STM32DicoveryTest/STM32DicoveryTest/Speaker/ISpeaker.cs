using System;
using Microsoft.SPOT;

namespace STM32DicoveryTest
{
    public interface ISpeaker
    {
        void Pause();
        void Play(double frequency);
    }

}
