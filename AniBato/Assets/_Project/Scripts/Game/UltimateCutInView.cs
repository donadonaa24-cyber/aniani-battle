using AniBato.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace AniBato.Game
{
    public sealed class UltimateCutInView : MonoBehaviour
    {
        [SerializeField] private BattleRunner runner;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text[] labels;
        [SerializeField] private Image[] portraits;
        [SerializeField] private Image[] bands;
        private readonly float[] startedAt = new float[2];
        public void Configure(BattleRunner value, GameObject root, Text[] text, Image[] images, Image[] colors)
        { runner = value; panel = root; labels = text; portraits = images; bands = colors; }
        private void OnEnable() { if (runner) { runner.EventProduced += OnEvent; runner.MatchRestarted += ResetView; } }
        private void OnDisable() { if (runner) { runner.EventProduced -= OnEvent; runner.MatchRestarted -= ResetView; } }
        private void ResetView()
        {
            panel.SetActive(false);
            for (int i = 0; i < 2; i++) { labels[i].text = ""; portraits[i].enabled = false; bands[i].enabled = false; }
        }
        private void OnEvent(BattleEvent value)
        {
            if (value.Kind != BattleEventKind.UltimateStarted) return;
            int player = value.SourcePlayer;
            startedAt[player] = Time.unscaledTime;
            if (!panel.activeSelf) ResetView();
            var character = runner.Character(player);
            labels[player].text = character.Movement.Name + "\n" + character.Ultimate.Definition.Name;
            bands[player].color = character.Tint; bands[player].enabled = true;
            portraits[player].sprite = character.Portrait; portraits[player].enabled = character.Portrait;
            panel.SetActive(true);
        }
        private void LateUpdate()
        {
            if (runner.Simulation == null || runner.Simulation.CutInRemainingFrames == 0) { panel.SetActive(false); return; }
            for (int i = 0; i < 2; i++)
            {
                float entry = Mathf.Clamp01((Time.unscaledTime - startedAt[i]) / .12f);
                bands[i].rectTransform.localScale = new Vector3(entry, 1, 1);
                portraits[i].rectTransform.localScale = Vector3.one * (1.08f - .08f * entry);
                labels[i].rectTransform.localScale = Vector3.one * (.85f + .15f * entry);
            }
        }
    }
}
