using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    public sealed class StageView : MonoBehaviour
    {
        [SerializeField] private StageData stage;
        [SerializeField] private PresentationSettings presentation;
        [SerializeField] private BattleRunner runner;
        private SpriteRenderer[] floors, edges;
        private SpriteRenderer leftWarning, rightWarning;
        private SpriteRenderer[] boardTiles;
        private BattleRect originalMain;
        private StageBackgroundView backgroundView;
        public void Configure(StageData data, PresentationSettings settings, BattleRunner battleRunner = null)
        { stage = data; presentation = settings; runner = battleRunner; }
        private void Start() => Rebuild(stage);
        public void Rebuild(StageData data)
        {
            stage = data;
            foreach (Transform child in transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var background = new GameObject("Stage Background"); background.transform.SetParent(transform, false);
            backgroundView = background.AddComponent<StageBackgroundView>(); backgroundView.Build(stage);
            StageGeometry geometry = stage.ToGeometry();
            originalMain = geometry[0].Bounds;
            floors = new SpriteRenderer[geometry.PlatformCount]; edges = new SpriteRenderer[geometry.PlatformCount];
            for (int i = 0; i < geometry.PlatformCount; i++)
            {
                StagePlatform platform = geometry[i];
                var visual = ProceduralSprites.Create(transform, "Platform " + i,
                    new Vector2(platform.Bounds.Size.X, platform.Bounds.Size.Y), platform.OneWay ? stage.Accent : stage.FloorColor);
                floors[i] = visual;
                visual.transform.localPosition = new Vector3(platform.Bounds.Center.X, platform.Bounds.Center.Y, 0);
                var edge = ProceduralSprites.Create(transform, "Platform Edge " + i, new Vector2(platform.Bounds.Size.X, 0.06f), stage.Accent, sortingOrder: 1);
                edges[i] = edge;
                edge.transform.localPosition = new Vector3(platform.Bounds.Center.X, platform.Bounds.Top, 0);
            }
            AddBlastLine("Left Boundary", new Vector2(geometry.LeftBlastLine, -1), new Vector2(0.08f, 20));
            AddBlastLine("Right Boundary", new Vector2(geometry.RightBlastLine, -1), new Vector2(0.08f, 20));
            AddBlastLine("Bottom Boundary", new Vector2(0, geometry.BottomBlastLine), new Vector2(geometry.RightBlastLine - geometry.LeftBlastLine, 0.08f));
            leftWarning = ProceduralSprites.Create(transform, "Left Disappearing Area", Vector2.one, presentation.Warning, sortingOrder: 2);
            rightWarning = ProceduralSprites.Create(transform, "Right Disappearing Area", Vector2.one, presentation.Warning, sortingOrder: 2);
            leftWarning.enabled = rightWarning.enabled = false;
            boardTiles = stage.Theme == StageTheme.BoardArena ? new SpriteRenderer[6] : null;
            if (boardTiles != null)
                for (int i = 0; i < boardTiles.Length; i++)
                    boardTiles[i] = ProceduralSprites.Create(transform, "Board Floor Cell " + i, Vector2.one,
                        i % 2 == 0 ? stage.FloorColor : stage.Accent * 0.7f, sortingOrder: 1);
        }
        private void LateUpdate()
        {
            if (!runner || runner.Simulation == null || floors == null) return;
            var sim = runner.Simulation;
            bool flash = (sim.Match.TickCount / 6) % 2 == 0;
            for (int i = 0; i < floors.Length; i++)
            {
                var platform = sim.Stage[i];
                floors[i].enabled = edges[i].enabled = sim.Stage.IsActive(i);
                floors[i].transform.localScale = new Vector3(platform.Bounds.Size.X, platform.Bounds.Size.Y, 1);
                edges[i].transform.localScale = new Vector3(platform.Bounds.Size.X, 0.06f, 1);
                bool warn = i > 0 && sim.Shrink.UpperPlatformsWarned && flash;
                floors[i].color = warn ? presentation.Warning : platform.OneWay ? stage.Accent : stage.FloorColor;
            }
            bool pending = sim.Shrink.PendingStep > 0;
            var current = sim.Stage[0].Bounds; var next = sim.Shrink.NextMainBounds;
            backgroundView.SetShrinkStep(sim.Shrink.CompletedSteps, sim.Rules.Shrink.Steps,
                sim.Shrink.WarningIssued, sim.Match.TickCount, current.Size.X / originalMain.Size.X);
            if (boardTiles != null)
                for (int i = 0; i < boardTiles.Length; i++)
                {
                    float tileWidth = originalMain.Size.X / boardTiles.Length;
                    float left = Mathf.Max(current.Left, originalMain.Left + i * tileWidth);
                    float right = Mathf.Min(current.Right, originalMain.Left + (i + 1) * tileWidth);
                    boardTiles[i].enabled = right > left;
                    boardTiles[i].transform.localPosition = new Vector3((left + right) * 0.5f, current.Center.Y, 0);
                    boardTiles[i].transform.localScale = new Vector3(Mathf.Max(0.001f, right - left - 0.04f), current.Size.Y * 0.8f, 1);
                }
            Area(leftWarning, current.Left, next.Left, current, pending && flash);
            Area(rightWarning, next.Right, current.Right, current, pending && flash);
        }
        private void Area(SpriteRenderer sprite, float left, float right, BattleRect floor, bool visible)
        {
            sprite.enabled = visible && right > left;
            Color warning = stage.Theme == StageTheme.FutureCity ? new Color(0.2f, 0.8f, 1) : presentation.Warning;
            sprite.color = new Color(warning.r, warning.g, warning.b, 0.7f);
            sprite.transform.localPosition = new Vector3((left + right) * 0.5f, floor.Center.Y, 0);
            sprite.transform.localScale = new Vector3(Mathf.Max(0.001f, right - left), floor.Size.Y + 0.08f, 1);
        }
        private void AddBlastLine(string name, Vector2 position, Vector2 size)
        {
            var visual = ProceduralSprites.Create(transform, name, size, new Color(0.8f, 0.15f, 0.2f, 0.6f));
            visual.transform.localPosition = position;
        }
    }
}
