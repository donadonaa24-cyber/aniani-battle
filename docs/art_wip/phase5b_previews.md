# フェーズ5B アニメプレビュー

既存原画からSpriteForgeで局所中割り・整合を行った42クリップ。idleは単一キーの胴・足を固定し、上部だけ±1pxの呼吸を生成する。idle12fps、その他15fps。原画・旧加工結果は保持し、新規AI生成は行っていない。全出力368F、42GIF全フレーム復号、217ファイル再生成のSHA-256一致を検査済み。大きい姿勢差ではキーポーズ切替が残り、Unity実画面での評価は待ち。

| クリップ | コマ数 | 透真 | 蓮 | 千鶴 |
| --- | ---: | --- | --- | --- |
| idle | 8 | [GIF](toma_idle_preview.gif) | [GIF](ren_idle_preview.gif) | [GIF](chizuru_idle_preview.gif) |
| walk | 8 | [GIF](toma_walk_preview.gif) | [GIF](ren_walk_preview.gif) | [GIF](chizuru_walk_preview.gif) |
| dash | 8 | [GIF](toma_dash_preview.gif) | [GIF](ren_dash_preview.gif) | [GIF](chizuru_dash_preview.gif) |
| jump | 6 | [GIF](toma_jump_preview.gif) | [GIF](ren_jump_preview.gif) | [GIF](chizuru_jump_preview.gif) |
| fall | 6 | [GIF](toma_fall_preview.gif) | [GIF](ren_fall_preview.gif) | [GIF](chizuru_fall_preview.gif) |
| attack1 | 6 | [GIF](toma_attack1_preview.gif) | [GIF](ren_attack1_preview.gif) | [GIF](chizuru_attack1_preview.gif) |
| attack2 | 6 | [GIF](toma_attack2_preview.gif) | [GIF](ren_attack2_preview.gif) | [GIF](chizuru_attack2_preview.gif) |
| attack3 | 6 | [GIF](toma_attack3_preview.gif) | [GIF](ren_attack3_preview.gif) | [GIF](chizuru_attack3_preview.gif) |
| air_attack | 6 | [GIF](toma_air_attack_preview.gif) | [GIF](ren_air_attack_preview.gif) | [GIF](chizuru_air_attack_preview.gif) |
| special_neutral | 6 | [GIF](toma_special_neutral_preview.gif) | [GIF](ren_special_neutral_preview.gif) | [GIF](chizuru_special_neutral_preview.gif) |
| special_side | 6 | [GIF](toma_special_side_preview.gif) | [GIF](ren_special_side_preview.gif) | [GIF](chizuru_special_side_preview.gif) |
| special_up | 6 | [GIF](toma_special_up_preview.gif) | [GIF](ren_special_up_preview.gif) | [GIF](chizuru_special_up_preview.gif) |
| special_down | 6 | [GIF](toma_special_down_preview.gif) | [GIF](ren_special_down_preview.gif) | [GIF](chizuru_special_down_preview.gif) |
| guard | 6 | [GIF](toma_guard_preview.gif) | [GIF](ren_guard_preview.gif) | [GIF](chizuru_guard_preview.gif) |

一覧PNG：[透真](toma_contact.png)・[蓮](ren_contact.png)・[千鶴](chizuru_contact.png)。細部は同名contact_01〜06.pngの分割ページで確認できる。持続姿勢の添字は元キーの進行度を6〜8Fへ再マップし、戦闘の発生・持続・硬直は絵から変更しない。

再生成は`tools/SpriteForge/BuildPhase5B.ps1`、検査・再現性は`tools/SpriteForge/VerifyPhase5B.ps1 -Rebuild`。Unityへ反映するにはClaudeのBuildAllが必要。
