# ART2 指示書：透真・蓮のドット絵スプライト＋PlayMode残課題（Claude → Codex）

## Claudeによる ART1 検証結果（Unity実機）
- **BuildAll が例外で失敗していた**：`AnimationSetBuilder.Import` が、初回インポート時（既定のNPOTスケール）に縮小されたテクスチャの幅で寸法チェックしていた（672px → 縮小）。**Claudeが修正済み**：`importer.GetSourceTextureWidthAndHeight` で元PNGの寸法を検査（`AnimationSetBuilder.cs`）。以後この方式を維持すること。
- 修正後：BuildAll 成功、EditMode **182/182**、Windowsビルド成功。
- 実機（ビルドに本物のキー入力）：千鶴の待機・歩き・ジャブ・ジャンプ・爆弾おにぎり（飛翔→命中→もぐもぐ表示）・フライパン突撃が絵として再生され、ハート減少も確認。スクショ：`docs/screenshots/art1_*.png`。
- **PlayModeの入力基盤の根本原因（Claudeが修正済み）**：Unity Test Framework は **`[UnitySetUp]` を `[SetUp]` より先に実行**するため、`InputTestFixture` の分離・フォーカス設定がキーボード追加より後になっていた（さらに override に `[SetUp]` 属性が無く基底の自動実行もされていなかった）。`BattleSceneTests` は `Setup/TearDown` を空の override にし、`IsolateInput()` を `[UnitySetUp]` 冒頭、`RestoreInput()` を `[UnityTearDown]` 末尾で明示的に呼ぶ形にした。**この構造を維持**すること。
- 現在の PlayMode：**4/10 合格**。残り6件は入力基盤ではなくテスト内容の問題（結果：`TestResults/art1_PlayMode.xml`）：
  1. `ChizuruSpritesFollowMovementFacingAndActiveJab`（:192）`Expected "walk_*" But was "idle_00"`
  2. `CurrentKeyboardMovesBothPlayersWithoutExplicitAssignment`（:146）
  3. `FinishedMatchFunctionKeysSelectCharactersAndRUpdatesViews`（:225）`Expected "蓮" But was "透真"`
  4. `PhaseTwoKeysChangeDefenseStateAndHudShowsLinkGauge`（:131）`Expected Guard But was Recovery`
  5. `Spec19_AllThreeCharactersMoveAndUseNormalsAndSpecialsOnBothPlayerSlots`（:164）`Expected > -5 But was -5`
  6. `TwoKeyboardPlayersMoveIndependentlyAndRRestartsFinishedMatch`（:89）「投入したP1入力がInput Systemへ届いていません」
  - ヒント：`InputTestFixture` の `Press()` 後、入力が `InputController.Capture()`（Update）→ 次の60Hz tick に届くまで数フレームかかる。`InputSystem.Update()` や `yield return null` の回数、`ProcessEventsInDynamicUpdate` の扱い、Rキー再戦など**フレーム境界の待ち方**を見直す。`currentキーボード` テストは `AssignKeyboard` で上書きされている点にも注意。ゲーム側の不具合と判明したらゲームを直す。**アサーションを弱めて通すのは禁止**。

## 1. 透真のスプライト（ART1と同じパイプライン・同じプロンプト規約）
- 参考：`docs/art_reference/characters/toma_sheet.webp`（正式設定画）、`toma_keyvisual.webp`。
- 外見：**乱れた黒髪、青い目、黒いスーツのジャケット（前開き）、白いシャツ、ゆるめた紺のネクタイ、黒いスラックス、黒い革靴、オレンジのライトが付いた頑丈な黒いタブレット端末**。落ち着いた・鋭い表情の青年。**約7頭身、目標身長 ≈ 124px**（千鶴より高い）。
- SF的な戦闘表現（端末から出る青〜オレンジの光、ホログラム状の解析線）は**本作独自のクロスオーバー演出**。原作の超能力として描かない。
- 技に合わせたポーズ：
  - 通常攻撃：コンパクトな打撃（ジャブ→肘→回し蹴りなど、スーツで動ける範囲の格闘）
  - 観測パルス：端末をかざして光のパルスを発射（エフェクトは別スプライト：パルス弾）
  - 遮断ステップ：低い姿勢の高速ステップ（残像は別スプライト or 色処理でよい）
  - 緊急上昇：ジャケットをなびかせて跳び上がる
  - 防御・妨害：端末を前に構えて遮断フィールド展開（エフェクト別スプライト：フィールド）
  - 境界突破（超必殺）：端末の光をまとって前方へ突進
  - 勝利：ネクタイを直す／端末を閉じる など。敗北：膝をつく。
- カットイン：`toma_keyvisual.webp` をもとに作成（Assets へは縮小コピー。出典記録）。

## 2. 蓮のスプライト
- 参考：`docs/art_reference/characters/ren_dog_rook.png`。
- 外見：**茶色の犬耳フード（耳の垂れた犬の顔付きフード）、焦げ茶の髪、青緑の目、八重歯、白い上着＋赤いスカーフ（骨のマーク・肉球ブローチ）、赤と青緑の縁取りのマント、黒の半ズボン、肉球ミトン、赤い肉球スニーカー、ふさふさの尻尾**。元気で純粋無垢な少年。**ちびにしすぎず約4.5〜5頭身、目標身長 ≈ 100px**（3人の中で最小）。
- マント・尻尾・フードの耳はアニメで揺らす。
- 技に合わせたポーズ（飛車＝縦横の直線移動のイメージ）：
  - 通常攻撃：肉球パンチ連打→フィニッシュの飛び蹴り
  - 飛車ライン：前方へ一直線の突き（マントが後ろへ伸びる）
  - 直線番長：水平に一直線の高速突進（体当たり）
  - 縦抜け：真上へ一直線に跳ぶ
  - 構え・急襲：低く構える→前方へ急襲（**カウンターの構えと絵で区別**できること）
  - 十字突破（超必殺）：縦横十字に駆け抜ける（十字の光は別スプライト）
  - 勝利：両手を上げて喜ぶ。敗北：しょんぼり座り込む。
- カットイン：既存の `ren-dog-rook.png` 由来のものを継続使用。

## 3. 共通
- 各キャラ ART1 と同じ23アニメ＋必要なエフェクト。品質が足りないフレームは作り直す（別人に見える・体格が揃わない・フレームが重なる）。
- 3キャラを並べた確認画像 `docs/art_wip/all_characters_lineup.png`（idle 1フレーム目を同じ縮尺で横並び）を出力し、**身長差が意図どおり**か確認。
- Hurtbox は見た目に合わせて微調整してよいが、3キャラの有利不利が極端にならないこと。変更値は `BALANCE_CONFIG.md` に記録。
- 頭上カウンターの位置（headHeights）を各キャラで正しく。
- 仮図形フォールバックは残す（将来キャラ用）。

## テスト
- 3キャラすべてで「全アニメのSpriteが存在」「状態→アニメ対応」「ヒットストップで停止」「向き反転」。
- PlayMode：上記6件を正しく直して **10/10**（Claudeが再検証）。

## 報告
生成枚数／作り直し枚数、lineup画像と contact画像のパス、残課題、Claudeに Unity で確認してほしい点。git commit はしない。
