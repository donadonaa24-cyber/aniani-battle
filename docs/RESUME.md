# 再開メモ：ART3C完了・Codex/Claude交代（2026-10-13）

## 現在の区切り：3人の新しゃがみ・再制作指示書、Unity全合格

- 指示書：[ART3_CROUCH](codex_tasks/ART3_CROUCH.md)。採用済み新待機を参照し、内蔵image_genで全3人の立ち/しゃがみ原画を生成。立ちの外接高さ由来の共通倍率でしゃがみを正規化し、頭/靴を小さくする処理は使わない。
- 固定：**224×160px/コマ、足元159、PPU50、8F/6fps、体/頭±2px、靴の下12行固定**。中立高さは透真91/蓮77/千鶴84px。顔/衣装は単一採用絵の整数行移動。待機やその他のPNG、既存Timing/スプライトIDは保持。
- 出力：[art3_crouch](../art_source/art3_crouch/README.md)の各8枚・横シート・JSON・GIF、原画/全文プロンプト/分割矩形/共通倍率。ゲームcrouch/AnimationSetへ接続し、Title/BattleはBuildAllで再生成。
- 他環境向け：[詳細な再制作指示書](SPRITE_GENERATION_GUIDE.md)、[持ち出しZIP](../art_source/sprite_generation_kit.zip)。参照9PNG/原画3PNG・全プロンプト・共通32色・spec.json・待機/しゃがみの採用出力を同梱。今後の全モーションでも同じセルと立ち倍率を使う。
- **今回のUnity実行：BuildAll成功・EditMode326/326・PlayMode29/29・Windowsビルド成功、失敗/スキップ0**。TestResults/art3c_*、テスト終了2026-10-13 00:00:49/00:02:08 JST。既存アサーション維持、新しゃがみ接続/倍率/足/顔/ループの3ケース追加。
- 素材24F＋待機24F回帰、425ファイル再生成SHA-256一致（106クリップ584F）、ZIP内84ファイル照合合格。Unity API参照コンパイル警告/エラー0。メモリマップ中のGIF保存問題は同一内容の書き込み省略/変更内容の置換で修正、復号検査合格。
- 起動：**Build/art3c/AniBato.exe**、保持ビルドart3c/art3b。[プレビュー](art_wip/art3c_previews.md)は素材比較。[実機撮影](screenshots/art3c_previews.md)は最前面確保に失敗し未保存、Windows画面の新しゃがみ目視確認は残す。撮影前は必ず「PCに触らないで」と案内、安全チェックを維持。
- 完了記録：[art3c_final](codex_logs/art3c_final.md)。コミット名`Add fixed-size crouch sprites and portable generation guide`、反映先origin main。開始時はclean、このフォルダに別active担当なし。前回ART3Bの7084a3fもpush済み。交代時はgit log/statusで今回の実際の反映を確認する。
- **Claudeへ交代**：AGENTS→HANDOFF→本書→STATUSを読み、tasklist/git statusで同時編集を防ぐ。再現はBuildArt3Crouch.ps1 -RebuildCheck（または-VerifyOnly）、キットはBuildSpriteGenerationKit.ps1。新Unity変更はrun_unity.ps1 -Tag <新作業名>を外部承認で全4工程。次の作業の指示書を先に作る。
- 次の候補：実機のしゃがみ確認、walk/dash/攻撃等の新絵柄化。6Bの物理パッド/144Hz等の手動確認は下の履歴。完了済み6Bや待機の作り直しは不要。
- 大きさの追加確認：頭部帯幅は透真37→37/蓮58→59/千鶴43→44px、靴の下8行の幅差0〜3px。等倍/3倍で目視確認、proportion-check.jsonと指示書§3へ保存。新ポーズも採用待機の頭/靴を基準に比較する。部分比較をBuildArt3Crouch/キット作成へ組み込み、追加後も425ファイル一致。

---

## ART3B完了時の履歴：3人の新しい呼吸待機、Unity全合格

