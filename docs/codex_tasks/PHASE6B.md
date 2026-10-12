# PHASE 6B 指示書：仕上げ（見た目・カメラ・最終テスト・資料）（Claude → Codex）

## Claudeによるフェーズ6A検証結果（Unity実機）
- BuildAll成功、EditMode **305/305**、Windowsビルド成功（`Build/phase6a/`。古いビルドは保持ルールで自動削除）。
- PlayMode **24/26**。失敗2件（`TestResults/phase6a_PlayMode.xml` / `.log`）：
  1. `GameFlowSceneTests.ActualDualSenseAndXboxDevicesSwitchAllActionLabels`：`/DualSenseGamepadHID/buttonSouth の入力イベントが反映されませんでした`（`WaitFor`、:81）。テスト用の DualSense HID デバイスへの `Press` が届いていない。HID デバイスの状態形式・`canRunInBackground`・`InputTestFixture` での扱いを調べる。どうしても HID レイアウトで押下を再現できない場合は、**DualSense として認識されるレイアウト（`DualSenseGamepadHID` を継承するテスト用デバイス等）で表記判定ロジックそのものを検証**し、入力の到達は別のテストで保証する。アサーション（表記が × □ OPTIONS ／ A X ／ Space に切り替わる）は弱めない。
  2. `GameFlowSceneTests.CpuSelectionKeepsDifficultyAndIgnoresPlayerTwoBattleInput`：SetUp で Input System の内部エラー `Cached unprocessed value unexpectedly became outdated ... new value '-1' old value '-0.003921568'`（`InputController.ReadMenu` :170 の `leftStick.ReadValue()` 経由）。**直前のテストで追加した DualSense/Xbox デバイスが削除されずに残っている**ことが原因と推定（-0.0039 は HID スティックの中立値）。テストで追加したデバイスは必ず `try/finally` または `[UnityTearDown]` で削除する。ゲーム側でも、テスト以外で同じエラーが出ないか（実機の DualSense で `ReadValue` を毎フレーム読む箇所）を確認し、必要なら `ReadValue` の呼び出しを1フレーム1回にまとめる等で回避する。
- この2件を直して **PlayMode 全合格** が最優先。

## 1. 見た目（`docs/REVIEW_NOTES.md` の未対応項目）
1. **背景とステージ選択のサムネ**：暗く情報量が少ない。特に推し駒Battle が「6×6の盤面の闘技場」に見えない。3ステージとも、キャラの視認性を保ったまま作品が分かる程度に明るく描き込む（画像生成可。遠景・中景・床のレイヤー。彩度・明度はキャラより控えめ）。縮小演出（AI警告／盤面の外側のマスが消える／文化祭終了）が背景から分かること。
2. **カメラ**：上部の小足場が画面端で切れている／キャラが画面高の約18%で小さい。ステージ全体（両端の場外ラインの少し内側まで）が収まりつつ、キャラが今より大きく見える寄り具合・追従を調整。縮小後は中央へ寄る。1920×1080 と 16:10・4:3 で確認できる設定に。
3. ステージ選択の操作説明の表記（「W/S 選択」だが並びは横）を実際の操作に合わせる。他の画面も表記と操作が一致しているか総点検。
4. **中割り**：Phase5B のブレンド中割りは 6A で廃止済み。主要アニメ（walk・dash）で動きが粗く見えるものは、**画像生成で**中間コマを描いて追加（重ね合わせ禁止）。コマ数より絵の一貫性を優先。
5. （余力があれば）透真の目元が暗い（青い目が見えない）・蓮の模様が細かすぎる点を、idle と主要アニメで改善（ユーザーは後回しと判断済み。優先度は低い）。

## 2. 操作の小改善（`docs/CONTROLS_REVIEW.md` の提案のうち採用分）
- 設定に「**左右2回入力のダッシュ ON/OFF**」を追加（既定 ON。OFF でも専用ボタンでダッシュ可能）。キーコンフィグ・操作説明・チュートリアルの表記に反映。
- 他の提案（初心者プリセット、防御3種の図解など）は実装しない（ユーザーの実機の感想待ち）。

## 3. 最終テスト・ビルド・資料（原本 11・12・15章）
- **必須テスト1〜24の対応表**を `IMPLEMENTATION_STATUS.md` に作る。各項目について「自動テスト名／実機確認／ユーザー決定による変更（旧→新）」を記載。特に 7（通常攻撃はガードを通過）・13（超必殺は通常ガードで防げない）は**ユーザー決定（2026-10-12）で変更**されたので、新ルール（ガードは全攻撃を防ぐがスタミナ消費、しゃがみ攻撃はガード崩し、スタミナ不足で超必殺は貫通）のテストで置き換えたことを明記。
- `README.md`（起動方法・操作・練習モード・チュートリアル）、`GAME_DESIGN.md`（確定ルール／ユーザー決定による変更／暫定ルール）、`BALANCE_CONFIG.md`、`docs/ARCHITECTURE.md`（将来拡張：キャラ追加の手順、サポートキャラ、ステージの時間帯差分、オンラインに向けた入力と戦闘の分離）を最新に。
- 実装済み／未実装の一覧（原本15章の成果物12項目に対して）を `IMPLEMENTATION_STATUS.md` に。

## 検証・コミット
- Unity検証は Claude（不在時は `docs/HANDOFF_TO_CODEX.md` §2）。新しい asmdef はテスト asmdef の references に追加。
- Claude が Unity 全合格を確認したら、Claude が commit・push する（Codex は commit しない）。
- 最終報告：変更概要、背景・カメラの before/after スクショ（`docs/screenshots/phase6b_*.png`。Unity で撮れない場合は生成画像やサムネで代替し、その旨を明記）、PlayMode 修正の原因、必須テスト対応表の要約、Claude に実機で見てほしい点。
