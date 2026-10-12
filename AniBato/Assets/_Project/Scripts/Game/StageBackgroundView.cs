using UnityEngine;
using System.Collections.Generic;

namespace AniBato.Game
{
    // Original placeholder scenery made entirely from our own shapes, with two parallax layers.
    [DefaultExecutionOrder(60)]
    public sealed class StageBackgroundView : MonoBehaviour
    {
        private Transform far, mid;
        private StageData data;
        private Camera cameraComponent;
        private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        private readonly List<Color> colors = new List<Color>();
        private int dimStep = -1;
        private readonly List<SpriteRenderer> boardCells = new List<SpriteRenderer>();
        private SpriteRenderer artwork, leftClosed, rightClosed, warningPanel;
        public void Build(StageData stage)
        {
            data = stage; cameraComponent = Camera.main;
            far = new GameObject("Far Scenery").transform; far.SetParent(transform, false);
            mid = new GameObject("Mid Scenery").transform; mid.SetParent(transform, false);
            if (cameraComponent) cameraComponent.backgroundColor = data.Background;
            if (data.FarArtwork)
            {
                var go = new GameObject("Far Painted Scenery"); go.transform.SetParent(far, false);
                artwork = go.AddComponent<SpriteRenderer>(); artwork.sprite = data.FarArtwork;
                artwork.sortingOrder = -30; artwork.color = new Color(.84f,.84f,.84f,1);
                go.transform.localPosition = new Vector3(0, 6, 0);
                go.transform.localScale = new Vector3(80 / data.FarArtwork.bounds.size.x, 45 / data.FarArtwork.bounds.size.y, 1);
                Details();
            }
            else if (data.Theme == StageTheme.FutureCity) City();
            else if (data.Theme == StageTheme.BoardArena) Board();
            else Festival();
        }
        private void Shape(Transform parent, string name, float x, float y, float w, float h, Color color, bool round = false)
        {
            var sprite = ProceduralSprites.Create(parent, name, new Vector2(w, h), color, round, parent == far ? -30 : -20);
            sprite.transform.localPosition = new Vector3(x, y, 0);
            sprites.Add(sprite); colors.Add(color);
        }
        public void SetShrinkStep(int step, int total, bool warning = false, int tick = 0, float remaining = 1)
        {
            if (warningPanel) { warningPanel.enabled = warning; warningPanel.color = new Color(data.Accent.r, data.Accent.g, data.Accent.b, tick / 12 % 2 == 0 ? .65f : .25f); }
            if (leftClosed)
            {
                float removed = 18 * (1 - remaining);
                leftClosed.enabled = rightClosed.enabled = removed > .01f;
                leftClosed.transform.localPosition = new Vector3(-18 + removed * .5f, 1, 0);
                rightClosed.transform.localPosition = new Vector3(18 - removed * .5f, 1, 0);
                leftClosed.transform.localScale = rightClosed.transform.localScale = new Vector3(removed, 9, 1);
                for (int i = 0; i < boardCells.Count; i++)
                {
                    int col = i % 6;
                    float left = Mathf.Max(-18 * remaining, -18 + col * 6), right = Mathf.Min(18 * remaining, -12 + col * 6);
                    boardCells[i].enabled = right > left;
                    var p = boardCells[i].transform.localPosition; p.x = (left + right) * .5f;
                    boardCells[i].transform.localPosition = p;
                    boardCells[i].transform.localScale = new Vector3(Mathf.Max(.001f, right - left - .06f), .72f, 1);
                }
            }
            if (data.Theme != StageTheme.SchoolFestival || step == dimStep) return;
            dimStep = step;
            float brightness = 1 - step / (float)Mathf.Max(1, total) * 0.45f;
            for (int i = 0; i < sprites.Count; i++)
            { var c = colors[i]; sprites[i].color = new Color(c.r * brightness, c.g * brightness, c.b * brightness, c.a); }
            if (artwork) artwork.color = Color.white * (1 - step / (float)Mathf.Max(1,total) * .25f);
        }
        private void Details()
        {
            // Midground details and foreground floor are separate from the painted far layer.
            if (data.Theme == StageTheme.BoardArena)
            {
                for (int row = 0; row < 6; row++) for (int col = 0; col < 6; col++)
                {
                    var tile = ProceduralSprites.Create(mid, "Board Backdrop Cell " + row + "-" + col, new Vector2(5.94f,.72f),
                        (row + col) % 2 == 0 ? new Color(.72f,.66f,.52f,.32f) : new Color(.25f,.24f,.23f,.32f), sortingOrder: -19);
                    tile.transform.localPosition = new Vector3(-15 + col * 6, -1.4f + row * .78f, 0); boardCells.Add(tile);
                }
            }
            else if (data.Theme == StageTheme.FutureCity)
            {
                for (int i = -4; i <= 4; i++)
                { Shape(mid,"Lit Data Conduit",i * 7,-1.7f,5,.06f,data.Accent); Shape(mid,"Deck Support",i * 7,-3.8f,.3f,4,new Color(.18f,.24f,.3f)); }
            }
            else
            {
                for (int i = -3; i <= 3; i++)
                { Shape(mid,"Stall Light",i * 9,7,.5f,.13f,new Color(.9f,.72f,.42f)); Shape(mid,"Counter Trim",i * 9,-1.5f,7,.1f,data.Accent); }
            }
            leftClosed = ProceduralSprites.Create(mid,"Closed Left Scenery",Vector2.one,new Color(.06f,.08f,.11f,.68f),sortingOrder:-18);
            rightClosed = ProceduralSprites.Create(mid,"Closed Right Scenery",Vector2.one,new Color(.06f,.08f,.11f,.68f),sortingOrder:-18);
            leftClosed.enabled = rightClosed.enabled = false;
            warningPanel = ProceduralSprites.Create(mid, data.Theme == StageTheme.FutureCity ? "AI Restriction Signal" : data.Theme == StageTheme.BoardArena ? "Outer Cells Warning" : "Festival Closing Signal",new Vector2(28,.12f),data.Accent,sortingOrder:-17);
            warningPanel.transform.localPosition = new Vector3(0,6.6f,0); warningPanel.enabled = false;
        }
        private void City()
        {
            for (int i = -8; i <= 8; i++)
            {
                float x = i * 5, h = 5 + (i * i % 5);
                Shape(far, "Skyline", x, h * 0.5f + 2, 3.7f, h, data.FarColor);
                for (int j = 0; j < 3; j++) Shape(far, "Window Band", x, 3 + j * 2, 2.6f, 0.08f, data.MidColor);
            }
            Shape(far, "Launch Tower", 13, 10, 1.2f, 16, data.MidColor);
            Shape(far, "Tower Halo", 13, 14, 5, 0.2f, data.Accent * 0.6f);
            for (int i = -5; i <= 5; i++) Shape(mid, "Data Conduit", i * 8, 1.3f, 6, 0.08f, data.MidColor);
        }
        private void Board()
        {
            for (int level = 0; level < 5; level++)
            {
                Shape(far, "Tier Tower", -20, 2 + level * 1.4f, 8 - level, 1, data.FarColor);
                Shape(far, "Tier Tower", 20, 2 + level * 1.4f, 8 - level, 1, data.FarColor);
            }
            for (int i = 0; i < 6; i++) for (int j = 0; j < 6; j++)
                Shape(mid, "6x6 Arena Tile", -15 + i * 6, -4 + j * 1.7f, 5.8f, 1.55f,
                    (i + j) % 2 == 0 ? data.FarColor : data.MidColor);
            foreach (float x in new[] { -24f, 24f })
            {
                Shape(mid, "Giant Piece Base", x, 2, 4, 1, data.MidColor);
                Shape(mid, "Giant Piece Body", x, 4, 2.8f, 4, data.MidColor);
                Shape(mid, "Giant Piece Crown", x, 6.5f, 3.6f, 1.6f, data.MidColor, true);
            }
        }
        private void Festival()
        {
            for (int i = -4; i <= 4; i++)
            {
                float x = i * 9;
                Shape(far, "Kitchen Stall", x, 3, 7.5f, 4, data.FarColor);
                Shape(mid, "Stall Awning", x, 5.5f, 8, 0.7f, data.MidColor);
                Shape(mid, "Kitchen Counter", x, 1.5f, 8, 0.8f, data.MidColor);
                Shape(mid, "Cooking Pot", x - 2, 2.4f, 1.4f, 1, data.MidColor, true);
                Shape(mid, "Plate", x + 2, 2.3f, 1.8f, 0.2f, data.Accent * 0.6f);
                Shape(far, "Decoration String", x, 9, 9, 0.08f, data.MidColor);
                for (int flag = 0; flag < 4; flag++) Shape(far, "Festival Flag", x - 3 + flag * 2, 8.5f, 0.7f, 0.8f, data.MidColor);
            }
        }
        private void LateUpdate()
        {
            if (!cameraComponent || !data) return;
            var p = cameraComponent.transform.position;
            far.localPosition = new Vector3(p.x * data.FarParallax, p.y * data.FarParallax, 0);
            mid.localPosition = data.Theme == StageTheme.BoardArena ? Vector3.zero : new Vector3(p.x * data.MidParallax, p.y * data.MidParallax, 0);
            if (artwork)
            {
                // Keep the painted skyline legible at every zoom/aspect; fixed huge
                // world-sized artwork would show only floor when the camera moves in.
                float height = cameraComponent.orthographicSize * 2 * 1.05f;
                artwork.transform.localPosition = new Vector3(p.x * (1 - data.FarParallax), p.y * (1 - data.FarParallax), 0);
                artwork.transform.localScale = new Vector3(height * cameraComponent.aspect / artwork.sprite.bounds.size.x,
                    height / artwork.sprite.bounds.size.y, 1);
            }
        }
    }
}
