# PHASE 1 指示書：戦闘基盤（Claude → Codex）

## 前提
- `AniBato/` は Claude が作成済みの空のUnity 6000.6.0f1プロジェクト。パッケージ追加済み：ugui 2.6.0、inputsystem 1.20.0、test-framework 1.8.0、2d.sprite 1.0.0。
- `ProjectSettings` の `activeInputHandler` は現在 0（旧Inputのみ）。Input System を使うため **2（Both）** に変更すること。
- 製品名「あにあにバトル！！」、companyName `aniani`、デフォルト解像度 1920x1080、ウィンドウ/フルスクリーン切替可。
- `AGENTS.md` の技術方針を厳守。

## このフェーズのゴール
**キーボード2人で実際に操作して対戦でき、勝敗がつく状態。** 加えて、フェーズ2（特殊攻撃・ガード・回避・カウンター・ジャスト・コンボ・場外）を書き換えなしで載せられる骨格を作る。

## 実装内容
1. **フォルダ・asmdef**
   - `Assets/_Project/Scripts/Battle`（`AniBato.Battle`、UnityEngine参照は Mathf/Vector2 程度まで。できれば System.Numerics 不使用の自前 `Fix`/float構造体でよい）
   - `Assets/_Project/Scripts/Game`（`AniBato.Game`：MonoBehaviour群）
   - `Assets/_Project/Scripts/Editor`（`AniBato.EditorTools`）
   - `Assets/_Project/Data`（ScriptableObjectアセット）
   - `Assets/_Project/Tests/EditMode`、`Assets/_Project/Tests/PlayMode`
2. **シミュレーション（純C#）**
   - `BattleSimulation`：2人の `FighterState`、`MatchState`（tick数、残り時間、結果）、`StageGeometry` を保持。`Tick(in FrameInput p1, in FrameInput p2)`。
   - `FrameInput`：方向(x,y)、各ボタンの Held / Pressed（Jump, Attack, Special, Guard, Dodge, Counter, Ultimate, Dash）。入力バッファ（数フレーム）を考慮。
   - `FighterStateMachine`：状態 Idle / Move / Dash / Jump / Fall / Attack / Special / Guard / Dodge / Counter / Hitstun / Knockback / Recovery / KO を enum で定義（フェーズ1で未使用のものも定義だけ）。
   - 移動：左右移動、高速ダッシュ（地上・2回タップ or ダッシュボタン）、ジャンプ、2段ジャンプ、空中ダッシュ、急降下（空中で下）。**ステージ端から端までダッシュで約2〜3秒**。
   - 通常攻撃：地上・空中。`AttackData`（発生/持続/硬直フレーム、ヒットボックス矩形、ヒットストップ、ノックバック、`IsNormal`/`IsSpecial`/`IsUltimate` 区分、`IsCloseRange`）。最大3段の連打可能な構造。
   - `HitResolver` / `DamageSystem`：攻撃インスタンスID＋ヒット済み集合で多重ヒット防止。通常攻撃ヒットで被弾カウント+1、3でハート-1→0/3。被弾カウントは時間・移動・ジャンプ・ガードで回復しない。
   - `ComboTracker` の骨格（連続判定ウィンドウをデータ化）。フェーズ1ではヒット数カウントと「同一コンボ中のハート減少は最大1」のフックまで。
   - 勝敗：ハート0で KO。**70秒**（60Hz×70=4200tick）タイマー、時間切れ判定（ハート多→被弾蓄積少→引き分け）。同tick同時発生時の優先順位：場外 → ハート0 → 時間切れ（場外はフェーズ2だが優先順位の実装はここで）。両者同時KOは引き分け。
   - イベント出力：`BattleEvent`（Hit, HeartLost, Break, KO, TimeUp…）のリストを tick ごとに出し、UI/演出側が購読する。
