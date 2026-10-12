# ART3C：しゃがみ3人・再制作指示書（2026-10-13）

## 完了したこと

[ART3_CROUCH](../codex_tasks/ART3_CROUCH.md)を作成してから着手。透真・蓮・千鶴の新待機を参照し、内蔵image_genで同倍率の立ち/しゃがみ原画を各1枚生成。立ちの実高さを物差しにして共通倍率で正規化し、しゃがみだけを小さくリサイズする処理は使っていません。採用1枚の画素から整数行移動で呼吸を作り、全員224×160px/コマ・8F/6fps・足元159・PPU50へ接続しました。

- 実装用の各8枚・1792×160横シート・JSON・GIF：[art3_crouch](../../art_source/art3_crouch/README.md)。原画・全文プロンプト・分割矩形/倍率を保存。中立高さは透真91/蓮77/千鶴84px、立ちの身長基準124/100/112pxを維持。
- crouchのprebuiltSheetとAnimationSetだけを差し替え、新待機とその他のPNG・既存Timing/スプライトIDを維持。Title/BattleはEditor BuildAllで生成。新しゃがみの顔/衣装は全8コマで同じ画素を使い、靴の下12行は完全固定。
- [再制作指示書](../SPRITE_GENERATION_GUIDE.md)に、モデルに依存しない固定値・参照の役割・3人の容姿/衣装・プロンプト・倍率・整数行移動・JSON・採否基準を記録。[持ち出し用ZIP](../../art_source/sprite_generation_kit.zip)には参照9PNG・原画3PNG・パレット・プロンプト・規格・待機/しゃがみ出力を同梱。84ファイルのSHA-256をZIP内まで照合。
- デスクトップのGIFプレビューがファイルをメモリマップすると再生成に失敗する問題を修正。同一内容は書き直さず、変更は一時ファイルから置換。GIFのバイト形式は維持しました。
- ユーザーの大きさの懸念に合わせ、採用待機との頭/髪・靴の部分比較を追加。頭部帯の幅は透真37→37/蓮58→59/千鶴43→44px、靴の下8行の幅差0〜3px。等倍/3倍で目視し、測定領域と結果をproportion-check.json、今後の条件を指示書§3とキットへ保存。部位の投影幅は目視を補う指標で、頭の縦寸法の測定とは扱わない。

## 検証結果

| 検証 | 結果・証跡 |
|---|---|
| Unity BuildAll | **成功、exit 0**。TestResults/art3c_buildall.log、生成シーン実在 |
| Unity EditMode | **326/326合格、失敗/スキップ0**。art3c_EditMode.xml。終了2026-10-13 00:00:49 JST |
| Unity PlayMode | **29/29合格、失敗/スキップ0**。art3c_PlayMode.xml。終了00:02:08 JST |
| Windowsビルド | **成功、exit 0、Result: Success**。art3c_build.log、Build/art3c/AniBato.exe実在 |
| 新しゃがみ24F | サイズ/PPU/二値alpha/32色以内/頭頂実高さ/固定靴/胴の上下/顔と上体の画素移動/GIF復号合格。art3c-crouch-verification.json |
| 既存待機24F回帰 | ArtThreeIdle.Verify合格。新待機の画素と動きを維持 |
| 待機との部分倍率比較 | **頭部帯の幅差0〜1px、靴の幅差0〜3px、目視確認済み**。CompareArt3Proportions.ps1、art_source/art3_crouch/proportion-check.json。肩/マントを測定領域から除外 |
| 全素材再生成 | **425ファイルSHA-256一致、106クリップ584F**。BuildArt3Crouch.ps1 -RebuildCheck |
| 生成キット | **84ファイルのSHA-256をZIP内まで照合、合格**。BuildSpriteGenerationKit.ps1 |
| メモリマップ中のGIF | 同一内容・変更内容の書き込み、変更後GIF復号合格。TestResults/art3c-mapped-gif-check.ps1 |
| Unity API参照コンパイル | 警告0・エラー0。art3c-dotnet-unity-compile.log。Unity実行結果とは別に記録 |

Unityはtools/verify/run_unity.ps1 -Tag art3cをサンドボックス外で承認実行し、-nographicsなし。既存アサーションは変更せず、Crouch状態の8コマ/ループ・固定サイズ/倍率・足/顔の画素保持を確認する3ケースを追加。全4工程で合格後にcommit/pushする区切りです。

部分比較と指示書/キットを追加した後も425ファイルの再生成SHA-256が一致し、検証済みのゲーム用PNG/JSONを保持。Unity以後にゲームのコードや接続を変更していません。

プレビューは[GIF一覧と同倍率比較](../art_wip/art3c_previews.md)。実機撮影は[撮影状態](../screenshots/art3c_previews.md)に記録：最前面確保に失敗し、ART3Cスクショは未保存。Windows版でのしゃがみ目視確認は残課題です。自動テスト合格や素材比較を実機確認として扱いません。

## 交代・次の作業

Codex/Claudeとも[RESUME](../RESUME.md)冒頭・git statusから再開。開始時tasklistとこのフォルダの別active担当がいないことを確認しました。完了一式のコミット名は`Add fixed-size crouch sprites and portable generation guide`、反映先origin main。交代時git log/statusで実際の反映状態を確認してください。

最新ビルドはart3cとart3bのみ保持。次は実機でのしゃがみ確認、またはwalk/dash/攻撃等を新絵柄へ合わせる作業が候補。着手前にdocs/codex_tasks/へ指示書を作り、224×160セル・立ち由来の倍率・共通パレットを守ります。物理パッド/144Hz/全チュートリアルなど6Bの手動確認項目も残っています。
