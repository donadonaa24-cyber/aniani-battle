using System;
using System.IO;
using System.Linq;
using AniBato.Battle;
using AniBato.Game;
using UnityEditor;
using UnityEngine;

namespace AniBato.EditorTools
{
    public static class PaperLedgeBuilder
    {
        [Serializable] private sealed class Manifest { public int schemaVersion; public bool complete,userShapeConfirmed; public string handPattern,thumbSide; public Frame[] frames; }
        [Serializable] private sealed class Frame { public int index,handX,handY,grippingHands; public string key,prepared,sha256,pose,grippingHand; }
        public static PaperPose HangingPose(int index)
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("../art_source/paper_ledge/adoption.json"));
            var frame=manifest.frames.Single(f=>f.index==index);
            if(manifest.schemaVersion!=3 || !manifest.userShapeConfirmed || manifest.handPattern!="A" || manifest.thumbSide!="image-left" || frame.grippingHands!=1 || frame.pose!="one_hand_hang" || frame.grippingHand!="left")
                throw new InvalidOperationException("User-confirmed A ledge hand pattern required.");
            var sprite=PaperActionArtworkBuilder.Import(frame.key+"_ledge_hang",frame.prepared,frame.sha256,
                1304,1450,new Vector2(frame.handX/1304f,1-frame.handY/1450f));
            return new PaperPose { Key="ledge",States=new[]{AniBato.Presentation.AnimationState.LedgeHang},
                Frames=new[]{sprite},HoldTicks=8,Airborne=true };
        }
        public static void Apply(CharacterData[] roster)
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("../art_source/paper_ledge/adoption.json"));
            if(manifest.schemaVersion!=3 || !manifest.complete || !manifest.userShapeConfirmed || manifest.handPattern!="A" || manifest.thumbSide!="image-left" || manifest.frames.Length!=9) throw new InvalidOperationException("Missing user-confirmed A ledge artwork.");
            foreach(var frame in manifest.frames)
            {
                var data=roster[frame.index];
                var hanging=HangingPose(frame.index);data.PaperMotion.LedgeHang=hanging.Frames[0];
                var poses=data.PaperMotion.Poses.ToList();
                foreach(var pose in poses) pose.States=pose.States.Where(s=>s!=AniBato.Presentation.AnimationState.LedgeHang).ToArray();
                poses.RemoveAll(p=>p.States.Length==0);
                poses.Add(hanging);
                data.PaperMotion.Poses=poses.ToArray();
                EditorUtility.SetDirty(data.PaperMotion);
                if(data.LedgeSchemaVersion>=1) continue;
                foreach(var attack in data.GroundNormals.Concat(data.AirNormals).Concat(data.DirectionalNormals).Concat(data.Specials).Append(data.UpNormal).Append(data.Ultimate).Where(a=>a).Distinct())
                {
                    var d=attack.Definition;
                    d.CanHitLedge=d.NormalDirection==NormalDirection.Crouch || d.NormalDirection==NormalDirection.AirDown ||
                        d.NormalDirection==NormalDirection.AirForwardDown || d.Ability.Look==AbilityLook.Rocket || d.Ability.Look==AbilityLook.Pyramid;
                    attack.Definition=d; EditorUtility.SetDirty(attack);
                }
                data.LedgeSchemaVersion=1;EditorUtility.SetDirty(data);
            }
        }
    }
}
