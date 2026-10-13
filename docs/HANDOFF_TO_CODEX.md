# 引き継ぎ書：Claude → Codex（Codex単独で開発を続けるための手順）

> 2026-10-12 ユーザー指定：**Codexが実装・レビュー・Unity検証・取りまとめを担当し、Claudeにも随時交代できる**ようにする。
> 読む順序は `AGENTS.md` → 本書 → `docs/RESUME.md` → `IMPLEMENTATION_STATUS.md`。
> **6Bは完了済み**。保存済みUnity結果とGitHub mainを確認した最新情報は§4。旧資料の「Claude待ち／Codexはcommitしない」は過去の分担であり、現在の指定ではない。

---

## 0. 役割の変更（重要）
- これまで：Codex＝実装、Claude＝指示書作成・レビュー・**Unity実機検証**・ユーザー報告。
- これから：**Codex がすべて行う**。つまり
  1. 次の作業の指示書を自分で書く（`docs/codex_tasks/` に追記。下の「残りフェーズ」を元に）
  2. 実装する
  3. **Unityで実際に検証する**（下の §2。.NET代替テストだけで「完了」としない）
  4. 自分の差分をレビューする（§3 のチェックリスト）
  5. `IMPLEMENTATION_STATUS.md` と `docs/RESUME.md` を更新し、ユーザーに日本語で短く報告する
- 優先順位（原本仕様16章）：**遊べること ＞ 戦闘の気持ちよさ ＞ ルールの正確性と安定性 ＞ UI ＞ キャラ演出 ＞ 背景**。
- 仕様に反する未完成状態を完成と偽らない。テストのアサーションを弱めて通さない。
- Claudeへ交代しても同じ指示書・証跡・再開メモを使う。開始時にtasklistでcodex.exeを確認し、同じフォルダで別の担当が作業中なら編集を待つ。同時編集は禁止。

## 1. 絶対ルール（AGENTS.md より再掲）
- 書き込みは `anianiバトル！！` フォルダ配下のみ。他のゲームのフォルダは読み取り専用。
- Git：公開リポジトリ `https://github.com/donadonaa24-cyber/aniani-battle`（`main`）。**Unity検証が全合格した区切りで commit・push してよい**（2026-10-12 ユーザー許可）。force push・履歴書き換え禁止。秘密情報・個人情報（メール、PCユーザー名入りの絶対パス）を書かない。
- 秘密情報を書かない、外部送信しない、有料・出典不明アセットを入れない。
- `docs/art_reference/_style_local/` は他者のファンアート。画風の参考に見るだけ。Assets にコピーしない、キャラ・ポーズを写さない。
- 千鶴の正装は**私服**（茶色ボブ、オレンジのセーター、紺のフレアスカート、白スニーカー）。シェフ服は将来の覚醒モード用に温存。
- 透真はユーザー提供の設定画（`docs/art_reference/characters/toma_*.webp`）が正。SF装備の演出は本作独自表現として扱う。

## 2. Unity検証のやり方（Codexが自分で行う）
### 前提
- **Unity はサンドボックスの外で実行する必要がある**（サンドボックス内ではライセンスIPCに接続できず失敗する）。Codexアプリでコマンド実行の承認を求めるか、ユーザーにフルアクセスを許可してもらうこと。
- **`-nographics` は絶対に付けない**（Personalライセンスでは exit 198）。
- Unityエディタで `AniBato` を開いているとバッチ実行が失敗する。スクリプトが検出して止まるので、ユーザーにエディタを閉じてもらう。

### コマンド
```powershell
# BuildAll → EditMode → PlayMode → Windowsビルド → 失敗テストの要約
powershell -ExecutionPolicy Bypass -File tools\verify\run_unity.ps1 -Tag <作業名>
# 一部だけ
powershell -ExecutionPolicy Bypass -File tools\verify\run_unity.ps1 -Tag <作業名> -Only playmode -Filter <テスト名>
powershell -ExecutionPolicy Bypass -File tools\verify\run_unity.ps1 -Tag <作業名> -Only summary
```
- 結果：`TestResults/<作業名>_EditMode.xml`、`_PlayMode.xml`、各 `.log`、ビルドは `Build/<作業名>/AniBato.exe`。
- 失敗時はログの `error CS` / `Exception:` を読む。BuildAll が失敗したらテストに進まない。

