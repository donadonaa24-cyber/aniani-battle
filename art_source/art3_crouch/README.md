# ART3C：3人共通サイズのしゃがみ

透真・蓮・千鶴の採用済み待機を参照し、内蔵image_genで立ち/しゃがみの2姿勢原画を各1枚生成。立ちの外接高さから求めた倍率をしゃがみにも適用しています。頭や靴を縮めず、膝と腰を曲げています。

最終出力は全員**224×160px/コマ、8F/6fps、約1.33秒ループ、足元y159、PPU50、pivot(0.5,0)**。採用した1枚のしゃがみから整数行移動で体/頭±2pxの呼吸を作り、顔・衣装の画素と靴の下12行を保持しています。ゲーム用PNGは透明、確認GIFの背景は#222731です。

| キャラ | 実装用横シート（1792×160） | 確認用GIF（672×480） | 中立しゃがみの実高さ |
|---|---|---|---:|
| 透真 | [ready/toma](ready/toma/crouch_sheet.png) | [プレビュー](../../docs/art_wip/art3c_toma_crouch_preview.gif) | 91px |
| 蓮 | [ready/ren](ready/ren/crouch_sheet.png) | [プレビュー](../../docs/art_wip/art3c_ren_crouch_preview.gif) | 77px |
| 千鶴 | [ready/chizuru](ready/chizuru/crouch_sheet.png) | [プレビュー](../../docs/art_wip/art3c_chizuru_crouch_preview.gif) | 84px |

各readyフォルダにはcrouch_00.png〜crouch_07.png（透過224×160）、crouch_sheet.png、crouch.json、等倍crouch_preview.gifがあります。ゲームはAniBato/Assets/_Project/Art/Sprites/<キャラ>/crouch.png・crouch.jsonを使用します。既存待機は保持しています。

待機との頭/髪と靴の部分倍率も等倍/3倍で比較しました。頭部帯幅は透真37→37/蓮58→59/千鶴43→44px、靴の下8行の幅差0〜3px。[proportion-check.json](proportion-check.json)に測定領域を含めて保存。セルの寸法だけで合格にせず、今後も採用待機の頭/靴を基準にします。

## 原画・倍率・出典

| 原画 | 原画の寸法 | 立ちの実高さ | 共通倍率 | 立ち基準 |
|---|---|---:|---:|---:|
| [toma_crouch_pair_v1](generated/toma_crouch_pair_v1.png) | 2098×750 | 708px | 124/708 | 124px |
| [ren_crouch_pair_v1](generated/ren_crouch_pair_v1.png) | 2098×750 | 674px | 100/674 | 100px |
| [chizuru_crouch_pair_v1](generated/chizuru_crouch_pair_v1.png) | 1983×793 | 670px | 112/670 | 112px |

大きい原画はゲーム用セルとは異なります。左の立ち姿を倍率の物差しにし、右のしゃがみを同倍率で縮小・共通32色へ整えます。しゃがみの高さだけを独立してリサイズしません。分割矩形・実倍率・配置は[normalization.json](normalization.json)、実際の全文プロンプトと参照の役割は[generation-log.json](generation-log.json)。normalized/は減色前の中間シートです。

参照はart_source/art3_idle/<キャラ>_idle_v1.png（容姿/衣装）とbreathing/ready/<キャラ>/idle_00.png（画素倍率/色）。原画・旧rawは保存し、旧raw由来の全クリップ共通パレットを維持しました。生成サービスへの参照はこの採用済み自作キャラのみ。本作のしゃがみと呼吸は独自表現です。

## 再現・引き継ぎ

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Crouch.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Crouch.ps1 -VerifyOnly
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/CompareArt3Proportions.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildSpriteGenerationKit.ps1
```

モデルに依存しない[詳細な再制作指示書](../../docs/SPRITE_GENERATION_GUIDE.md)と[持ち出し用ZIP](../sprite_generation_kit.zip)には、参照PNG・パレット・全文プロンプト・規格・待機/しゃがみの採用済み出力をまとめています。固定値は[SPRITE_SPEC](../../docs/SPRITE_SPEC.md)。Unity全4工程の結果と実機撮影の未確認事項は[最終報告](../../docs/codex_logs/art3c_final.md)を参照してください。
