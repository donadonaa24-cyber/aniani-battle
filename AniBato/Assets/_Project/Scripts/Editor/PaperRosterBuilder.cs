using System;
using System.Linq;
using AniBato.Game;
using AniBato.Presentation;
using UnityEditor;
using UnityEngine;
using AnimationState = AniBato.Presentation.AnimationState;

namespace AniBato.EditorTools
{
    public static class PaperRosterBuilder
    {
        public static CharacterData[] Build(CharacterData[] original)
        {
            var pictures=GameFlowBuilder.ImportPaperRoster();
            string[] keys={"Minato","Shiro","Yui","King","Akatsuki","Ryuta"};
            string[] names={"湊","シロ","結衣","黒獅子王","暁","龍太"};
            int[] source={0,0,1,1,2,2},picture={1,2,4,5,7,8};
            var roster=new CharacterData[9];Array.Copy(original,roster,3);
            original[0].SelectionSprite=pictures[0];original[1].SelectionSprite=pictures[3];original[2].SelectionSprite=pictures[6];
            for(int i=0;i<keys.Length;i++)
            {
                string path="Assets/_Project/Data/"+keys[i]+".asset";
                var data=AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if(!data){data=ScriptableObject.CreateInstance<CharacterData>();EditorUtility.CopySerialized(original[source[i]],data);AssetDatabase.CreateAsset(data,path);}
                var movement=data.Movement;movement.Name=names[i];data.Movement=movement;
                data.SelectionSprite=pictures[picture[i]];data.Portrait=data.SelectionSprite;data.PrototypeSharedMoves=true;
                string paperPath="Assets/_Project/Data/"+keys[i]+"PaperMotion.asset";
                var paper=AssetDatabase.LoadAssetAtPath<PaperCharacterMotion>(paperPath);
                if(!paper){paper=ScriptableObject.CreateInstance<PaperCharacterMotion>();AssetDatabase.CreateAsset(paper,paperPath);}
                if(paper.SchemaVersion<1)
                {
                    paper.Sprite=data.SelectionSprite;paper.Enabled=true;paper.SchemaVersion=1;
                    paper.Motion=original[2].PaperMotion.Motion;paper.StandingHeight=2.35f;
                    paper.Poses=new[]{new PaperPose{Key="prototype_whole_illustration",States=(AnimationState[])Enum.GetValues(typeof(AnimationState)),
                        Frames=Enumerable.Repeat(paper.Sprite,4).ToArray(),HoldTicks=8,HeadHeight=2.35f,FootMinX=-.38f,FootMaxX=.38f}};
                    EditorUtility.SetDirty(paper);
                }
                data.PaperMotion=paper;roster[i+3]=data;EditorUtility.SetDirty(data);
            }
            foreach(var data in original)EditorUtility.SetDirty(data);
            return roster;
        }
    }
}

