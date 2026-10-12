# ART3・ART3B完了報告（2026-10-12）

## やったこと

透真・蓮・千鶴の新しい待機絵3枚を生成・採用し、体と頭が上下±2px動く8コマ/6fpsの呼吸待機をゲームへ接続した。靴の下12行は全コマ完全一致。顔・衣装は同じ画素を整数行移動して保持する。

今後の全キャラ・全モーションを224×160px/コマ、足元159、PPU50に固定。[SPRITE_SPEC](../SPRITE_SPEC.md)をAGENTS・SpriteForge・Unityインポータへ反映。身長差は透真124/蓮100/千鶴112pxで維持する。原画・プロンプト・呼吸4キー/人を保存し、実装用8枚/人・1792×160横シート・JSON・GIFも出力した。4キーは動きの参考、ゲーム用8Fは静止原画から作る。

変更：idle PNG/JSON・AnimationSet、BuildAll生成Title/Battle、SpriteForgeのprebuiltSheet/ArtThreeIdle/BuildArt3Idle、インポート寸法検査、Unityアセット回帰3ケース、制作/出典/再開資料。旧rawと待機以外のPNG・Timing・スプライトID・戦闘性能は維持。撮影はゲーム表示領域のみ、ビルド削除はBuild配下の検証済み絶対パスのみとする。

## 実行した検証

| 工程 | 実結果 | ローカル証跡 |
|---|---|---|
| Unity BuildAll | 成功、exit 0。Title/Battle実在 | TestResults/art3b_buildall.log |
| Unity EditMode | **323/323、失敗0・スキップ0** | TestResults/art3b_EditMode.xml・.log |
| Unity PlayMode | **29/29、失敗0・スキップ0** | TestResults/art3b_PlayMode.xml・.log |
| Windowsビルド | **Success、exit 0、実行ファイル実在** | TestResults/art3b_build.log、Build/art3b/AniBato.exe |
| API参照コンパイル | 警告0・エラー0 | TestResults/art3b-dotnet-unity-compile.log |
| 素材 | 新24F：靴固定・体の上下動・顔/衣装画素一致・224×160/PPU50・二値alpha・32色以内・実頭頂高さ・GIF復号が合格 | TestResults/art3b-breathing-verification.json |
| 再生成 | **388ファイルSHA-256一致**。全106クリップ584Fの品質検査も合格 | BuildArt3Idle.ps1 -RebuildCheckの実行結果 |

Unityはサンドボックス外の承認実行でrun_unity.ps1 -Tag art3bを使用。-nographicsなし。既存アサーションを維持し、3キャラの体呼吸と固定足・画素/寸法のテストを追加。EditMode終了23:19:22 JST、PlayMode終了23:20:43 JST。ビルドはart3b/phase6bの2つのみ保持。

## スクショ・再現・引き継ぎ

「PCに触らないで」と事前に案内し、検証済みWindows版へ実キー入力を送った。[5枚の実画面](../screenshots/art3b_previews.md)で3人の新idle・接地・頭上カウンターを目視確認。[GIF](../art_wip/art3b_previews.md)、[実装用出力](../../art_source/art3_idle/breathing/README.md)。ゲーム領域に絞って再撮影済みで、他ウィンドウは写らない。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag art3b
```

Claudeへは[RESUME](../RESUME.md)冒頭・ART3_BREATHING・本報告を渡す。開始時tasklist/git status、同時編集禁止、次の指示書を先に作る。新たなUnity変更は全4工程、撮影前の声かけを継続する。

今回一式のコミット名は`Add fixed-size breathing idle sprites for three fighters`、反映先はorigin main。交代時はgit log/statusとremoteの一致を確認する。

## 残課題・次の候補

移動・攻撃・勝敗などは既存の絵柄。次に揃える場合は新待機を容姿の基準にし、同セル/同倍率で指示書から着手する。今回の上下幅・速度の好みと他モーションへ切り替わるときの見た目は、ユーザー/Claudeの実プレイ所見で次の調整を選ぶ。物理パッド/144Hz/全14課題など6Bの未記録の手動確認は別に残る。
