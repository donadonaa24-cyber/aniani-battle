using System;
using System.Linq;
using AniBato.Game;
using UnityEditor;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    // Display-only sharing: the second attack retains its own timings and physics.
    public static class PaperNormalFourKeyBuilder
    {
        public static void Apply(CharacterData[] roster)
        {
            foreach(var data in roster)
            {
                var paper=data.PaperMotion;if(!paper)continue;
                var first=paper.Find(AnimationState.Attack1);if(first==null)continue;
                var second=paper.Find(AnimationState.Attack2);if(second==null)continue;
                if(ReferenceEquals(first,second))continue; // Explicit standing placeholder before generation.
                second.Frames=first.Frames;second.HoldTicks=first.HoldTicks;
                second.FrameHeadHeights=first.FrameHeadHeights;second.FrameFootBaseYs=first.FrameFootBaseYs;
                second.FrameFootMinXs=first.FrameFootMinXs;second.FrameFootMaxXs=first.FrameFootMaxXs;
                second.FrameOriginYs=first.FrameOriginYs;second.HeadHeight=first.HeadHeight;
                second.FootBaseY=first.FootBaseY;second.FootMinX=first.FootMinX;second.FootMaxX=first.FootMaxX;
                second.Airborne=first.Airborne;second.ReverseFacing=first.ReverseFacing;second.TiltDegrees=first.TiltDegrees;
                EditorUtility.SetDirty(paper);
            }
        }
    }
}
