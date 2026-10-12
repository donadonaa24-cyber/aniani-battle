# PHASE 5B 指示書：滑らかさ（カクつき解消）と試合テンポのバランス（Claude → Codex）

ユーザーの実機プレイ（PS5コントローラー、144Hzモニター）での感想：「操作感は悪くないが、カクつく。理想にはまだ遠い」。
Claudeの CPU集計レビュー：**810試合すべて 4〜7秒で KO 決着**。70秒・最後10秒の縮小という原本の中心設計が機能していない。
この2点をフェーズ6（仕上げ）より先に直す。

## Claudeによるフェーズ5検証結果（Unity実機）
- BuildAll成功、EditMode **225/225**、Windowsビルド成功。
- PlayMode **15/20（フェーズ4の16/17から後退）**。結果 `TestResults/phase5_PlayMode.xml` / `.log`。
  1. `GamepadsNavigateBothCharacterSlotsAndStartPausesBattle`（:238）`Expected StageSelect But was Countdown` → **持ち越し不具合が直っていない**
  2. `HeldConfirmAndCancelRequireAllPlayersToReleaseAcrossScreens`（:186）`Expected Pause But was Battle`
  3. `PauseAndItsControlsFreezeTicksAndResumeDoesNotInjectAttack`（:122）`Expected Pause But was Battle` → **Esc/Startでポーズに入れない回帰の疑い（重大）**
  4. `ReconnectingPadsKeepOriginalSlotsAndNeverControlBothPlayers`（:205）`Expected same as <Gamepad:/Gamepad> But was <Gamepad:/Gamepad1>`
  5. `ResultRematchResetsEntireMatchAndKeepsSelection`（:140）`Expected Countdown But was Result` → **リザルトから再戦できない回帰の疑い（重大）**
- 推定：フェーズ5で入れた「離すまで無効」ガードが、ポーズ（Esc/Start）や再戦の決定まで塞いでいる、または遷移直後のフレームでガード解除判定が正しく行われていない。

## 0. 最優先：上記 PlayMode 5件（回帰）を直す
- ゲーム側の不具合として原因を特定し修正する。テストのアサーションは弱めない。
- 「離すまで無効」ガードの設計を見直す：**遷移を起こしたボタン（そのボタン・そのデバイス）だけ**を、離されるまで次画面で無効にする。他のボタン（Esc／Start によるポーズ等）は即座に有効。
- `wasPressedThisFrame` を同じフレーム内で複数の画面処理が読む構造になっていないか、`InputSystem` の更新タイミング（`ProcessEventsInDynamicUpdate`）と `Update` の実行順で同じ押下が2フレーム見えていないか、を確認。
- 修正後、キーボード・パッド両方で：キャラ確定→ステージ選択で止まる／Escでポーズ／ポーズから再開で攻撃が漏れない／リザルトから再戦できる、をPlayModeで確認。

## A. カクつき解消（`docs/REVIEW_NOTES.md` の「カクつく」節を必ず読む）
原因（Claude調査済み）：
1. **描画が tick 間を補間していない**。ユーザーのモニターは144Hz、シミュレーションは60Hz。`FighterView` が `fighter.Position` をそのまま代入しているため、1 tick の位置が2〜3フレームずつ不均一に表示される（ジャダー）。`FixedTickClock.Fraction` は未使用。
2. **アニメのコマ数が少ない**（攻撃3〜4コマ/10fps、落下2コマ/8fps、ガード2コマ/6fps）。
3. **AI生成のコマ間で体格・輪郭・位置が微妙にずれる（ボイリング）**。特に idle で止まっていても震えて見える。

実装：
1. **描画補間**：シミュレーションは一切変えない（決定性維持）。各ビュー（キャラ・弾・エフェクト・頭上カウンター・HUD以外のワールドUI・カメラの追従目標）が「前tickの状態」と「現tickの状態」を持ち、`Lerp(prev, curr, clock.Fraction)` で描く。
   - ヒットストップ中、ポーズ中、再戦・リセット・場外後の位置ジャンプ、向き反転、アニメのコマ選択は補間しない（コマ選択は現tick基準）。
   - `BattleRunner` が tick 前後のスナップショットをビューへ渡す仕組みにする。
