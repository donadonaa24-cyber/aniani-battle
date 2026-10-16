using System.IO;
using System.Linq;
using AniBato.EditorTools;
using AniBato.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.Tests
{
    public sealed class ChizuruMissingArtworkTests
    {
        [Test]
        public void ChizuruUsesTwelveKeysAndKeepsRiceEmissionAndAirborneProfile()
        {
            var data=AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Chizuru.asset");var paper=data.PaperMotion;
            var states=new[]{AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
            Assert.That(states.SelectMany(state=>paper.Find(state).Frames).Distinct().Count(),Is.EqualTo(12));
            foreach(var state in states)Assert.That(paper.Find(state).Frames.Length,
                Is.EqualTo(state==AnimationState.CrouchAttack||state==AnimationState.AirNeutral?1:2));
            Assert.That(paper.Find(AnimationState.Attack2).Frames,Is.EqualTo(paper.Find(AnimationState.Attack1).Frames));
            Assert.That(paper.Find(AnimationState.SpecialNeutral).SwitchOnProjectileRelease,Is.True);
            Assert.That(paper.Find(AnimationState.AirNeutral).Airborne,Is.True);
            foreach(var state in new[]{AnimationState.AirUp,AnimationState.CrouchAttack,AnimationState.Ultimate})
            {Assert.That(paper.Find(state).Airborne,Is.False);Assert.That(paper.Find(state).TiltDegrees,Is.Zero);}
            Assert.That(paper.UltimateImpactMinimumTicks,Is.EqualTo(96));Assert.That(paper.CurryAfterglowTicks,Is.EqualTo(24));
            var manifest=JsonUtility.FromJson<PaperActionBuilder.Manifest>(File.ReadAllText(PaperActionBuilder.ManifestPath));
            foreach(string key in new[]{"air_up","crouch_attack","air_neutral","ultimate"})
            {
                var row=manifest.poses.Single(p=>p.key==key);var pose=paper.Find((AnimationState)row.states.Last());
                for(int i=0;i<pose.Frames.Length;i++)
                {
                    var frame=row.frames[pose.Frames.Length==1?row.frames.Length-1:i];
                    if(frame.source.Contains("/prepared/"))
                        Assert.That(File.ReadAllBytes(AssetDatabase.GetAssetPath(pose.Frames[i])),Is.EqualTo(File.ReadAllBytes("../"+frame.source)),key);
                    Assert.That(pose.Frames[i].pixelsPerUnit,Is.EqualTo(532.5893f).Within(.001));
                }
            }
        }
    }
}
