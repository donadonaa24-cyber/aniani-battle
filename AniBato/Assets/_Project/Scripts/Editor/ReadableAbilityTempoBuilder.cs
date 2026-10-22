using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    public static class ReadableAbilityTempoBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            for (int i = 0; i < roster.Length; i++)
            {
                var data = roster[i];
                if (data.AbilityTempoSchemaVersion < 1)
                {
                    var up = data.Specials[(int)SpecialSlot.Up]; var recovery = up.Definition;
                    if (!recovery.Recovery.ChargedTeleport)
                    {
                        recovery.StartupFrames = (int)System.Math.Ceiling(recovery.StartupFrames * 1.5);
                        recovery.ActiveFrames = (int)System.Math.Ceiling(recovery.ActiveFrames * 1.5);
                        recovery.Recovery.MotionTimeScale = 2f / 3;
                        recovery.Recovery.BurstFrames = (int)System.Math.Ceiling(recovery.Recovery.BurstFrames * 1.5);
                        recovery.Recovery.SecondStartFrame = (int)System.Math.Ceiling(recovery.Recovery.SecondStartFrame * 1.5);
                    }
                    recovery.RecoveryFrames = (int)System.Math.Ceiling(recovery.RecoveryFrames * 1.5);
                    recovery.Validate(); up.Definition = recovery; EditorUtility.SetDirty(up);
                    var special = data.Specials[0].Definition; var ultimate = data.Ultimate.Definition;
                    int[] startup = {10,10,10,10,12,10,14,8,18}; special.StartupFrames = startup[i];
                    if (i == 8)
                    {
                        special.AbilityCharge = Charge(18,60,1,3.5f); ultimate.StartupFrames = 60;
                    }
                    else
                    {
                        int minimum = i == 1 || i == 5 ? 24 : 30;
                        int maximum = i == 0 || i == 3 || i == 4 || i == 7 ? 120 : 90;
                        ultimate.StartupFrames = minimum; ultimate.AbilityCharge = Charge(minimum,maximum,i == 3 ? 2 : 2.5f);
                        if (i == 4)
                        { ultimate.AbilityCharge.MaximumStageWidthFraction = 2f / 3; ultimate.AbilityCharge.WidthOnly = true; ultimate.DamageDelayFrames = 0; }
                        if (i == 2 || i == 7) ultimate.AbilityCharge.WidthOnly = true;
                        if (i == 6)
                        { ultimate.AbilityCharge.FormationOnly = true; ultimate.ProjectileBehavior.FormationVerticalSpread = 1.8f; }
                        if (i == 7) ultimate.ProjectileBehavior.MainPlatformOnly = true;
                        if (i == 0)
                        {
                            ultimate.ProjectileBehavior.ExplodeOnLanding = true;
                            ultimate.ProjectileBehavior.ExplosionSize = new BattleVector(6,3.5f);
                            ultimate.ProjectileBehavior.ExplosionFrames = 18;
                        }
                    }
                    special.Validate(); ultimate.Validate(); data.Specials[0].Definition = special; data.Ultimate.Definition = ultimate;
                    EditorUtility.SetDirty(data.Specials[0]); EditorUtility.SetDirty(data.Ultimate);
                    data.AbilityTempoSchemaVersion = 1; EditorUtility.SetDirty(data);
                }
                // Explicit landing rule also applies to already migrated assets without resetting their tuning.
                if (i == 0 && !data.Ultimate.Definition.ProjectileBehavior.MainPlatformOnly)
                {
                    var rocket=data.Ultimate.Definition; rocket.ProjectileBehavior.MainPlatformOnly=true;
                    data.Ultimate.Definition=rocket; EditorUtility.SetDirty(data.Ultimate);
                }
                // Art builders import fresh manifests every BuildAll. Reconnect this game's magic overlay afterward.
                if (i == 5 && data.PaperMotion)
                {
                    var paper = data.PaperMotion; var magic = paper.AbilityEffect(AnimationState.Attack3);
                    if (magic != null)
                    {
                        paper.AbilityEffects = paper.AbilityEffects.Where(a => a.State != AnimationState.Ultimate).Append(new PaperAbilityArt {
                            State = AnimationState.Ultimate, Look = AbilityLook.Default, Sprite = magic.Sprite, Scale = magic.Scale,
                            Offset = magic.Offset, VisibleSize = magic.VisibleSize, RockDegrees = 0 }).ToArray();
                        EditorUtility.SetDirty(paper);
                    }
                }
            }
        }
        private static AbilityChargeDefinition Charge(int minimum,int maximum,float scale,float knockback=1) =>
            new AbilityChargeDefinition { MinimumFrames=minimum,MaximumFrames=maximum,MaximumRangeScale=scale,MaximumKnockbackScale=knockback };
    }
}
