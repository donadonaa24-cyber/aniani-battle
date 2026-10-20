using AniBato.Battle;
using UnityEngine;

namespace AniBato.Game
{
    // Rectangular fragments of the same full illustration, with its original texture/PPU.
    // These are visual debris, never articulated limbs or simulation colliders.
    public sealed class TeleportPhaseView
    {
        private const int Columns = 4, Rows = 6;
        private readonly SpriteRenderer[] fragments = new SpriteRenderer[Columns * Rows];
        private readonly SpriteRenderer[] glow = new SpriteRenderer[2], flash = new SpriteRenderer[3];
        private readonly PresentationSettings settings;
        private readonly Transform root;
        private Sprite source;
        private long arrivedId;
        private int arrivalTick = -1;
        public TeleportPhaseView(Transform parent, PresentationSettings settings)
        {
            this.settings = settings;
            root = new GameObject("Teleport Fragments").transform; root.SetParent(parent, false);
            for (int i = 0; i < fragments.Length; i++)
                fragments[i] = ProceduralSprites.Create(root, "Teleport Fragment " + i, Vector2.one, Color.white, sortingOrder: 7);
            for (int i = 0; i < glow.Length; i++)
                glow[i] = ProceduralSprites.Create(parent, "Teleport Blue Glow " + i, Vector2.one, settings.TeleportBlue, sortingOrder: 4);
            for (int i = 0; i < flash.Length; i++)
                flash[i] = ProceduralSprites.Create(parent, "Teleport White Flash " + i, Vector2.one, Color.white, sortingOrder: 9);
            Hide();
        }
        private void Hide()
        {
            foreach (var piece in fragments) piece.enabled = false;
            foreach (var light in glow) light.enabled = false;
            foreach (var line in flash) line.enabled = false;
        }
        public void Reset() { arrivedId = 0; arrivalTick = -1; Hide(); }
        public void Dispose()
        {
            if (!source) return;
            foreach (var piece in fragments) if (piece.sprite) Object.Destroy(piece.sprite);
            source = null;
        }
        private void Source(Sprite sprite)
        {
            if (sprite == source) return;
            Dispose(); source = sprite;
            var rect = sprite.rect; float width = rect.width / Columns, height = rect.height / Rows;
            for (int y = 0; y < Rows; y++) for (int x = 0; x < Columns; x++)
                fragments[y * Columns + x].sprite = Sprite.Create(sprite.texture,
                    new Rect(rect.x + x * width, rect.y + y * height, width, height), new Vector2(.5f, .5f),
                    sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }
        public void Apply(FighterState fighter, SpriteRenderer body, int tick, bool finished)
        {
            Hide();
            if (finished || fighter.Mode == FighterMode.KO || fighter.HitstunRemaining > 0 || fighter.ParalysisRemaining > 0)
            { Reset(); return; }
            var attack = fighter.Attack;
            bool teleport = attack != null && attack.Definition.Recovery.ChargedTeleport;
            bool charging = teleport && RecoveryMotionSystem.IsCharging(fighter);
            if (teleport && attack.RecoveryStarted && attack.Id != arrivedId)
            { arrivedId = attack.Id; arrivalTick = tick; }
            int age = arrivalTick < 0 ? int.MaxValue : tick - arrivalTick;
            bool arriving = age >= 0 && age < settings.TeleportFlashTicks;
            if (!charging && !arriving) return;
            Source(body.sprite);
            float split = charging ? Mathf.Clamp01((attack.RecoveryChargeFrames - attack.Definition.Recovery.MinimumChargeFrames +
                settings.TeleportDissolveTicks) / (float)Mathf.Max(1, settings.TeleportDissolveTicks)) :
                1 - Mathf.Clamp01((age / 2 * 2) / (float)Mathf.Max(1, settings.TeleportAssembleTicks));
            Color blue = settings.TeleportBlue;
            for (int i = 0; i < glow.Length; i++)
            {
                var light = glow[i]; light.enabled = true; light.sprite = body.sprite; light.flipX = body.flipX;
                light.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
                light.transform.localScale = body.transform.localScale * (1.04f + i * .04f);
                light.color = new Color(blue.r, blue.g, blue.b, (.18f - i * .06f) * (1 - split));
            }
            body.color = Color.Lerp(Color.white, blue, charging ? .65f : .3f);
            if (split > 0)
            {
                body.color = new Color(blue.r, blue.g, blue.b, 0);
                var rect = source.rect; float ppu = source.pixelsPerUnit;
                for (int y = 0; y < Rows; y++) for (int x = 0; x < Columns; x++)
                {
                    int index = y * Columns + x; var piece = fragments[index]; piece.enabled = true;
                    float sign = body.flipX ? -1 : 1;
                    var center = new Vector3(((x + .5f) * rect.width / Columns - source.pivot.x) / ppu * sign,
                        ((y + .5f) * rect.height / Rows - source.pivot.y) / ppu, 0);
                    float angle = (index * 137.5f) * Mathf.Deg2Rad;
                    var scatter = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * settings.TeleportScatter * split;
                    piece.transform.position = body.transform.TransformPoint(center) + scatter;
                    piece.transform.rotation = body.transform.rotation * Quaternion.Euler(0, 0, split * (index % 2 == 0 ? 18 : -18));
                    piece.transform.localScale = body.transform.localScale; piece.flipX = body.flipX;
                    piece.color = Color.Lerp(Color.white, blue, charging ? .8f : split * .8f);
                }
            }
            if (!arriving) return;
            for (int i = 0; i < flash.Length; i++)
            {
                var line = flash[i]; line.enabled = true;
                float opacity = 1 - age / (float)Mathf.Max(1, settings.TeleportFlashTicks);
                line.color = new Color(1, 1, 1, opacity * (i == 0 ? 1 : .65f));
                line.transform.position = body.bounds.center + Vector3.right * (i == 0 ? 0 : (i == 1 ? -.17f : .17f));
                line.transform.localScale = new Vector3(i == 0 ? .065f : .025f, body.bounds.size.y * (i == 0 ? 1.5f : 1.1f), 1);
            }
        }
    }
}
