using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AniBato.Game;
using UnityEditor;
using UnityEngine;
using AnimationState=AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    public static class PaperRosterActionArtworkBuilder
    {
        private static readonly string[] Keys={"toma","ren","chizuru","minato","shiro","yui","king","akatsuki","ryuta"};
        private static readonly string[] Names={"透真","蓮","千鶴","湊","シロ","結衣","黒獅子王","暁","龍太"};
        private static readonly string[] Actions={"normal1","normal3","up","down","air","special","super"};
        private static readonly AnimationState[] States={AnimationState.Attack1,AnimationState.Attack3,AnimationState.AirUp,
            AnimationState.CrouchAttack,AnimationState.AirNeutral,AnimationState.SpecialNeutral,AnimationState.Ultimate};
        public static void Apply(CharacterData[] roster)
        {
            for(int index=1;index<Keys.Length;index++)
            {
                string file="../art_source/paper_roster_action/"+Keys[index]+"/packed-manifest.json";
                if(!File.Exists(file))continue;
                var m=JsonUtility.FromJson<PaperActionArtworkBuilder.Manifest>(File.ReadAllText(file));
                if(m.schemaVersion!=1||m.key!=Keys[index]||m.frames==null)throw new InvalidOperationException("人物の攻撃原画指示が不正: "+Keys[index]);
                var data=roster[index];var paper=data.PaperMotion;
                if(!paper||data.Movement.Name!=Names[index])throw new InvalidOperationException("攻撃原画の人物が一致しません");
                var additions=new List<PaperPose>();
                var imported=new Dictionary<string,Sprite>();
                Sprite Import(PaperActionArtworkBuilder.Frame frame)
                {
                    if(imported.TryGetValue(frame.sha256,out var sprite))return sprite;
                    sprite=PaperActionArtworkBuilder.Import(Keys[index]+"_"+frame.id,frame.prepared,frame.sha256,1304,1450,new Vector2(.5f,122f/1450));
                    imported.Add(frame.sha256,sprite);return sprite;
                }
                for(int action=0;action<Actions.Length;action++)
                {
                    bool single=Actions[action]=="down"||Actions[action]=="air";
                    var frames=(single?new[]{"impact"}:new[]{"prepare","impact"}).Select(phase=>m.frames.SingleOrDefault(f=>f.id==Actions[action]+"_"+phase)).ToArray();
                    if(frames.All(f=>f==null)&&!m.complete)continue;
                    if(frames.Any(f=>f==null))throw new InvalidOperationException("必要な攻撃原画が不足: "+Actions[action]);
                    var pose=new PaperPose{Key=Actions[action],States=new[]{States[action]},HoldTicks=8,
                        RotateWithRush=Keys[index]=="yui"&&(action==5||action==6),
                        Frames=frames.Select(Import).ToArray(),
                        FrameHeadHeights=frames.Select(f=>(1328-f.headTopY)/PaperActionArtworkBuilder.PixelsPerUnit).ToArray(),
                        FrameFootMinXs=frames.Select(f=>(f.footMinX-652)/PaperActionArtworkBuilder.PixelsPerUnit).ToArray(),
                        FrameFootMaxXs=frames.Select(f=>(f.footMaxX-652)/PaperActionArtworkBuilder.PixelsPerUnit).ToArray(),
                        FrameOriginYs=frames.Select(f=>f.originOffsetY).ToArray()};
                    pose.HeadHeight=pose.FrameHeadHeights[0];pose.FootMinX=pose.FrameFootMinXs[0];pose.FootMaxX=pose.FrameFootMaxXs[0];
                    additions.Add(pose);
                    if(action==0)
                    {
                        additions.Add(Copy(pose,"normal2",AnimationState.Attack2,false));
                        additions.Add(Copy(pose,"back",AnimationState.BackAttack,true));
                    }
                }
                var replaced=new HashSet<AnimationState>(additions.SelectMany(p=>p.States));
                var poses=paper.Poses.ToList();
                foreach(var pose in poses)pose.States=pose.States.Where(s=>!replaced.Contains(s)).ToArray();
                poses.RemoveAll(p=>p.States.Length==0);poses.AddRange(additions);paper.Poses=poses.ToArray();
                data.ActionArtworkSchemaVersion=m.complete?1:data.ActionArtworkSchemaVersion;
                if(Keys[index]=="yui"&&m.complete)NameYuiMoves(data);
                PaperRosterEffectArtworkBuilder.Apply(Keys[index],data);
                data.ActionArtworkPending=true;EditorUtility.SetDirty(paper);EditorUtility.SetDirty(data);
            }
        }
        private static void NameYuiMoves(CharacterData data)
        {
            for(int i=0;i<data.GroundNormals.Length;i++)Name(data.GroundNormals[i],i<2?"ステッキ打撃 "+(i+1):"ステッキ魔法 3");
            Name(data.UpNormal,"ステッキ打ち上げ");
            Name(data.DirectionalNormals[(int)AniBato.Battle.NormalDirection.Crouch],"低いステッキ打撃");
            Name(data.DirectionalNormals[(int)AniBato.Battle.NormalDirection.Back],"背後へのステッキ打撃");
            Name(data.DirectionalNormals[(int)AniBato.Battle.NormalDirection.AirNeutral],"空中ステッキ打撃");
        }
        private static void Name(AttackData asset,string label)
        { var definition=asset.Definition;definition.Name=label;asset.Definition=definition;EditorUtility.SetDirty(asset); }
        private static PaperPose Copy(PaperPose from,string key,AnimationState state,bool reverse)=>new PaperPose{
            Key=key,States=new[]{state},Frames=from.Frames,HoldTicks=from.HoldTicks,ReverseFacing=reverse,
            FrameHeadHeights=from.FrameHeadHeights,FrameFootMinXs=from.FrameFootMinXs,FrameFootMaxXs=from.FrameFootMaxXs,
            FrameOriginYs=from.FrameOriginYs,HeadHeight=from.HeadHeight,FootMinX=from.FootMinX,FootMaxX=from.FootMaxX};
    }
}
