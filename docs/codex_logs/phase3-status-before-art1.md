# 実装状況：フェーズ3（透真・蓮・千鶴）

更新日：2026-10-12（JST）。対象指示書：`docs/codex_tasks/PHASE3.md`。原本：`docs/SPEC_ORIGINAL.md`。

**検証結果への修正とフェーズ3のコード実装・.NET代替検証を完了しました。今回のUnity BuildAll／EditMode／PlayMode／Windowsビルドは未実行です。Unity実動作の合格はClaudeの検証待ちです。** git commit／push／公開／既存ファイル削除は行っていません。

## 検証結果を受けた修正

1. `TestResults/phase2_EditMode.xml`の94/94と`phase2_PlayMode.xml`の2/6・4失敗を確認。PHASE3指示書ではBuildAll・Windowsビルド成功、実キーで移動・防御・BREAK・ゲージ正常と報告されています。
2. PlayModeを`InputTestFixture`継承へ変更し、TestFramework asmdef/DLL参照、隔離されたKeyboard、Press/Release、Fixtureのplayer-loop配送へ移行。IgnoreFocus・DynamicUpdate・runInBackgroundをテスト中だけ設定し、設定・デバイス・エディタ配送・ネイティブランタイムを復元。旧キー状態・P1/P2実移動・R再戦等のアサーションを維持し、全3キャラ操作と仮選択の2テストを追加。**修正後のバッチモード実行はUnity待ちです。**
3. LINK獲得量は維持。通常9命中、通常3連2回＋カウンター1回＋被弾1回などで100へ到達します。約24〜36秒という目安は仮定であり実測ではありません。全計算とフェーズ5のCPU実測・調整方針をBALANCE_CONFIGへ記載しました。

## 実装済み

- Unity非依存の純C#／60Hz／自前AABB／入力ラッチ／攻撃ID＋対象集合／既存攻防・コンボ・勝敗を維持。
- 透真・蓮・千鶴の移動、独立した地上通常3・空中通常3、特殊4枠・超必殺を`PhaseThreeDefaults`で定義し、CharacterData/AttackDataへ生成するコード。
- 透真：短射程の観測パルス・90F解析マーク、終盤だけ打撃する遮断ステップ、緊急上昇、敵弾消去フィールド、前方突進の境界突破、カウンター後180Fの移動/ダッシュ強化。SF装備は本作独自の表現。
- 蓮：長い飛車ライン、高速直線番長（ガードされると42Fの隙）、縦抜け、構えから新規通常/特殊入力で急襲（カウンターではない）、十字の和集合判定の十字突破、カウンター吹っ飛び2倍。
- 千鶴：放物線の爆弾おにぎり・30Fもぐもぐ麻痺、敵弾1回消去のフライパン突撃、1ヒットのおたま旋風、24F後に蓄積だけ-1する食事（4秒再使用制限・被弾中断）、広範囲の満腹カレー！、カウンターで蓄積-1。
- 汎用SkillSystem：最大45Fの麻痺、マーク、自己蓄積回復、移動/ダッシュ/発生倍率の期限付きバフ、吹っ飛び倍率、特殊クールダウン。アーマーなし。キャラ名で戦闘処理を分岐しません。
- 各FighterStateに独立したICounterEffect。CharacterDefinition.CounterからDataCounterEffectを生成。旧シミュレーション全体フックsetterは互換用に両者へ設定。
- 超必殺は100消費・2ハート・1試合1回、範囲外・空振り硬直・共通ジャスト相性を維持。UltimateStartedと30Fの固有カットイン。停止中は時計/物理/効果/バッファ停止・押下破棄、同時発動は共有、最後tickの時間切れを延期しません。
- BattleSetupと試合終了画面のF1/F2（P1）・F3/F4（P2）→R再戦。初期P1=透真/P2=蓮。実行中の選択はアセットへ保存しません。
- 仮シルエット・耳/コック帽・食事モーション、マーク/強化/もぐもぐ/構え表示、十字表示、キャラ色の弾。蓮/千鶴のユーザー作品画像2枚をカットインへ利用。元画像SHA-256一致、出典記録。
- BuildAllは旧22アセットを保持し、新37を追加して標準59アセットを生成するコードへ更新。既存調整・各新キャラ調整・BattleSetupを再実行で保持。SchemaVersion=2は維持（ルールに新規項目なし）。シーン/素材インポートはEditor API。
- 原作調査をCHARACTER_REFERENCEへ事実/本作独自解釈に分離し、README技表、BALANCE_CONFIG全数値・ゲージ想定、GAME_DESIGN、ARCHITECTURE、ASSET_SOURCES等を更新。

## 検証結果

