namespace AniBato.Presentation
{
    // Display-only afterglow; repeated renders and pauses cannot advance this clock.
    public sealed class PaperUltimatePoseHold
    {
        private AnimationSample held;
        private int impactTick=-1, previousTick=-1, previousFrame=-1;
        public void Reset(){impactTick=previousTick=previousFrame=-1;}
        public AnimationSample Evaluate(in AnimationSample current,int tick,int minimumImpactTicks)
        {
            if(tick<previousTick)Reset();
            previousTick=tick;
            if(current.State==AnimationState.Ultimate && current.IsAttack)
            {
                if(current.AttackFrame<previousFrame)impactTick=-1;
                previousFrame=current.AttackFrame;
                if(current.AttackFrame>=current.Startup+current.DamageDelay)
                {
                    if(impactTick<0)impactTick=tick;
                    held=current;
                }
                return current;
            }
            // Movement, another attack, defense, charge and all damage immediately interrupt.
            bool rest=!current.IsAttack&&(current.State==AnimationState.Idle||current.State==AnimationState.Land);
            if(rest&&impactTick>=0&&tick-impactTick<minimumImpactTicks)
            {
                held.FlipX=current.FlipX;return held;
            }
            impactTick=previousFrame=-1;return current;
        }
    }
}