- 指示書：[ART3_IDLE](codex_tasks/ART3_IDLE.md) → [ART3_BREATHING](codex_tasks/ART3_BREATHING.md)。ユーザーの追加依頼で3枚とも採用し、体が上下して呼吸する待機をゲームへ接続した。
- 固定規格：[SPRITE_SPEC](SPRITE_SPEC.md)。全員・今後の全モーションは**224×160px/コマ、足元y159、PPU50**。身長は透真124/蓮100/千鶴112。8F/6fps、体±2px、靴の下12行は完全固定。
- 出力：[breathing/ready](../art_source/art3_idle/breathing/README.md)の8枚/人・1792×160横シート・JSON・GIF。原画/最終プロンプトはart_source/art3_idle/とbreathing/generation-log.json。生成した4キーは動きの参考で、最終8Fは採用済み静止絵の整数行移動。旧rawは他モーションの共通縮尺/パレット基準として保持。
- 実装：SpriteForgeのprebuiltSheet、ArtThreeIdle/BuildArt3Idle、全3人idle/AnimationSet、固定セルのインポート検証、Unityアセットテスト3ケース。既存Timing・スプライトIDと待機以外のPNGを維持。シーンはEditor BuildAllで生成。
- 検証：**BuildAll成功、Unity EditMode323/323・PlayMode29/29、失敗/スキップ0、Windowsビルド成功**。TestResults/art3b_*、編集テスト終了23:19:22 JST・プレイテスト終了23:20:43 JST。API参照コンパイルも警告/エラー0。24Fの靴固定・胴/顔の画素移動など合格、388ファイル再生成SHA-256一致。
- 起動：**Build/art3b/AniBato.exe**。最新2ビルドart3b/phase6bのみ保持。[実ゲーム撮影](screenshots/art3b_previews.md)で3人の待機・接地・頭上数値を目視確認。撮影前のPC操作禁止案内と最前面チェックを守る。
- 完了記録：[art3b_final](codex_logs/art3b_final.md)。完了一式のコミット名は`Add fixed-size breathing idle sprites for three fighters`、反映先origin main。交代時git log/statusで確認する。更新記録の67a8d9cは前回の区切り。
- 次の候補：walk/dash/攻撃を新待機の絵柄へ揃える。今回の対象はidleで、他の動作は既存絵柄。新しい指示書を先に書き、同セル/同倍率・元原画保持で実装する。6Bの物理パッド/144Hzなどの残確認は下の履歴を参照。
- **Claudeへ交代時**：tasklistとgit statusで同時編集がないことを確認し、この冒頭から再開。再現はtools/SpriteForge/BuildArt3Idle.ps1（-RebuildCheckまたは-VerifyOnly）。新たなUnity変更はrun_unity.ps1 -Tag <作業名>を外部承認で全4工程。撮影前には必ず「PCに触らないで」と声をかける。

---

## 6B完了時の状態と共通交代手順

**6Bは完了済み。Codexがメイン実装・レビュー・Unity検証・取りまとめを担当し、Claudeにも随時交代できる。** ユーザーの最新指定を優先する。以下の過去履歴にある「検証待ち／Codexはcommitしない」は現在の指示ではない。

## 確認済みの状態

- Unity：BuildAll成功、**EditMode320/320・PlayMode29/29、失敗・スキップ0、Windowsビルド成功**。TestResults/phase6b_runner.txt・各XML・生成/ビルドログを確認。今回は保存済み結果の確認で、再実行はしていない。
- 実装コミット **9560ed4**（6A・6B）はorigin mainへpush済み。git ls-remoteでも一致。資料更新開始時はclean。
- 起動：**Build/phase6b/AniBato.exe**。Title/Battleシーンも実在。ビルドはphase6a・phase6bの2つを保持。
- 現在の作業：[PHASE6B_CLOSEOUT](codex_tasks/PHASE6B_CLOSEOUT.md)。担当分担と完了記録のみ更新。コード・アセット・テストは変更していない。
- 画像：[phase6b比較一覧](screenshots/phase6b_previews.md)、docs/screenshots/phase6b_*.png。サムネ/オフライン投影図でありUnity実画面ではない。新しい実キー撮影は行っていない。

## 次の作業・交代時の手順

1. AGENTS → HANDOFF → 本書 → STATUSを読み、tasklistでcodex.exeと他担当の作業を確認。同じフォルダの同時編集は禁止。
2. git statusと最新コミットを確認。記録更新の差分が残っていればPHASE6B_CLOSEOUTに沿って確認・commit/pushする。交代する担当に作業名・差分・検証結果を渡す。
3. 次はユーザーの実プレイ所見を反映する具体的な修正を選び、docs/codex_tasks/に指示書を作ってから着手。新機能の範囲はまだ決めていない。
4. 手動所見の記録が今回見つからなかった項目：物理DualSense/2台再接続、3比率/144Hzのカメラ・HUD・新中割り・縮小、全14課題、音。実SO810試合集計も未確認。必要に応じて次の確認対象にする。
5. Unityはtools/verify/run_unity.ps1 -Tag <作業名>をサンドボックス外で承認申請。全4工程が合格した区切りでcommit・origin mainへpush。.NETだけで完了扱いにしない、アサーションを弱めない、-nographics/force push禁止。
6. play_and_capture.ps1の前に必ずユーザーへ「PCに触らないで」と声をかける。実行結果とスクショを確認し、本書/STATUSへ記録する。