### 実機での見た目確認（任意・注意）
```powershell
powershell -ExecutionPolicy Bypass -File tools\verify\play_and_capture.ps1 -Tag <作業名> `
  -Steps "shot:start; hold:D:600; tap:J; wait:120; shot:jab; tap:K; wait:300; shot:special"
```
- 本物のキー入力をゲームに送り `docs/screenshots/<作業名>_*.png` に保存する。
- **ユーザーがPCを操作中だとキーが他のアプリに入る危険がある**。スクリプトはゲームが最前面でなければ中止するが、実行前にユーザーに「数十秒PCに触らないで」と伝えること。
- 撮った画像は自分で見て確認する（キャラの絵、頭上カウンター、HUD、エフェクト）。
- 起動時のキャラ組合せは `AniBato/Assets/_Project/Data/BattleSetup.asset` の `Player1Index`/`Player2Index`（0=透真、1=蓮、2=千鶴）。確認用に一時変更したら必ず元に戻す。

### これまでに判明した落とし穴（再発させない）
1. **Unity Test Framework は `[UnitySetUp]` を `[SetUp]` より先に実行する**。`BattleSceneTests` は `Setup()/TearDown()` を空の override にし、`IsolateInput()` を `[UnitySetUp]` の冒頭、`RestoreInput()` を `[UnityTearDown]` の末尾で呼ぶ構造。維持すること。
2. `InputTestFixture` のテスト用ランタイムは `Application.runInBackground` ではなく `runtime.runInBackground` / `runtime.focusState` を見る（`InputFixtureFocus.Configure` で設定済み）。
3. 新規PNGの初回インポートは既定でNPOT縮小されるため、寸法チェックは `importer.GetSourceTextureWidthAndHeight` を使う（`AnimationSetBuilder.cs`）。
4. `Object.GetInstanceID()` は Unity 6.6 でコンパイルエラー（obsolete）。使わない。
5. Windows PowerShell 5.1 は BOM無しUTF-8 の日本語リテラルを誤読する。`.ps1` は ASCII のみで書く。
6. **.NET代替コンパイルは asmdef の参照関係を検査しない**。新しい asmdef（例 `AniBato.Presentation`）を作ったら、それを使うテストasmdef（EditMode/PlayMode）の `references` に追加すること。ART2 で PlayModeTests に `AniBato.Presentation` が抜けて Unity で CS0012 になった。
7. **`EditorSceneManager.NewScene(…, NewSceneMode.Single)` は C# 変数からしか参照されていないアセットをアンロードする**（UnityEngine.Object が fake-null になり `if (obj)` が false）。フェーズ4で Title シーンが作られず Battle が上書きされた原因。判定は NewScene の前に行い、必要なアセットは NewScene の後に `AssetDatabase.LoadAssetAtPath` で読み直す（`ProjectBuilder.BuildScene`）。
8. BuildAll の成功ログだけで判断しない。生成されたファイル（`Assets/_Project/Scenes/*.unity` 等）が実在するか確認する。

## 3. 自己レビューのチェックリスト（Claudeが見ていた観点）
- 原本仕様（`docs/SPEC_ORIGINAL.md`）の確定ルールに反していないか：通常攻撃はガード不可／特殊はガード可・カウンターに勝つ／超必殺はハート2・通常ガード・通常回避不可・ジャストで防御可・1試合1回／被弾3でハート-1・蓄積は保持／同一コンボのハート減少は最大1（超必殺は例外、上限時は2/3で保持）／カウンター同tick優先／場外即敗北／勝敗優先順位（場外→ハート0→時間切れ）
- 多重ヒットしないか（攻撃インスタンスID＋ヒット済み集合）、描画FPSで結果が変わらないか
- 戦闘ロジックが `AniBato.Battle`（純C#）に閉じているか、数値がデータ化されているか、巨大ファイルになっていないか
- テストを弱めていないか、失敗を隠していないか
- 見た目：キャラが別人に見えない、足元が揃う、頭上カウンターが頭上にある、判定（F9表示）と絵が大きくずれない

## 4. 現在の状態：ART3Eの残り攻撃・前進/被弾後退（2026-10-13）

**最新（2026-10-14）：ART3E完了、保存処理復旧・GitHub反映も完了。** [再起動後の詳細](codex_logs/RESTART_CHECKPOINT.md)/RESUME冒頭を先に読む。保存ロック解消、欠落キット作成スクリプトを復元して採用原画のみのZIP処理に接続。721ファイル/38.75MiBの正式ZIPを照合、全90原画と旧ZIPを保持。ユーザー明示承認で未公開2097c99だけを77c526dへ作り直し、origin mainへ通常push成功、GitHubの同一SHAを確認。元コミットはローカル保全し、作業ファイル3169件を保持。公開済み182e970以前の履歴変更/全体リセット/force pushなし。反映記録は通常の追加コミットに保存する。

全通常/歩き/走り/投げは接続済み、BuildAll成功・Edit424/424・Play32/32・Windowsビルド成功、純C#375/375、素材895ファイル再生成一致・91PNG保持。今回は保存済み結果の確認だけで再実行していない。最新exe Build/art3e、原画90/採用19。実機撮影はキー送信前に中止しPNGなし、素材画像docs/art_wip/art3e_*。次は公開整理→ART3F急停止→ART3G強弱被弾画像/表示（純C#反動と実ハート消失フラグは完了）。以降の「未接続/未検証」は制作中の履歴。

停止後「再開して」で続行。最新は[RESUME](RESUME.md)冒頭と[再開時チェックポイント](codex_logs/art3e_checkpoint_20261013.md)。歩き/走り6クリップ48Fは接続済み。透真3段/蓮1段v13/千鶴1段v3は正規化合格・未接続、残り4攻撃/投げカバンが残る。前進0.18/回と被弾追加後退0.18/6tick、実ハート消失フラグを実装。実Unityの通常前進28/28・被弾後退21/21、BuildAll単独成功、純C#366/366。停止/再開時は保存済み証跡確認のみ。**全EditMode/PlayMode/Windowsビルド未実行、未commit/未push（HEAD main 182e970）**。ART3F急停止・ART3G強弱被弾も残る。原画79/採用14/全文条件/ローカル参考を保存、Claudeへ交代可能。同時編集なしを確認する。

以下の旧状態は制作途中の履歴。

ユーザー再開指示により続行。同時編集なし。歩き24枚と走り24Fを接続、6クリップ48Fの画素/接地/浮遊/頭/32色/GIF、旧回帰と91PNG一致合格。dash採用は透真v3/蓮v7/千鶴v2、全身/両腕/靴を拡大確認。次は通常9原画→全15クリップ102F→キット/Unity全4工程。今回Unity未実行・未コミット/未push。最新採用/手順はRESUME冒頭。以下の中断状態は履歴。

ユーザーがPCを切るため制作/検証を停止。次の再開指示まで続けない。最新の正確な状態は[RESUME](RESUME.md)冒頭と[中断引き継ぎ](codex_logs/art3e_checkpoint.md)。歩きの両腕/透真の中央の持ち手は修正して24枚へゲーム接続済み、素材検査/旧回帰/91PNG一致合格。走りの透真v3/千鶴v2は正規化合格だが未接続、蓮v4は端切れ/最後の腕を直す必要があり未採用（selection=null）。通常9原画は未着手。純C#317/317、APIコンパイル警告0/エラー0。今回Unity全4工程/実機撮影は未実施、未コミット/未push、HEAD main 182e970。原画39枚/全文プロンプト/採否/次の手順を保存済みで、Claude/Codexへそのまま交代可能。以下の「再制作中」は途中経過。

最新のユーザー所見で「脚はよいが、左右の腕が振り子になっていない」と指摘。以前の歩き完成判定を取り下げて3人の両腕を再制作する。肩を支点に左右逆位相、脚とも逆位相、通過で下ろす/中間角度を描く。透真の端末の腕も振る。歩き修正完了→走り（逆振り/蹴り出し/浮遊/反対足接地）→通常の順。動作参考のユーザー画像は絵柄/衣装をコピーせず、公開素材へ収録しない。原画/全文プロンプト/途中採用はwalk_attacks、最新手順はRESUME冒頭、Unity未実行。

[ART3_WALK_ATTACKS](codex_tasks/ART3_WALK_ATTACKS.md)に沿い、歩き8枚/人・計24枚を生成/接続し素材検査合格。追加指定で左右の支え脚/曲げ脚を交互にし、走りも歩き完成後に修正する。透真/蓮通常はパンチ→パンチ→キック、千鶴カバンは攻撃中だけ。最終対象15クリップ102F、全224×160/PPU50/立ち倍率/共通32色・既存Timing/ID/性能を維持。原画/全文プロンプト/採用/検査はart_source/art3_walk_attacks/、BuildArt3WalkAttacks.ps1（途中は-WalkOnly）。次は走り3人→通常3段→全検証/キット/記録。**今回のUnityは未実行、未コミット差分あり**。最新はRESUME冒頭、同時編集確認を先に行う。

### 前回完了：ART3D基本動作・地上ジャンプ予備、Unity全合格

- [ART3_ACTIONS](codex_tasks/ART3_ACTIONS.md)に沿って3人のジャンプ/fall/land/guard/dodge/dash、18クリップ93Fを新絵柄へ接続。224×160/PPU50/立ち倍率/共通32色、全51キーの頭部帯幅±2px以内・靴/関節の目視。採用15原画＋不採用4原画を保持。全106クリップ584F、既存ID/Timing/待機/しゃがみを維持。
- ユーザー指定の地上2tick浅いかがみ→同初速で踏切を実装。schema7/GroundJumpStartupFrames=2、FighterMode末尾15/AnimationState末尾34、landコマ0へ接続。着地8tickは浅いかがみ4tick→待機4tick、空中2段は即時。スタミナ予備開始で1度消費、中断時払い戻しなし、新行動バッファ保持。中断/足場消失/場外/ヒット停止も検査済み。
- **Unity全4工程合格：BuildAll成功、EditMode357/357・PlayMode30/30（失敗/スキップ0）、Windowsビルド成功**。TestResults/art3d_*、Edit/Play終了01:53:43/01:55:04 JST。実入力→BattleRunner→SpriteRendererで3人の予備/踏切/防御/回避/ダッシュも検査。初回のschema6固定期待と追加テストの入力待ちを修正して全再実行、アサーションは維持。
- 最新起動Build/art3d/AniBato.exe、保持art3d/art3c。実機撮影は事前声かけ後、最前面確保できずキー送信前に停止、PNG未保存。[素材比較/GIF](art_wip/art3d_previews.md)は実機スクショではない。[撮影の残課題](screenshots/art3d_previews.md)。再撮影前は必ず「PCに触らないで」と案内し安全チェックを維持。
- [RESUME](RESUME.md)冒頭・[art3d_final](codex_logs/art3d_final.md)から交代。原画/全文プロンプト/採否/共通倍率/実装PNGはart_source/art3_actions/。BuildArt3Actions.ps1 -RebuildCheck（検査だけ-VerifyOnly）、BuildSpriteGenerationKit.ps1で再現。665ファイル再生成一致、ZIP341ファイル照合合格。[詳細条件](SPRITE_GENERATION_GUIDE.md)§10とキットを別環境へ渡せる。
- 実装/素材コミット**52de881**（`Add fixed-size action sprites and two-tick jump anticipation`）をorigin mainへ通常push済み、HEAD/origin/main一致・cleanを確認後、この反映記録だけを追記。次は実機目視、walk/攻撃/勝敗など新絵柄化、6Bの物理パッド/144Hz等の手動確認。新指示書を先に作り、開始時tasklist/git statusで同時編集を防ぐ。

### ART3C完了時の履歴

- 現行指示書は[ART3_CROUCH](codex_tasks/ART3_CROUCH.md)。新待機を参照して同倍率の立ち/しゃがみ原画を各1枚生成し、立ち由来の倍率でしゃがみへ接続。全員224×160px/コマ・足元159・PPU50、8F/6fps、体/頭±2px・靴の下12行固定。新待機/その他PNG/Timing/スプライトIDを保持。
- **今回のUnity実行：BuildAll成功、EditMode326/326・PlayMode29/29、失敗/スキップ0、Windowsビルド成功**。TestResults/art3c_*。新しゃがみ接続・倍率・足/顔・ループの3ケースを追加、既存アサーション維持。素材24F＋既存待機24F回帰、425ファイル再生成一致、ZIP内84ファイル照合合格。
- 詳細な固定条件/容姿/プロンプト/加工/JSON/検査は[再制作指示書](SPRITE_GENERATION_GUIDE.md)。参照PNG・パレット・全文プロンプト・採用済み待機/しゃがみは[持ち出しZIP](../art_source/sprite_generation_kit.zip)。別環境でもこの一式を渡せる。
- 最新起動はBuild/art3c/AniBato.exe、保持ビルドはart3c/art3b。[プレビューと同倍率比較](art_wip/art3c_previews.md)は素材の確認。[実機撮影](screenshots/art3c_previews.md)は最前面確保に失敗し画像未保存、新しゃがみのWindows目視確認は残課題。
- [RESUME](RESUME.md)冒頭と[art3c_final](codex_logs/art3c_final.md)から再開。原画/最終プロンプト/倍率/8枚はart_source/art3_crouch/、再現はBuildArt3Crouch.ps1 -RebuildCheck、キットはBuildSpriteGenerationKit.ps1。コミット名`Add fixed-size crouch sprites and portable generation guide`、反映先origin main。tasklist/git statusで反映と同時編集なしを確認。
- 次は実機のしゃがみ確認またはwalk/dash/攻撃などの新絵柄化が候補。新たな指示書を先に作り、同セル/立ち基準倍率/共通32色を維持。実キー撮影前は必ず「PCに触らないで」と案内し、最前面チェックを無効にしない。
- 追加の大きさ確認：待機との頭部帯幅差0〜1px、靴の下8行の幅差0〜3px、等倍/3倍目視済み。測定領域と基準は指示書§3・proportion-check.json。CompareArt3Proportions.ps1を再生成とキット作成に組み込み、今後も頭/靴の比較を必須とした。追加後も425ファイル再生成一致。

### ART3B完了時の履歴

- 現行指示書は[ART3_BREATHING](codex_tasks/ART3_BREATHING.md)。ART3の透真・蓮・千鶴を採用して8F/6fpsの体呼吸をゲームへ接続。今後も[SPRITE_SPEC](SPRITE_SPEC.md)の224×160px/コマ・足元159・PPU50に固定する。体/頭±2px、靴は固定、旧rawと待機以外のPNGは保持。
- **今回のUnity実行：BuildAll成功、EditMode323/323・PlayMode29/29、失敗/スキップ0、Windowsビルド成功**。TestResults/art3b_*。新しいアセット回帰3ケース追加、既存アサーション維持。24Fの画素/接地検査と388ファイル再生成一致も合格。
- 最新起動はBuild/art3b/AniBato.exe。保持ビルドはart3b/phase6bの2つ。[実画面](screenshots/art3b_previews.md)で全3人の待機・接地・頭上数値を確認した。物理パッド/144Hz等の手動確認を今回済みとして扱わない。
- 引き継ぎは[RESUME](RESUME.md)冒頭と[art3b_final](codex_logs/art3b_final.md)。原画/プロンプト/個別PNG8枚はart_source/art3_idle/、再現はBuildArt3Idle.ps1。完了一式のコミット名は`Add fixed-size breathing idle sprites for three fighters`、反映先origin main。次は移動/攻撃などの新絵柄化が候補で、指示書を先に作る。

### 6B完了時の履歴

- ユーザーが「6bは終わったよ」と確認。保存済み証跡：BuildAll exit 0、**EditMode320/320・PlayMode29/29（失敗・スキップ0）**、Windowsビルド exit 0・Success。TestResults/phase6b_runner.txt、phase6b_EditMode.xml、phase6b_PlayMode.xml、phase6b_buildall.log、phase6b_build.logで確認した。
- 6B完了時はTitle/Battleの生成シーンとBuild/phase6b/AniBato.exeが実在。現在の最新ビルドは上のART3Cを参照。
- 実装コミット9560ed4（6A・6B）はGitHub origin mainへ反映済み。資料更新開始時の作業ツリーはclean。今回は保存済み結果を読み、Unityの再実行や実キー入力は行っていない。
- USB HID・機器の後始末・旧シーン停止の修正を含め、全PlayModeが合格。背景・中割り・カメラ・左右表記・ダッシュ設定・必須24テスト/12成果物表も実装済み。
- 比較画像はdocs/screenshots/phase6b_*.png（サムネ・オフライン投影図）。6B実機撮影・物理パッド2台・3比率/144Hzの手動所見・全14課題の通し確認・実SO810試合集計の記録は今回確認できていない。自動テストの合格と分ける。
- カメラの全体常時表示と常時拡大、idle目元/柄などは改善候補。完了した6Bを作り直さず、ユーザーの操作感に基づいて次の具体的な修正を選び、着手前にdocs/codex_tasks/へ指示書を書く。
- 6B完了記録67a8d9cもorigin mainへpush済み。後続ART3の3枚はART3Bで採用・接続済み。交代時はRESUME冒頭の作業名・差分・結果・次の手順から再開する。
## 5. 残りフェーズ（指示書は自分で `docs/codex_tasks/PHASE4.md` 等に書いてから着手）
以下は工程全体の計画履歴です。フェーズ1〜6Bは実装・Unity検証済み。現在の再開地点は§4とRESUMEに従い、完了済みの素材やフェーズを作り直さないでください。
### フェーズ4：ステージとゲーム進行（原本 6・8章）
- 3ステージ（STAGE 01 星の終わり：近未来都市・青紺・電子光／STAGE 02 推し駒Battle：6×6盤面の闘技場・巨大な駒／STAGE 03 Battle à la carte：文化祭の調理場）。**地形・当たり判定は3つとも同一**（既存 `StageGeometry`）、背景・色・床の見た目だけ変える。背景は自作（画像生成可、出典記録）。
- **ステージ縮小**：残り10秒（開始60秒）で警告 → 左右の床が段階的に消え、最後は中央の小足場のみ。全ステージ共通の `StageShrinkController`（シミュレーション側で決定的に）、背景演出は分離（星の終わり＝AI警告、推し駒＝外側のマス消失、à la carte＝文化祭終了）。キャラの立ち位置を考慮し突然消さない。
- 画面：タイトル／メインメニュー／キャラ選択（3人、P1/P2、CPU or 2P）／ステージ選択／操作説明／ポーズ（Esc）／リザルト（勝者・再戦・キャラ選択へ戻る）。
- テスト：必須テスト17（残り10秒で縮小開始）、18（残り0秒の勝敗）、22（リザルトから再戦）。
### フェーズ5：CPUと演出（原本 8・9章）
- CPU：やさしい／ふつう／むずかしい（反応遅延・行動確率・ガード/回避/カウンター使用率をデータ化）。歩行・ジャンプ・通常・特殊・ガード・回避・カウンターを状況で使う。場外へ自滅しない、縮小時は中央へ寄る。CPUは `FrameInput` を生成するだけ（シミュレーションは共通）。
- ゲームパッド：P1/P2 割り当て、操作説明画面に表示。
- 効果音（手続き生成でよい）、ヒット・ガード・ジャスト・BREAK・COMBO FINISH・超必殺・試合終了の演出。
- **CPU同士の自動対戦テスト**で、70秒中に超必殺が実際に撃てるか（ゲージ量）を確認し `BALANCE_CONFIG.md` を調整。
- テスト：必須テスト20（CPU戦成立）、21（ローカル2人対戦）。
### フェーズ6：検証と仕上げ（原本 11・12・15章）
- 必須テスト1〜24を一覧化し、自動テスト／実機確認のどちらで確認したかを `IMPLEMENTATION_STATUS.md` に表で記録。
- 最終Windowsビルド（`Build/Release/`）、`README.md`（起動方法・操作）、`GAME_DESIGN.md`（確定／暫定ルール）、`BALANCE_CONFIG.md`、`IMPLEMENTATION_STATUS.md`（実装済み／未実装／既知の問題）、`docs/ARCHITECTURE.md`（将来拡張：キャラ追加・サポート・ステージ差分）を仕上げる。
- 完成後、commit・push してユーザーに報告する。

## 6. 各作業の終わりにやること
1. `tools\verify\run_unity.ps1` で全検証（失敗があれば直してから）
2. `IMPLEMENTATION_STATUS.md` と `docs/RESUME.md`（何が終わり、次に何をするか）を更新
3. ユーザーへ日本語で：やったこと／Unityでの検証結果（件数）／スクショの場所／残課題／次にやること
