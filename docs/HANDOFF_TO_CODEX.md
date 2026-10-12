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

## 4. 現在の状態：6B完了・担当交代の記録更新

- ユーザーが「6bは終わったよ」と確認。保存済み証跡：BuildAll exit 0、**EditMode320/320・PlayMode29/29（失敗・スキップ0）**、Windowsビルド exit 0・Success。TestResults/phase6b_runner.txt、phase6b_EditMode.xml、phase6b_PlayMode.xml、phase6b_buildall.log、phase6b_build.logで確認した。
- Title/Battleの生成シーンとBuild/phase6b/AniBato.exeが実在。最新2ビルドはphase6a・phase6b。
- 実装コミット9560ed4（6A・6B）はGitHub origin mainへ反映済み。資料更新開始時の作業ツリーはclean。今回は保存済み結果を読み、Unityの再実行や実キー入力は行っていない。
- USB HID・機器の後始末・旧シーン停止の修正を含め、全PlayModeが合格。背景・中割り・カメラ・左右表記・ダッシュ設定・必須24テスト/12成果物表も実装済み。
- 比較画像はdocs/screenshots/phase6b_*.png（サムネ・オフライン投影図）。6B実機撮影・物理パッド2台・3比率/144Hzの手動所見・全14課題の通し確認・実SO810試合集計の記録は今回確認できていない。自動テストの合格と分ける。
- カメラの全体常時表示と常時拡大、idle目元/柄などは改善候補。完了した6Bを作り直さず、ユーザーの操作感に基づいて次の具体的な修正を選び、着手前にdocs/codex_tasks/へ指示書を書く。
- 現在の記録作業はcodex_tasks/PHASE6B_CLOSEOUT.md。交代時はRESUME冒頭の作業名・差分・結果・次の手順から再開する。
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
