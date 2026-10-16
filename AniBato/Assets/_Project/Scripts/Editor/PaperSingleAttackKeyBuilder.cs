using System.Linq;
using AniBato.Game;
using UnityEditor;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    // Presentation policy only: startup, active frames, recovery and gravity remain in Battle.
    public static class PaperSingleAttackKeyBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            foreach (var data in roster)
            {
                var paper = data.PaperMotion;
                if (!paper) continue;
                foreach (var state in new[] { AnimationState.CrouchAttack, AnimationState.AirNeutral })
                {
                    var source = paper.Find(state);
                    if (source == null) continue;
                    // A placeholder may be shared with idle and core poses. Separate the state first.
                    int index = source.Frames.Length - 1;
                    var states = source.States.Where(s => s == state ||
                        state == AnimationState.AirNeutral && s == AnimationState.AirAttack).ToArray();
                    var pose = new PaperPose {
                        Key = source.States.Length == states.Length ? source.Key : source.Key + "_" + state,
                        States = states, Frames = new[] { source.Frames[index] }, HoldTicks = source.HoldTicks,
                        Airborne = source.Airborne, ReverseFacing = source.ReverseFacing, TiltDegrees = source.TiltDegrees,
                        SwitchOnProjectileRelease = source.SwitchOnProjectileRelease,
                        FrameHeadHeights = new[] { Pick(source.FrameHeadHeights, source.HeadHeight, index) },
                        FrameFootBaseYs = new[] { Pick(source.FrameFootBaseYs, source.FootBaseY, index) },
                        FrameFootMinXs = new[] { Pick(source.FrameFootMinXs, source.FootMinX, index) },
                        FrameFootMaxXs = new[] { Pick(source.FrameFootMaxXs, source.FootMaxX, index) },
                        FrameOriginYs = new[] { Pick(source.FrameOriginYs, 0, index) }
                    };
                    pose.HeadHeight = pose.FrameHeadHeights[0]; pose.FootBaseY = pose.FrameFootBaseYs[0];
                    pose.FootMinX = pose.FrameFootMinXs[0]; pose.FootMaxX = pose.FrameFootMaxXs[0];
                    source.States = source.States.Where(s => !states.Contains(s)).ToArray();
                    paper.Poses = paper.Poses.Where(p => p.States.Length > 0).Concat(new[] { pose }).ToArray();
                }
                EditorUtility.SetDirty(paper);
            }
        }
        private static float Pick(float[] values, float fallback, int index) =>
            values != null && values.Length > index ? values[index] : fallback;
    }
}
