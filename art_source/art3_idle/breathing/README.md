# ART3B：3人共通サイズの呼吸待機

透真・蓮・千鶴すべて、1コマ224×160px、8コマ、6fps、約1.33秒のループ。足元はy=159、PPU50、pivot(0.5,0)。体と頭は上下±2px、靴の下12行は全コマ同じです。人物の身長差は保っています。

| キャラ | 実装用8枚・横シート・JSON・GIF | 確認用GIF（3倍表示） |
|---|---|---|
| 透真 | [ready/toma](ready/toma/idle_sheet.png) | [672×480プレビュー](../../../docs/art_wip/art3b_toma_idle_preview.gif) |
| 蓮 | [ready/ren](ready/ren/idle_sheet.png) | [672×480プレビュー](../../../docs/art_wip/art3b_ren_idle_preview.gif) |
| 千鶴 | [ready/chizuru](ready/chizuru/idle_sheet.png) | [672×480プレビュー](../../../docs/art_wip/art3b_chizuru_idle_preview.gif) |

各readyフォルダのidle_00.png〜idle_07.pngは透過224×160px。idle_sheet.pngは1792×160px、idle.jsonはUnity接続と同じメタデータ、idle_preview.gifは等倍224×160pxです。実ゲームはAniBato/Assets/_Project/Art/Sprites/<キャラ>/idle.png・idle.jsonを参照します。プレビュー背景色はゲーム用PNGに含まれません。

generated/は内蔵image_genで静止原画を参照して生成した4キーの呼吸アトラスです。細部や靴がキー間で変わるため動きの参考として保存し、ゲーム用8コマは採用済み静止絵の画素を整数行移動して作っています。顔・衣装のちらつき、前後画像の混合、倍率変化を防ぎ、下腿で小さな上下動を吸収して足を接地させています。normalized/は減色前の加工中間データです。独自の待機演出であり原作の正史設定ではありません。

最終プロンプトと参照の役割は[generation-log.json](generation-log.json)。原画は保存したままです。旧raw idleは他モーションの縮尺・32色パレット基準として保持し、manifestのprebuiltSheetで新idleだけを差し替えています。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1 -VerifyOnly
```

規格は[SPRITE_SPEC](../../../docs/SPRITE_SPEC.md)。足固定・胴の上下動・顔/衣装の画素保持・色数・二値alpha・頭上高さ・GIF復号・再生成一致を検査します。Unity全4工程の結果は[art3b_final](../../../docs/codex_logs/art3b_final.md)に記録します。