| 項目 | 結果 | 証跡 |
| --- | --- | --- |
| 純C# EditModeソースの.NET代替実行 | **143成功、0失敗、0スキップ** | TestResults/phase3-dotnet-battle.xml・.log |
| Unity API参照でGame／Editor／EditMode／PlayModeを.NETコンパイル | **警告0、エラー0** | TestResults/phase3-dotnet-unity-compile.log |
| 画像コピーのSHA-256 | 2枚とも元ファイル一致 | docs/ASSET_SOURCES.md |
| 今回のUnity BuildAll／EditMode／PlayMode／Windowsビルド | **未実行**。AGENTS.mdに従いClaudeが実行 | フェーズ3のUnity結果はまだありません |
| 画面・実キー・操作感・物理パッド | **今回未確認** | Claudeの手動検証待ち |
| 過去のフェーズ2 Unity検証 | EditMode94/94、PlayMode2/6。BuildAll・Windows・実キー攻防はClaude報告で成功 | phase2_EditMode.xml・phase2_PlayMode.xml、PHASE3指示書 |

代替143件は既存92＋追加51。3キャラの通常/特殊/超必殺/固有カウンター/復帰、単発命中、蓄積保持、食事の時期/中断/クールダウン/ハート非回復、放物線、麻痺の上限/行動禁止/防御/追加被弾、バフ期限と倍率、弾消去数、構え入力・失敗・通常被弾、ガード時の隙、十字の範囲外、カットイン時計/物理/効果停止・押下破棄・同時開始・最後tick、30/60/144/240FPSでの一致を確認しました。

生成アセットのEditMode2件とPlayMode8件はコンパイルのみ。標準設定のUnity EditModeは計145件になる想定です。**.NET成功をUnity実行成功とは扱っていません。** 過去のUnity XML/ログは上書きせず、今回の結果はphase3-dotnet名で保存しました。パッケージ新規取得・外部送信はありません。

## 変更ファイル概要

- Scripts/Battle：Definitions、状態/状態機械、AttackInstance、接触/ダメージ/防御、Projectile、BattleSimulationを拡張。SkillSystem、PhaseThreeDefaultsを追加。
- Scripts/Game：CharacterData、BattleRunner、InputController、FighterView、ProjectileView、HUDControllerを更新。BattleSetup、UltimateCutInViewを追加。
- Scripts/Editor/ProjectBuilder.cs：3キャラ/起動設定/Portrait/カットインの生成、旧データ保持。
- Tests/EditMode/CharacterTests.csを追加、ProjectAssetTests更新。PlayMode/BattleSceneTests.csとasmdefを更新。ValidationのTestFramework DLL参照追加。
- Art/Characters画像2枚。README、GAME_DESIGN、BALANCE_CONFIG、docs/CHARACTER_REFERENCE・ASSET_SOURCES・ARCHITECTURE・codex_logs/phase3_final、Data/Scenes README、Validation README、本ファイル。

## 未実装・既知の問題

- フェーズ3のUnity実行、PlayMode入力基盤のバッチ合格、実アセット生成/調整保持、画面、Windows再ビルドは未確認。現存Battleシーンは再生成が必要です。
- キャラ選択画面・タイトル/ステージ選択/ポーズ、作品3ステージ・最後10秒の縮小、CPU、音、サポートは後続フェーズ。
- 戦闘Sprite/モーションは自作の仮図形。透真の容姿/本人肖像は確認できず未導入。原作設定を推測で確定していません。
- 飛び道具は放物線にも対応しましたが、地形で消滅しません。麻痺はヒットストップを除く最大45Fで、追加ヒットストップ分の実時間延長はあります。
- 全キャラ数値・ゲージ到達頻度は暫定。CPU試合の実測はフェーズ5。ゲームパッド再接続/割り当てUIは未実装、物理パッド未確認。
- 日本語/ハート、状態文言とHUD/ポップアップ/カットインの重なり、比率・カメラはUnity未確認。
- floatの異なるCPU/プラットフォーム間でのビット単位決定性は保証せず、オンライン未実装。

## ClaudeにUnityで確認してほしい点

1. READMEのBuildAll→EditMode→PlayModeを、エディタで同プロジェクトを開いていない状態で-nographicsなし・Focus無しバッチで実行。標準想定 **EditMode145、PlayMode8**。phase3の新しいXML/ログに保存。
2. 標準59アセット、3キャラ独立攻撃、BattleSetup、Portrait Sprite、新規.metaを確認。旧22を保持し、Inspector調整後のBuildAll再実行でも値が維持されること。
3. 旧失敗4件、currentキーボード、独立移動/R、全3キャラ×両プレイヤー枠、Fキー選択→R→耳/帽子/HUDが通ること。失敗時はFixture.Setupの実行とキー状態を含むXML/ログを返してください。
4. 実キー/物理パッドで3キャラ全技、カウンター、上技復帰。蓮の構え追加入力/ガード42F、透真の3秒強化/敵弾消去、千鶴の30F麻痺/食事24F後・中断・4秒制限・ハート非回復を確認。
5. 超必殺100消費/2ハート/1回、通常ガード/回避貫通と5Fジャスト、範囲外/空振り硬直、30F暗転中の時計/物理/効果停止、同時発動の2帯・原作肖像・再戦リセットを確認。
6. 日本語・頭上カウンター・HUD/ポップアップ/カットイン・画面比率・F11を確認し、Windowsビルドを作り直して操作感をレビュー。

詳細な引継ぎは `docs/codex_logs/phase3_final.md`。Unity結果に問題がなければ、フェーズ4のステージ/縮小/メニュー/選択/進行へ進みます。

