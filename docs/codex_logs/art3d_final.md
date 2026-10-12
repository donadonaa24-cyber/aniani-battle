# ART3D：3人の基本動作とジャンプ予備/着地（2026-10-13）

## 実装

[ART3_ACTIONS](../codex_tasks/ART3_ACTIONS.md)を作成して着手。全3人のジャンプ→ガード→回避→ダッシュを順番に内蔵image_genで生成・検査した。追加指定で地上ジャンプの前と着地後の浅いかがみを生成し、ユーザーが選択した「2フレームかがんでから踏切」を純C#に実装した。

- 固定セル224×160、PPU50、pivot(0.5,0)、地上足y159、共通32色。立ち物差しの124/100/112pxから同倍率で全姿勢を正規化。全51キーで頭部帯幅が待機基準±2px以内、頭/顔/靴/関節/服/小物を等倍と3倍で目視確認。人物や部分の独立リサイズなし。
- 原画19枚を保存（採用15、不採用4）。蓮の回避v1は端切れ/v2は頭周辺幅超過、dash v1はフード幅超過、千鶴dash v1も頭部幅超過で再生成。採用は蓮dodge v3/dash v2、千鶴dash v2。他はv1。許容値を緩めず、原画/全文プロンプト/寸法/採否/共通倍率/実マスクを[art3_actions](../../art_source/art3_actions/README.md)へ保存。
- 実マスクで人物を分離し、隣のマントがx方向に重なっても画素を混ぜない。透明領域で分離した瞳等は同一人物の外接矩形内にある場合に保持。
- jump6F/15fps・fall6F/15fps・land2F/15fps・guard6F/15fps・dodge3F/12fps・dash8F/15fps、18クリップ93Fを差し替え。完全なキーを保持/切り替え、混合や残像なし。全106クリップ584F、既存スプライトID/Timing/待機/しゃがみを維持。ガード/下降は姿勢保持クリップ。
- BattleRules.GroundJumpStartupFrames=2、ルールschema7へBuildAllで移行。FighterMode末尾JumpStartup=15/AnimationState末尾34、既存値は維持。予備2tickは接地・移動停止しlandのコマ0を表示、3tick目に既存初速で踏切。空中2段は即時。予備開始で従来スタミナコストを1度だけ支払い、中断時の払い戻しなし。予備中の新行動バッファ保持。被弾/場外/足場消失で中断、ヒット停止中は残りtickを停止。浅い姿勢に通常しゃがみのHurtbox縮小を適用しない。
- 着地8tickの最初4tickは浅いかがみ（実高さ104/89/98px）、次4tickは採用待機と同じ画素。深いしゃがみの流用なし。
- [詳細指示書](../SPRITE_GENERATION_GUIDE.md)§10、SPRITE_SPEC/GAME_DESIGN/BALANCE_CONFIG、[持ち出しZIP](../../art_source/sprite_generation_kit.zip)更新。キットは原画/採用選択/共通倍率/全プロンプト/32色/参照/実装PNG/生成ツールを同梱。生成ツールは元リポジトリのmanifest/旧rawが必要で、新AI生成の画素が完全一致するとは扱わない。

## 検証結果

サンドボックス外・-nographicsなしでtools/verify/run_unity.ps1 -Tag art3dを実行。最終結果：

| 工程 | 結果 | 証跡 |
|---|---|---|
| BuildAll | 成功・exit0 | TestResults/art3d_buildall.log、生成Title/Battle・SO/AnimationSet |
| Unity EditMode | **357/357、失敗/スキップ0** | TestResults/art3d_EditMode.xml/log、終了2026-10-13 01:53:43 JST |
| Unity PlayMode | **30/30、失敗/スキップ0** | TestResults/art3d_PlayMode.xml/log、終了2026-10-13 01:55:04 JST |
| Windowsビルド | 成功・exit0 | TestResults/art3d_build.log、Build/art3d/AniBato.exe |

純C#代替317/317、Unity API参照コンパイル警告/エラー0。18クリップ93Fの固定セル/アンカー/キーとの全画素一致/二値alpha/色/GIF全コマ復号と、既存待機24F・しゃがみ24Fの回帰に合格。665ファイル再生成SHA-256一致。ZIP内341ファイルのチェックサム照合合格。

新しいEditMode検査は予備/被弾/場外/足場/入力保持/ヒット停止、全新クリップの全コマ/fps/loop/足元/実頭頂、実予備/着地8tick/採用待機への復帰を検証。既存ID回帰も新6クリップへ拡張。PlayModeは実Input System→BattleRunner→SpriteRendererで3人の予備2tick/踏切/ガード/回避/ダッシュを確認。

初回はEdit356/357、Play29/30。Editの旧schema6固定期待を7へ更新し予備2も追加検査。Playの追加テストは入力イベント反映前に読んだため、既存の実PlayerLoop待ちへ修正。該当Playテスト1/1合格後に全4工程を再実行し、上の全合格となった。既存のジャンプ後の判定は予備後まで待ち、旧アサーションを同じ強さで検査する。失敗を無効化/許容値の緩和/スキップで通していない。

## 実機・保存・交代

事前に「PCに触らないで」と案内して[実機撮影](../screenshots/art3d_previews.md)を試行したが、最前面を確保できずキー送信前に停止、PNG未保存。Windows画面の目視は未完了。[素材の比較/GIF](../art_wip/art3d_previews.md)は実機撮影ではない。撮影停止後はPC操作可能と案内済み。

保持ビルドはart3d/art3c。古いTestResultsのphase5/5b/6a/6bファイル111件を指定の容量管理で整理し、art3b/c/dとdotnet/dotnet-homeを保持。過去の最終報告は保持。公開対象のテキストに個人絶対パス/秘密パターンなし。

全4工程合格後、この区切りを`Add fixed-size action sprites and two-tick jump anticipation`の名前でcommitし、origin mainへ通常pushする。実際のSHA/反映はgit log/statusで確認する。force pushなし。Claudeへは[RESUME](../RESUME.md)冒頭・[引き継ぎ書](../HANDOFF_TO_CODEX.md)§4・本報告を渡す。開始時tasklist/git status、同時編集禁止、次の指示書を先に作る。再現はBuildArt3Actions.ps1 -RebuildCheck（検査だけなら-VerifyOnly）、キット再作成はBuildSpriteGenerationKit.ps1。

残課題はWindows実機画面の目視、walk/攻撃/勝敗などの新絵柄化、物理パッド/144Hz等の6B手動確認。次に進む場合は指示書から作る。今回のユーザー依頼は全4動作と予備/着地の素材生成・ゲーム接続まで完了。
