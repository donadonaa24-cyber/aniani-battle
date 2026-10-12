using System;
using UnityEngine;

namespace AniBato.Game
{
    [CreateAssetMenu(menuName = "あにバト/対戦起動設定")]
    public sealed class BattleSetup : ScriptableObject
    {
        public CharacterData[] Characters;
        public StageData[] Stages;
        public CpuDifficultyData[] CpuDifficulties;
        public uint CpuSeed = 20261012;
        public int Player1Index;
        public int Player2Index = 1;
        public CharacterData Character(int index)
        {
            if (Characters == null || index < 0 || index >= Characters.Length || !Characters[index])
                throw new ArgumentException("対戦起動設定のキャラクターが不正です。");
            return Characters[index];
        }
    }
}