カメラの全体常時表示と常時拡大、idleの目元/柄、サポート・追加キャラ・オンライン・正式音源などは改善候補/未実装。完了した6Bの再実装は不要。

---

## 以下は6B提出時の履歴（現在の結果・担当は上記を参照）

# 再開メモ：フェーズ6BのUnity検証・実機調整待ち（提出時）

[PHASE6B](codex_tasks/PHASE6B.md)のコード・素材・資料を更新。**commit/pushなし。6B Unity全実行はClaude担当、PlayMode全合格はまだ確認していない。** 6A履歴：BuildAll成功・EditMode305/305・PlayMode24/26・Windowsビルド成功（Build/phase6a/）。既存ポータルの試作版公開は別作業で、6Bでは公開していない。

完了：DualSense差分イベント→USB HIDレポート、try/finally・UnityTearDownの全機器削除、旧シーン停止、入力更新ごとの軸サンプル共有。旧アサーションを維持し、全8USB行動・設定保存/専用ダッシュ・カメラ比率のPlayMode回帰を追加。生成済み背景3枚・中割り3アトラスを復号・品質確認して再利用し、walk2姿勢＋dash4姿勢/人を旧原画と接続。重ね合わせなし。背景/サムネ6PNG、6×6セル・縮小演出、段階的カメラ、左右表記、両者共通ダッシュ2回ON/OFF、必須24テスト・成果物12項目・拡張資料を更新。

検証：NUnit307成功/0失敗/0スキップ、Editor/Player参照コンパイル警告/エラー0、asmdef既存6件。CPU810試合、ふつう48.09秒/縮小23.7%/超必殺65.6%、自滅近似0。素材106クリップ584F・106GIF、351ファイル再生成一致。TestResults/phase6b-*は**Unityの合格証跡ではない**。

Claudeの次の作業：

1. Unityを閉じ `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase6b`。BuildAll→EditMode→PlayMode→Windowsビルド。-nographics禁止。想定320/29件を合格と扱わない。まず失敗2件と新規USB/設定/カメラ回帰を確認。
2. Title/Battle実在、FarArtwork/Thumbnail・AppearanceSchema1、Presentation.CameraSchema1、新.meta、27方向AttackData・全34状態・Timing/独自調整保持を確認。
3. 実USB DualSenseの×/○/OPTIONS・8行動・中立・再接続、Xbox/PS/キー表記、キー再割当を確認。ダッシュOFF→専用F/右Alt/L3→保存再起動、キーコンフィグ/ポーズの設定・説明も確認。
4. 1920×1080／1920×1200／1440×1080で開始全体・上足場・追従・HUD/頭上カウンター・縮小中央を確認。**カメラの全体常時表示と常時拡大は未達**。地形を維持し段階化したため、ユーザーの操作感で最終調整する。
5. 各背景の視認性・AI警告/外マス消失/文化祭消灯、3人walk/dash GIF・144Hz・F9、練習4相手・全14課題を通す。[代替画像一覧](screenshots/phase6b_previews.md)はUnity実画面ではない。低優先度idle目元/柄は維持。
6. CpuBalanceReportBuilder.Writeで実SO810試合を集計。全合格後に本書/IMPLEMENTATION_STATUSへ結果を追記し、**Claudeがcommit/pushする**。Codexはcommitしない。

再現：`tools/Validation/RunPhase6B.ps1`、`tools/SpriteForge/BuildPhase6B.ps1`、`VerifyPhase6A.ps1 -Tag phase6b -Rebuild`。再生成は6B用Buildを使い6A原画を保持。[最終報告](codex_logs/phase6b_final.md)、[最新状況](../IMPLEMENTATION_STATUS.md)。workspace-write／承認不可ではUnity IPCへ接続できず、HANDOFF §2の外部担当へ引き継ぐ。

---

## 以下は6A提出時の履歴（Unity結果は上の6B記録を参照）

# 再開メモ：フェーズ6AのUnity検証待ち（2026-10-12）

現行は[PHASE6A](codex_tasks/PHASE6A.md)冒頭の追加ユーザー決定。**ソース・素材・.NET検証を完了。UnityはClaude担当、commit/pushはまだ行わない。** 原本は変更していない。旧→新はGAME_DESIGN、旧テスト置換と必須24項目は[PHASE6A_TEST_CHANGES](PHASE6A_TEST_CHANGES.md)。

