using System;

namespace AniBato.Presentation
{
    public static class PracticePlayback
    {
        public static double Scale(double seconds,int index,bool paused,float[] rates)
        {
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
            if(rates==null||index<0||index>=rates.Length||rates[index]<=0)throw new ArgumentOutOfRangeException(nameof(index));
            return paused?0:seconds*rates[index];
        }
    }
}