2. **フレームペーシング**：既定は VSync ON（`QualitySettings.vSyncCount=1`、`Application.targetFrameRate=-1`）。オプション画面（ポーズ／メニュー）に「フレームレート：モニター同期／60／120／144／無制限」。`FixedTickClock` の追いつき処理が1フレームに大量の tick を流さないよう上限（例 5 tick、超過分は時間を捨てずに次フレームへ持ち越す or 明示的にスローダウン）を設ける。
3. **計測テスト**：PlayModeで、一定速度で移動するキャラの描画位置のフレーム間差分が「144Hz相当のフレーム時間列」で単調・均一（ばらつきが小さい）ことを検証するテストを追加。補間なしでは失敗、補間ありで通ること。
4. **アニメの増量**：主要アニメ（idle, walk, dash, jump, fall, attack1〜3, air_attack, 各special の溜め〜発生〜戻り）を **6〜8コマ・12〜15fps** に。SpriteForge に「中割り生成」を追加：既存のキーフレーム2枚を参照して間のコマを画像生成し、同じ加工（減色・足元合わせ）を通す。全部を作り直さなくてよい。目立つもの（idle・walk・dash・attack1〜3）から優先。
5. **ボイリング対策**：SpriteForge に整合処理を追加：
   - アニメ内で頭頂・足元・体の中心（胴の重心）を揃える（現在の足元合わせに加え、idle は胴の中心も固定）。
   - パレットのコマ間ゆらぎを抑える（同じ部位の色が他の色にならないよう、キャラ共通パレットへ最近傍で寄せる）。
   - idle は「1枚の基準コマから、呼吸による上下1〜2pxの変化だけを作る」方式（腕・髪などの小さな差分のみ）に作り直す。各キャラの idle を最優先で作り直すこと。
   - 作り直したアニメは contact 画像に加え、**GIF or 連番プレビュー**（`docs/art_wip/<キャラ>_<アニメ>_preview.gif`）を出力して、目で確認できるようにする。

## B. 試合テンポ（原本 1章・3章・16章：70秒の超高速勝負、最後10秒の縮小）
目標（CPU同士 ふつう vs ふつう、全9組×seed30）：
- **平均決着 35〜55秒**、**時間切れ・縮小フェーズ（残り10秒以降）まで進む試合が 20〜40%**
- 超必殺：**半数前後の試合で少なくとも1回**撃たれる（現在はゲージが溜まりすぎ。命中14/被弾10 → 戻す方向）
- やさしい：初心者が数回は当てられる強さ。むずかしい：ジャスト防御・カウンターを狙う。
- 自滅（自分から場外）はほぼ0を維持。

調整の手段（**原本の確定ルールは変えない**：特殊命中＝ハート1、通常3ヒット＝ハート1、超必殺＝ハート2、ハート3）：
1. 特殊攻撃を当てにくく・隙を大きく：発生・空振り硬直・ガードされた時の硬直を増やす、技ごとの**再使用クールダウン**（データ化、例 1.0〜2.0秒）、飛び道具は画面内1発まで。
2. **ハートを失った直後の立て直し**：ハート減少時に短い無敵（例 60〜90F、点滅表示）＋両者の間合いをリセットする程度のノックバック。無敵中は攻撃しても相手の無敵は延長しない。原本の「同一コンボのハート減少は最大1」と矛盾しない。
3. CPU の防御：相手の特殊攻撃の発生モーション（予備動作）を見てガード／回避する確率を難易度別に。むやみに特殊を連打しない（クールダウンとリスクを考慮）。
4. ゲージ：試合が長くなる分、獲得量を下げる（目標の発動率になるよう再調整）。
5. 変更した数値はすべて `BALANCE_CONFIG.md` に before/after で記録。`docs/CPU_BALANCE_REPORT.md` を再集計（**ScriptableObject の実データで集計できるなら実データで**。できなければ純C#既定値と明記）。

## テスト
- 描画補間の計測テスト（A-3）、ヒットストップ・リセット時に補間しないテスト。
- ハート減少後の無敵・クールダウンのテスト。原本の必須テスト1〜18が引き続き通ること。
- CPU集計の目標値をテストとして固定（seed固定の小さめの集計で、平均決着時間の範囲などを確認）。

## 検証・報告
- Unity検証は Claude（不在時は `docs/HANDOFF_TO_CODEX.md` §2）。新しい asmdef を作ったらテスト asmdef の references に追加。
- `IMPLEMENTATION_STATUS.md`、`docs/RESUME.md`、`BALANCE_CONFIG.md`、`docs/CPU_BALANCE_REPORT.md` を更新。git commit はしない。
- 最終報告：集計の before/after、作り直したアニメと preview GIF の場所、Claudeに実機で見てほしい点。
