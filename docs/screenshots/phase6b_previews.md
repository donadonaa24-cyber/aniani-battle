# フェーズ6B before/after（Unity実画面の代替）

6BのUnityは現権限で実行できないため、生成済み背景とサムネ、実CameraFramingの出力を使ったオフライン投影図で代替した。**Unityスクリーンショットではない**。投影図内にも明記。描画の確認用で、HUD・中景セル・縮小のマスク・時間追従・SE・GPUフレーム時間を再現していない。

背景は内蔵image_gen原画、前のサムネは既存BuildAll成果物。作成手順は `BuildPhase6B.ps1`、`dotnet run --project tools/Validation/BattleValidation.csproj -- --phase6b-camera`、`RenderPhase6BPreviews.ps1`。

| テーマ | before：旧サムネ | after：新サムネ |
|---|---|---|
| 星の終わり | [before](phase6b_before_thumbnail_city.png) | [after](phase6b_after_thumbnail_city.png) |
| 推し駒Battle | [before](phase6b_before_thumbnail_board.png) | [after](phase6b_after_thumbnail_board.png) |
| Battle à la carte | [before](phase6b_before_thumbnail_festival.png) | [after](phase6b_after_thumbnail_festival.png) |

カメラ比較は同じ新背景・idle・地形で倍率の差だけを示す。開始のoverviewは場外±24の2u内側まで、combatは上足場±11.5と両者、shrinkは幅10.8の中央床と近い両者。停止/アスペクト変更・追従中の包含はテストするが、実画面の最終確認はClaudeへ引き継ぐ。

| 比率 | 旧カメラ | 開始全体 | 対戦 | 縮小後 |
|---|---|---|---|---|
| 16:9 / 1920×1080 | [before](phase6b_camera_before_1920x1080.png) | [overview](phase6b_camera_overview_1920x1080.png) | [combat](phase6b_camera_combat_1920x1080.png) | [shrink](phase6b_camera_shrink_1920x1080.png) |
| 16:10 / 1920×1200 | [before](phase6b_camera_before_1920x1200.png) | [overview](phase6b_camera_overview_1920x1200.png) | [combat](phase6b_camera_combat_1920x1200.png) | [shrink](phase6b_camera_shrink_1920x1200.png) |
| 4:3 / 1440×1080 | [before](phase6b_camera_before_1440x1080.png) | [overview](phase6b_camera_overview_1440x1080.png) | [combat](phase6b_camera_combat_1440x1080.png) | [shrink](phase6b_camera_shrink_1440x1080.png) |

**未達の点**：全体常時表示と従来以上の常時拡大は両立していない。地形・Sprite/Hurtbox比を維持し、開始全体→上足場/両者→縮小中央に段階化。16:9中央で透真は旧20.7%→対戦17.4%→縮小23.8%、蓮16.7%→14.0%→19.2%。4:3は対戦13.0%/10.5%。縮小後の寄りは改善したが、対戦中のキャラの大きさはユーザーの操作感を元に実機調整が必要。

中割りの確認：

| キャラ | walk | dash |
|---|---|---|
| 透真 | [GIF](../art_wip/toma_walk_preview.gif) | [GIF](../art_wip/toma_dash_preview.gif) |
| 蓮 | [GIF](../art_wip/ren_walk_preview.gif) | [GIF](../art_wip/ren_dash_preview.gif) |
| 千鶴 | [GIF](../art_wip/chizuru_walk_preview.gif) | [GIF](../art_wip/chizuru_dash_preview.gif) |

各8F・15fps。新規2/4姿勢を画像生成で描き、既存キーと順次配置した。前後コマの重ね合わせなし。原画は[生成記録](../../art_source/phase6b/generation-log.md)。