完了：練習/14課題チュートリアル/技表/自動ボタン表記/キーコンフィグ、しゃがみ・背面・空中7方向、前/斜めの移動、スタミナ、全攻撃ガード/不足超必殺貫通/しゃがみ崩し、控えめなWin/Lose。旧混合生成を廃止し、綺麗なキー保持に変更。

検証：**NUnit292成功/0失敗/0スキップ**、Unity API参照/プレイヤー分岐コンパイルは警告/エラー0、asmdef既存6件。CPU810試合：ふつう **48.09秒/縮小23.7%/超必殺65.6%**、全難易度自滅近似0・全試合両者攻撃/ハート減少。**584F・106GIF全復号・351ファイル再生成SHA-256一致**。証跡TestResults/phase6a-*。Unity実行結果ではない。

Claudeの手順：

1. Unityを閉じて `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase6a`。-nographics禁止。BuildAll→EditMode→PlayMode→Windowsビルド。想定EditMode305/PlayMode26を合格結果とは扱わない。
2. Title/Battle実在、BattleRules.Schema6、キャラAccessibilitySchema6・方向枠10/9技・スタミナ、全34状態、新規.meta/AttackData27個を確認。NewScene前判定/後再読込・Timing保持を維持。
3. CpuBalanceReportBuilder.Writeで実SO810試合をTestResults/phase6a-cpu-unity.*へ保存。平均35〜55秒・縮小20〜40%・超必殺半数前後・自滅ほぼ0を再確認。
4. DualSenseのPS表記、割り当て変更→保存→再起動→初期化、P1/P2独立、画面遷移ガード・再開押下漏れを確認。
5. 練習4相手/固定ON・OFF、チュートリアル14段を順に実行。ガード崩し/不足超必殺/ジャスト、空中斜め・後ろ、下18F降下、着地隙を確認。
6. 144HzでF9判定と絵、新原画の顔/衣装/足元、終了直後/リザルトのWin/Lose、旧walkの二重脚除去を確認。[106GIF](art_wip/phase6a_previews.md)。
7. 結果をIMPLEMENTATION_STATUS/本書へ追記。**この依頼ではcommitしない。**

再現：tools/Validation/RunPhase6A.ps1、tools/SpriteForge/BuildPhase6A.ps1、VerifyPhase6A.ps1 -Rebuild。新攻撃は綺麗な単一キーを保持するため、細かな連続動作の描き足しは改善余地。背景/カメラ最終調整、サポート・オンライン・正式音源は別の残作業。

詳細：[phase6a_final](codex_logs/phase6a_final.md)、[CONTROLS_REVIEW](CONTROLS_REVIEW.md)、[CPU_BALANCE_REPORT](CPU_BALANCE_REPORT.md)。Codexはworkspace-write/承認不可でUnity IPCへ接続できず、ユーザーの分担に従いClaudeへ引き継ぐ。

---

## 以下はフェーズ5Bの履歴（5BはClaudeが全合格を確認）

> **最新（Claude追記）**：フェーズ5B 完了・Claude検証済み：BuildAll成功、EditMode 243/243、PlayMode 23/23、Windowsビルド成功（`Build/phase5b/`）。次は**フェーズ6**（`docs/REVIEW_NOTES.md` の全項目：中割りの描き直し、背景・カメラ、PS表記・キーコンフィグ、操作表示、必須テスト24項目の総点検、最終ビルド、資料）。

# 再開メモ：フェーズ5BのUnity検証待ち（2026-10-12）

原本 docs/SPEC_ORIGINAL.md、指示書 docs/codex_tasks/PHASE5B.md。**0→A→Bのソース実装・素材生成・オフライン検証は完了。Unity実行／実機確認はClaudeへ引き継ぐ。** commit／push／公開なし。

## 完了・結果