3. **ステージ地形（共通）**：メイン足場1つ＋上部の小足場2つ（下から通り抜け可・下入力で降りられる）＋左右・下の場外ライン。`StageGeometry` はデータ化。
4. **Unity側**
   - `BattleRunner`（MonoBehaviour）：固定60Hzでシミュレーションを進める（`FixedUpdate` で `Time.fixedDeltaTime=1/60` か自前アキュムレータ。描画FPSに依存しないこと）。
   - `InputController`：Input System。P1 = A/D/W/S、Space ジャンプ、J 通常、K 特殊、L ガード、Left Shift 回避、I カウンター、U 超必殺、ダッシュはダブルタップ＋（任意で）専用キー。**P2 キー配置も定義**（例：矢印キー＋テンキー 1/2/3/4/5/6/0…など、P1と衝突しない配置。キー配置はScriptableObjectか定数1箇所で変更可能に）。ゲームパッドは各デバイスをP1/P2へ割り当て可能な構造にし、フェーズ1で動けばなお良い。
   - 描画：仮Sprite（コード生成テクスチャでよい）。キャラ2体は色・形で区別（仮キャラA/B）。向きの反転、状態に応じた簡易な見た目変化（攻撃時に腕ボックス、被弾時点滅など）。
   - **頭上カウンター** `0/3` 表示：キャラに追従（ワールド→スクリーン変換 or ワールド空間Canvas）。1/3で小さく弾む、2/3で黄〜オレンジ、3到達で「BREAK!」演出、その後0/3。
   - HUD（画面上部）：両者のキャラ名、ハート3個、被弾カウント、残り時間。
   - カメラ：両者を収める簡易追従＋ズーム（範囲制限）。
   - 試合終了表示（勝者／DRAW）と R キーで再戦（最低限）。
5. **シーン生成**：`AniBato.EditorTools.ProjectBuilder.BuildAll`（`-executeMethod` 用）で、ScriptableObjectアセット生成＋`Assets/_Project/Scenes/Battle.unity` 生成＋Build Settings登録。手書きYAML禁止。
6. **テスト（EditMode、シミュレーション直接駆動）**
   - 通常攻撃1回で1/3、2回で2/3、3回でハート-1かつ0/3
   - 時間を空けても蓄積保持
   - 同一攻撃インスタンスが複数tick重なっても1回しか数えない
   - 70秒で時間切れ、時間切れの勝敗判定（3パターン）
   - 同tickでハート0と時間切れが起きた場合の優先順位
   - 描画フレームレートに依存しないこと（例：同じ入力列を異なるUpdate分割で流しても同結果、の考え方をRunner側ユーティリティで確認できれば尚良）
7. **ドキュメント**：`README.md`（起動方法、開発環境、P1/P2操作表）、`IMPLEMENTATION_STATUS.md`、`BALANCE_CONFIG.md`（初期値と意味）、`docs/ARCHITECTURE.md`（シミュレーション／ビュー分離、tick、イベント）、`GAME_DESIGN.md`（確定ルールと暫定ルールを区別）。すべて日本語。

## 初期値の目安（暫定、BALANCE_CONFIG に記録）
- 走行 9〜10 u/s、ダッシュ 16〜18 u/s、ステージ主足場幅 ≈ 36u（ダッシュで端から端 ≈ 2.2s）
- ジャンプ初速・重力は「キビキビ」寄り（滞空約0.5〜0.6s）
- 通常攻撃：発生4F・持続3F・硬直8F前後、ヒットストップ4F
- コンボ連続判定ウィンドウ：前ヒットから 40F 以内

## 検証
- 可能ならUnityバッチモードで `BuildAll` 実行 → EditModeテスト実行し、結果を `TestResults/` に保存。
- Unityが起動できない場合は、`dotnet` 等での代替コンパイルを試みてよいが、最終報告に「Unity未検証」と明記。

## 完了報告に含めること
変更ファイル一覧の要約、テスト結果、Unity実行可否、未実装／既知の問題、フェーズ2に向けた懸念。