- 前フェーズClaude結果：BuildAll成功、EditMode225/225、PlayMode15/20、Windowsビルド成功。
- 回帰5件：ボタン・デバイス単位の解放待ち、押下の重複防止、自動遷移時のガード撤去、Removed通知でもパッド席予約を保持。既存5失敗のアサーションは維持。HeldConfirmの準備はP1の実際のREADYキーEnterを保持する形に更新し、別キー／別パッドの即時受付も追加検査する。Unityの合格は未確認。
- RenderSnapshotsの前後tick補間をキャラ・弾ID・エフェクト・頭上UI・カメラ目標へ接続。停止／KO／反転／位置ジャンプ／再戦はスナップ、アニメは現tick基準。VSync既定ON、同期・60・120・144・無制限、5tick上限と残時間保持。
- 3キャラ各14クリップ更新、245→368F、42GIF。idle8F/12fps、walk/dash8F/15fps、jump/fall/attack1〜3/air_attack/特殊4種/guard6F/15fps。単一基準idleの胴・足固定、頭頂±1px。その他は既存2キーの局所中割り・微小整合・共通32色。原画保持、旧加工結果art_source/phase5b_keys/。AI新規生成は行っていない。
- 特殊発生+12F／硬直+18F／CD120F／ガード時48F以上。上復帰発生維持・CD90F、食事240F維持。弾1人1発、ハート減少後75F無敵と最低(12,8)吹っ飛び。通常3回=1、特殊1、超必殺2、ハート3、70秒・最後10秒縮小は保持。
- CPU防御・攻撃後の間合い・無敵相手への無駄撃ち・空振り連発を改善。ゲージ14/10/20/18→12/8/16/14。ルール・CPU・キャラSchema5移行は旧既定値に一致する項目だけ変更し独自設定を保持。
- .NET230成功／0失敗／0スキップ、全ソース／プレイヤーコンパイルは警告0／エラー0、asmdef6件（新規なし）。無敵・CD・弾制限・設定移行・現行値の30/60/144/240Hz決定性・180試合目標を追加検証。旧原本1〜18の単独回帰も維持。
- 144Hz相当の描画差分0.06597185〜0.06597233u、単調。368F素材、42GIF全コマ復号、idle胴/足一致・頭頂±1px、PNG/JSON/contact/GIF217ファイル再生成のSHA-256一致を検査。千鶴ジャブの持続姿勢は6F化で添字2／attack1_02へ更新し、厳密な一致テストを維持。
- CPU全9組×30seed×3難易度810試合：全試合両者攻撃／ハート減少、自滅近似0。ふつう平均4.83→45.51秒、縮小0→21.9%、超必殺67.8→56.7%。afterは純C#作成既定値、実SO集計ではない。

## 次の手順（Claude）

1. Unityを閉じ、外部実行可能な環境で powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase5b。-nographics禁止。
2. BuildAll後にTitle/Battle実在、ルール/CPU/キャラSchema5、無敵75F・特殊CD・ゲージ12/8/16/14、Presentation接続、アニメセット125/118/125Fと持続姿勢を確認。NewScene前判定／後アセット再読込・Timing保持を維持。
3. Unity想定EditMode243／PlayMode23。**想定件数を合格と報告しない**。フェーズ5の5失敗を含め全件確認。
4. AniBato.EditorTools.CpuBalanceReportBuilder.Writeで実アセット810試合をTestResults/phase5b-cpu-unity.csv/.mdへ保存。ふつうの平均35〜55秒／縮小20〜40%／超必殺半数前後を再確認。
5. ActualFighterViewMovesUniformlyWith144HzFrameTimeSequenceは実FighterViewを計測しphase5b-render-144hz-unity.csvを保存する。144Hz実機のフレーム時間／移動／ジャンプ／弾／カメラ／頭上数値も確認。
6. キーボード・DualSense：確定後StageSelectに留まる、決定保持中もEsc／OPTIONSが有効、ポーズと操作説明の停止、再開J/×の攻撃・ジャンプ漏れなし、再戦、逆順再接続。
7. GIF／無敵点滅／特殊の隙／CPU攻防を目視。結果と所見をIMPLEMENTATION_STATUS／本書へ追記してからフェーズ6へ。

## 再現・残課題

オフラインは tools/Validation/RunPhase5B.ps1、素材生成は tools/SpriteForge/BuildPhase5B.ps1、検査は VerifyPhase5B.ps1（-Rebuildで全3キャラの再現性も検査）／ValidateArt2.ps1。42GIFは docs/art_wip/<character>_<clip>_preview.gif、[リンク一覧](art_wip/phase5b_previews.md)とcontact画像も更新。

Unityのシーン／.asset／.meta／実行ファイルは5Bで再生成していない。PNG/JSONとソース更新済み、最初にBuildAllが必要。workspace-write／承認不可でUnity IPCへ接続できないためAGENTS.mdとHANDOFF §2に従って外部担当へ引き継ぐ。

中割りはローカル処理で、姿勢差が大きい部分は近いキーポーズへ切り替える。原画由来の顔・衣装・ポーズ差、144Hzの快適さ、対人難易度・キャラ相性・音は実機待ち。UnitySetUpの入力分離／UnityTearDown復元を維持。

フェーズ6の残りは背景／カメラ最終調整、操作表示整理、PS表記／キーコンフィグ、必須24項目棚卸し・リリースビルド。サポート／オンライン／正式音源は未実装。詳細は IMPLEMENTATION_STATUS.md、BALANCE_CONFIG.md、docs/CPU_BALANCE_REPORT.md。
