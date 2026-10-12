# フェーズ5 アーキテクチャ

## 分離

`AniBato.Battle` はUnityEngineを参照しない独立asmdefです。自前の `BattleVector` / `BattleRect`、純C#定義、状態、入力、物理、攻撃、ダメージ、終了判定を保持します。Rigidbody2DやUnityの物理・時刻を使いません。`BattleSimulation.Tick(in FrameInput p1, in FrameInput p2)` だけで進行し、EditModeテストから直接駆動します。

`AniBato.Game` はScriptableObjectから純C#定義への変換、Input System入力、描画、カメラ、HUD、Unity時間からのtick駆動を担当します。描画はシミュレーション状態を読むだけです。`AniBato.EditorTools` は初期アセットとシーンをUnityのEditor APIで生成します。手書きのシーン/アセットYAMLは使いません。

## tickの処理順

1. 前tickイベントを消去。終了済みなら以後の状態は更新しない。
2. カットイン残りF>0なら1減らして終了。時計・物理・効果・入力バッファは更新しない。それ以外はtick数+1、残りtick-1。ヒットストップでも試合時計は進む。
3. StageShrinkControllerが時計から共通縮小と警告/消滅イベントを更新し、消失した接地と地上ジャンプ資源を解除する。入力を処理する前の有効な近接／飛び道具の重なりIDを両者に保存。入力を6Fバッファへ保存し、両者の行動受付を確定。防御受付は接触解決に先行する。
4. 超必殺開始を検知したらイベントと共有カットイン残りFを設定。カットインがある場合は、このtickの終了判定を実施してから移動・接触を保留する。時間切れなどで試合終了した場合はカットインも解除する。それ以外は両者の移動・重力・自前AABB対ステージを処理。固体足場、小足場の通過・着地・降下、着地時の復帰資源回復を判定。飛び道具の位置/重力/寿命を更新・発射し、持続中の弾消去効果を適用する。
5. 両方向の近接と全飛び道具の接触を収集。全接触の防御結果を先に評価し、途中のダメージで同tickの防御意思を変更しない。
6. 防御またはダメージを適用し、カウンター、ジャスト、ゲージ、通常蓄積、ハート、停止・硬直、フィニッシュを反映。そのtickに設定した追撃無敵は収集済みの残り接触にも適用する。
7. `MatchResolver` が場外、ハート0、時間切れの順に終了判定。

`AttackInstance` は試合内の一意IDと解決済み対象集合を持ちます。命中・防御・回避後に何tick重なっても同対象へ再解決しません。通常列は最大3段で、次段は新ID。十字の2判定も同ID・同対象集合を使います。`Projectile` は発射元の同じインスタンスを保持し、元の攻撃の硬直終了・被弾中断後も独立した寿命・位置・発射時の向きで進みます。ProjectileLaunchVelocityとProjectileGravityで放物線にも対応。後の向き変更を反映せず、発射者のKOで除去します。地形による消滅は未実装です。

## 入力と固定60Hz

`BattleRunner.Update` は最初に `InputController.Capture` を呼びます。Updateで読んだPressedを `FrameInputLatch` にORして、次のtickで1回だけ消費します。複数Updateの間に押して離しても押下を保持します。遅い描画で複数tick進む場合、Held/方向は保持し、Pressedは最初のtickのみです。

`FixedTickClock` はdoubleのアキュムレータで1/60秒ごとにtickを呼びます。経過時間を捨てず、描画の30/60/144/240FPSの分割に依存しません。入力が実際に観測された時刻は描画とデバイス更新に依存するため、自動テストは同じtick入力列を渡してシミュレーション進行の独立性を確認します。オンライン用の入力時刻補正やネットコードは未実装です。

押下時の方向も行動ボタンごとにラッチし、移動用の最新X/Yとは分けます。描画の次のサンプルで方向を離しても特殊の選択を失いません。1tick内の別々の押下はそれぞれの方向を保持するため、上位行動が使用不可でも次の行動の方向を失いません。バッファにも押下方向を保存して回復終了時に利用します。同時押し優先順位は [GAME_DESIGN.md](../GAME_DESIGN.md) を参照してください。

操作設定は `InputBindings.asset` に集約し、全アクションにPrimary＋Alternateキー配列を用意します。パッド割り当ては `AssignGamepad(player, device)` で明示でき、起動時は先頭2台を個別割り当て。切断・フォーカス喪失時はラッチをクリアします。キーボードは通常`Keyboard.current`、テスト等では`AssignKeyboard`で対象を固定できます。再接続用の選択UIは未実装です。

Claudeのフェーズ2実機検証でゲーム入力は正常、PlayModeはキーステート未配送4件が失敗しました。今回は`Unity.InputSystem.TestFramework`の`InputTestFixture`を継承し、ネイティブデバイス・イベントを隔離します。Fixtureはテスト用ランタイムをplayer loopへ接続し、UnityTest内のPress/Releaseを次の入力更新へ配送します。ローカルPackageCacheのFixture実装を確認しています。`IgnoreFocus`、DynamicUpdate、`runInBackground`をテスト中だけ適用し、元の設定・エディタ配送・デバイス・ネイティブランタイムをTearDownで復元。キー状態・実移動・R再戦の旧アサーションとcurrent経路を維持します。修正後のバッチモード合格はClaudeの再実行待ちです。

## データと状態

- `BattleRules` → `BattleRuleDefinition`：試合、コンボ、フィニッシュ、場外、`DefenseDefinition`、`GaugeDefinition`、`StageShrinkDefinition`。
- `CharacterData` → `CharacterDefinition`：移動・当たり寸法、地上/空中通常、`SpecialSlot`順の特殊4枠、超必殺、CounterEffectDefinition、UltimateCutInFrames。全攻撃配列をコピー。色・仮シルエット・PortraitはGame側だけ。
- `AttackData` → `AttackDefinition`：分類、フレーム、AABBと任意の第2AABB、打撃開始遅延、移動、飛び道具、麻痺/マーク/回復/バフ、吹っ飛び倍率、クールダウン、弾消去回数、ガード時硬直、追加入力を待つ構え。
- `BattleSetup`：CharacterData/StageData一覧・P1/P2初期インデックス。選択はGameFlowControllerで保持し、アセットへ書き戻さない。
- `StageData` → 共通`StageGeometryData` → `StageGeometry`：足場配列、出現位置、場外ライン。配列は構築時/試合開始時にコピー。作品テーマはGame側で使用する。
- `PresentationSettings`：表示専用のカメラ・色・反応時間。戦闘状態に影響しない。
- `FighterState` / `MatchState`：外部からのsetterは公開せず、Game側は読み取り専用で使用。

`CharacterDefinition` の元アセット配列は試合開始時にコピーし、試合中のアセット調整で戦闘が変わらないようにします。演算はfloatで、同じランタイム・入力列で再現できます。異なるCPU/プラットフォーム間のビット単位の決定性は保証していません。将来のオンライン時には固定小数点などを再検討します。

## イベントと表示

`BattleSimulation.Events` はそのtickの読み取り用リストです。次tickで消去されるため、`BattleRunner.EventProduced` は各tick直後に全イベントを配信します。複数tickが1描画に入っても演出イベントを落としません。外部購読者が後で保存する場合はイベント値をコピーしてください。

従来のイベントに`Guarded`、`Dodged`、`CounterSuccess`、`JustGuard`、`JustDodge`、`ComboFinish`、`GaugeChanged`、`UltimateStarted`を追加。防御成功イベントはSource＝防御者、Target＝攻撃者、Hit/Break/FinishはSource＝攻撃者、Target＝被弾者です。HUDはカウンター／ジャスト／フィニッシュ／BREAKを頭上にポップアップし、LINKと使用可否を表示します。FighterViewはガード／回避／受付中を色・透明度で表示。ProjectileViewは状態を読むだけのSpriteプール。再戦時にゲージ・飛び道具・無敵を新規状態へ戻し、一時演出も消去します。

## ジャスト・カウンター・コンボ

`DefenseSystem.CaptureOverlaps`は防御開始前の有効な重なりを記録し、開始時に`JustExcludedAttacks`へコピーします。`DefenseAge < JustWindowFrames`かつ除外IDでなければジャスト候補。Guardは特殊／超必殺に限り、Dodgeは全攻撃に適用可能。既に重なっていた超必殺には通常防御しか成立せず、貫通されます。予測物理や将来の相手入力は使いません。

近距離通常かつ近接接触にのみカウンターを認め、飛び道具は近距離フラグがtrueでも除外します。成功時に通常蓄積の共通処理を使って攻撃者へ+1を反映し、硬直・追撃無敵・ゲージを設定。各`FighterState.CounterEffect`の`ICounterEffect.OnSuccess`が挿入点で、初期値は各CharacterDefinition.Counterを読む`DataCounterEffect`です。P1/P2へ独立した任意フックを差し込めます。旧`BattleSimulation.CounterEffect`setterは互換用に両者へ同じフックを設定します。

`ComboTracker`は命中数・連続通常数・ハート減少予算を別管理します。特殊は連続通常数を切り、ハート予算はウィンドウ中維持。3連通常のみフィニッシュ。上限で通常BREAKが成立しなければ`NormalHitCount=NormalHitsPerHeart-1`を保持します。超必殺だけ予算を超えて2ハートを失わせます。`FollowupImmunity[attacker]`は攻撃者別で、別の攻撃者からの接触には流用しません。

上特殊は使用開始で`UpSpecialUsed`を設定します。地上からの発生待ちを着地と誤認して消費回数を戻さないよう、上特殊中は回復せず、本当の着地後に解除します。ヒットストップ中は攻撃・防御年齢・被弾／回復硬直を止め、クールダウン／追撃無敵と試合時計はtickで進みます。

`ProjectBuilder`は戦闘ルールSchemaVersion=4へ移行します。version2の防御、version3の縮小、version4のゲージ12/8→14/10を初回だけ補完します。ゲージは命中/被弾の両方が旧標準値の場合だけ更新し、独自調整は保持。既存の移動・通常・試合設定を維持し、InputBindingsのversion2移行ではP2の旧標準テンキーPrimaryのみを新Primaryへ置き換え、テンキーをAlternateとして補います。特殊4枠・超必殺はキャラごとに独立アセット。再実行は調整値を維持します。

フェーズ3では旧22アセットを保持し、3キャラ×12アセット（地上3/空中3/特殊4/超1/CharacterData）＋BattleSetupの37個を追加して標準計59個とし、当時のSchemaVersionは2でした。3キャラの各アセットは未初期化/未参照の時だけ初期値を補い、既存調整を保持します。画像のSpriteインポートもEditor APIで行います。

## 汎用効果とカットイン

`SkillSystem`は最初の持続Fに自己効果を1回だけ適用。被弾で攻撃が中断されれば未発動の回復は成立しません。命中時に麻痺とマークを付与し、防御された接触には付与しません。蓄積回復は0でクランプ、ハートは変えません。麻痺最大45F、アーマーなし。麻痺は被弾硬直と並行、ヒットストップでは停止。最後の麻痺Fでも移動制御を禁止します。バフ/マーク/特殊クールダウンはヒットストップ中も減り、カットインでは全て停止します。

`AttackInstance`は構え残りF・自己効果適用済み・弾消去残数も所有。構えは発生直前に留まり、通常/特殊の新規押下で持続へ進み、期限切れは回復硬直。モードはSpecialで防御/カウンターとは別です。弾消去は敵弾だけを判定し、接触収集前に除去。攻撃者も普通に被弾します。強化は移動/ダッシュ倍率、開始時の攻撃発生倍率、吹っ飛び倍率として使用します。

`UltimateStarted`で`UltimateCutInView`が名前・技名・色帯・任意Portraitを表示。描画の時刻で戦闘停止を制御せず、Simulationの残りFを読むだけです。停止中の入力はRunnerが消費しSimulationが破棄します。同tick両者発動は長い方のFを1回共有。開始tickの時計を消費後、指定Fだけ戦闘時計を停止。最後のtickの超必殺でも時間切れはそのtickに確定します。

## 検証

EditMode：仕様番号5〜15の攻防、ジャスト受付境界、カウンター受付境界、既存重なり除外、通常／飛び道具の相性、上限時の2/3保持、3ヒット後の同tick追撃禁止、ゲージ・1試合1回、復帰・再使用制限。既存の累積、勝敗、相打ち、入力、足場も維持。30/60/144/240FPSで攻防状態・イベント列も比較します。

PlayMode：シーン起動、実キーステートの配送とP1/P2独立移動、R再戦、頭上追従、P2のテンキー不要／代替ジャンプ、ガードとLINK／ポップアップ生成、通常のcurrentキーボード経路。Unity実行と.NET代替実行は別結果として記録します。

フェーズ3追加：3キャラの地上/空中通常・3連コンボ・4枠発動（食事は補助技）、固有カウンターと独立フック、復帰、単発ヒット、放物線、麻痺上限/防御/追加被弾、回復中断/クールダウン、バフ期限/発生/吹っ飛び倍率、弾消去、構え入力/失敗、ガード時の隙、十字の範囲外、超必殺の時間停止/同時発動/最後tick/1回制限、30/60/144/240FPSの状態・イベント一致。PlayModeに両プレイヤー枠で全3キャラの移動・通常・特殊とFキー選択→R→表示更新を追加。

## ART1：純C#表示選択とスプライト加工

`AniBato.Presentation`はBattleを参照するUnity非依存asmdefです。FighterAnimationPlayerが戦闘状態を読み、同じ60Hz tickでAnimationSampleを確定します。Battleには読み取り専用のIsFrozenThisTick公開のみを追加しました。カットイン前後の停止と、ヒットストップ残り1のtickでもSampleを止めます。戦闘終了後はBattleSimulationを更新せず表示選択だけ固定tickで更新します。

FighterViewはLateUpdateでSampleを読むだけです。CharacterAnimationSetのBindingsで状態からクリップと持続時の絵を取得し、攻撃FrameをStartup/Active/Recovery区間へ対応させます。Spriteがなければ従来の図形です。HUDはFighterViewの後に更新し、加工メタデータの不透明上端と余白で数値位置を求めます。飛び道具の回転は残り寿命から、麻痺アイコンは状態から、波は超必殺の持続経過から再生します。

SpriteForgeの原画・マニフェストはUnity外です。PNG/JSONをEditorのAnimationSetBuilderが読み、Sprite Editor Data Provider APIでスライスとSprite ID/name-file ID対応を保持します。ProjectBuilder.BuildAllがChizuruのアニメセットを生成してCharacterDataに接続します。手書きYAMLやAnimatorは使用しません。制作手順はtools/SpriteForge/README.md、Unity確認手順はdocs/codex_logs/art1_final.md。

PlayModeのInputTestFixture.runtime/focusStateはパッケージ内部APIです。InputFixtureFocusにreflectionを限定し、対象プロパティと値をassertして設定します。元Input Systemの設定・ランタイムはFixture.TearDownへ復元します。生産コードにreflectionは追加していません。

## ART2：3キャラ接続・入力と描画の検証

BuildAllがtoma/ren/chizuruの全セットを生成し、既存Touma/Ren/Chizuru.assetへ接続します。元PNGサイズはGetSourceTextureWidthAndHeightで検査し、初回NPOTインポートの縮小済み寸法を使いません。AnimationSetBuilderはClaude修正版のままです。CharacterSpriteEffectsは表示専用のクリップ名・倍率・位置・色・フィールド透明度をCharacterDataに持ち、ProjectileViewとFighterViewが参照します。エフェクト時刻は固定tickサンプル/弾寿命から取得し、戦闘数値や状態を変更しません。FighterView.RenderedSampleはLateUpdateが実際に描いたSampleの読み取り専用コピーです。

BattleSceneTestsの空Setup/TearDown overrideと、UnitySetUp冒頭IsolateInput/UnityTearDown末尾RestoreInputを保持。Press/Releaseをキーボードへ配送してから次のキーを作り、ビットキーの未処理deltaイベント上書きを防ぎます。自動PlayerLoopを使い、各キー/移動/tick/描画/HUDの状態へ2秒以内に到達したことをassertします。currentテストは別の割当済みKeyboardを解除してKeyboard.currentを操作。新tickと前LateUpdateの混同を防ぐため、ジャブの有効持続はRenderedSampleを見てattack1_01を検査します。3キャラ両枠の実Sprite/反転も検査し、シミュレーションへ入力や状態を直接代入してキーのテストを通していません。

SpriteForgeのValidateArt2.ps1は3キャラ245Fの原画分割・出力・24状態・持続姿勢・headHeightsを検査し、透真/蓮の102出力ファイルを再加工してSHA-256一致を確認します。ART2はClaudeがBuildAll・EditMode194/194・PlayMode10/10・Windowsビルドを検証済み。フェーズ4のUnity実行は未検証です。素材工程はdocs/codex_logs/art2_final.mdです。
SpriteEffects.SchemaVersion=1が初期化済みの印です。nullまたはversion0は未初期化と判断します（空のクラスがデシリアライズされても対応）。透真/蓮は初回BuildAllでversion1へ設定、千鶴はversion0のまま従来のPresentationSettingsへフォールバック。version1の表示調整値は再BuildAllで保持します。
## フェーズ4

StageGeometryDataは作品別StageDataから共通参照。BattleSimulationは地形をSnapshotし、StageShrinkControllerが60Hzで段階縮小とイベントを管理する。MovementSystemは削除/縮小後の支持を行動処理前に確認する。StageViewはSimulation.StageとShrinkの状態を読むだけで描画する。StageBackgroundViewは自作遠景/中景の視差、文化祭の減光のみ担当する。

GameFlowController（Game asmdef）がTitle内の画面状態・選択・カウントダウン・ポーズを管理する。MenuScreenViewは表示、GameFlowBuilderはEditor生成。BattleRunnerのManagedByFlow/Pausedで正式画面を接続し、CPU待機枠ではP2入力を破棄する。終了後は表示アニメのみ進行。再開時は同フレーム入力を抑制してメニュー決定の攻撃混入を防ぐ。直接起動Battleは既存回帰テスト用に残す。

新しいasmdefは作成していない。PlayModeTestsのAniBato.Presentation参照を維持。tools/Validation/ValidateAssemblyReferences.ps1で6アセンブリのプロジェクト内参照を静的確認する（Unityのコンパイラ実行の代替ではない）。GamePlayerCompileValidation.csprojはUNITY_EDITORを外して実行用ソースを追加コンパイルし、終了処理などのプレイヤー側分岐も確認する。

## フェーズ5：CPU・計測・イベント消費

Battle/CpuのICpuBrain.NextInput(simulation, player)がFrameInputを生成。CpuBrainは相手の位置・状態・攻撃の履歴をReactionFrames遅らせて判断する。自分の状態と床は安全判断に使用する。xorshiftの状態、判断間隔、ガード保持、通常の段予約はCPUが所有。シミュレーションや相手の入力を変更しない。BattleRunnerはP2人間入力を破棄してCPU入力を渡す。CpuDifficultyDataから純C# CpuDefinitionへ変換し、再戦で脳を再生成する。

CpuMatchMetricsは人間と同じTickを直接駆動し、AttackStarted／HeartLost／UltimateStarted／Hit／終了状態を数える。攻撃開始は行動受付直後、接触前に記録するので、同tickで被弾中断した攻撃も含められる。HitのAttackKindとMatchEndedのEndReasonは表示／集計情報で、ダメージ規則を変えない。CpuBalanceStudyは両席・同キャラを含む全組合せを複数seedで実行。.NET exporterは既定値、EditorのCpuBalanceReportBuilderは生成済み実アセットを使い、結果を別ファイルに保存する。

PHASE5BのInputControllerはButtonControlをキーに押下と解放待ちを管理する。物理的な立ち上がりを1描画フレーム1回にキャッシュし、短い押下・解放はInputState.updateCountで重複を避けて拾う。READY／画面遷移を起こした操作だけを解放まで遮断し、Esc・Startや別デバイスを止めない。自動画面遷移にガードを追加しない。戦闘Captureも遮断中のボタンだけを除外し、ラッチは遷移で消去。assignedPads／reservedPadsはRemoved通知でも機器予約を保持して再接続に備える。

ProceduralAudioとBattleFeedbackは既存AniBato.Presentation asmdef内の純C#。AudioManagerはBattleEvent購読とメニュー通知を受けAudioSourceへ渡す。生成AudioClipはランタイム所有で破棄時に解放。BattleEffectsViewは表示用時間だけを進め、Flash／終了バナーを更新。CameraControllerは前の揺れを取り除いてから追従し、HUDより前に目標を更新。5BでFighterViewのSprite間引きを撤去し、Sampleは現在tick基準に統一した。火花だけの表示スローを維持し、Time.timeScaleや戦闘状態を演出で変更しない。

## フェーズ5B：描画と試合テンポ

Presentation.RenderSnapshotsはUnity非依存の値スナップショット。BattleRunnerがtick直前／直後に捕捉し、Fractionでキャラ・弾・頭上UI・エフェクト・カメラ目標の位置を補間する。弾は攻撃IDで対応。アニメ・向き・状態は補間しない。停止／カットイン／KO／反転／大きな位置差はスナップ、ポーズ・再戦で履歴をResetする。表示状態はSimulationへ書き戻さない。

FixedTickClockの上限はRunnerから指定し、未処理秒を保持する。既存直接駆動の既定上限なしは回帰用に残す。FramePacingはVSync／描画FPSとPlayerPrefsだけを設定する。入力ラッチと60Hz戦闘は同じ境界。

PhaseFiveBDefaultsは現行生成既定値、BalanceMigrationは純C#の冪等な移行。旧既定値に一致する項目だけSchema5へ更新する。新しいルールのHeartRecoveryRemainingは状態機械で減少し、HitResolverと既に収集した接触の解決側で無敵を確認する。ハート減少からの無敵・吹っ飛びはDamageSystemとカウンター経路で共通化。ProjectileSystemはデータに従って1人1発を制限する。CPUの立て直し／無敵観測／防御と失敗連発後の接近はFrameInputだけを返す。

SpriteForgeは原画と共通パレットを維持して局所不透明ランの2キー中割りと微小整合、単一基準idleの呼吸、GIF出力を行う。Metadataは出力枚数を持ち、Binding.ActiveFrameを拡張列へ再対応させる。元原画のframesとinbetweenFramesを区別し、UnityはPNG寸法から検証・Editor APIで最大8Fをスライスする。新規asmdefはなく両TestsのPresentation参照を維持。

新規asmdefはなし。Game→Battle/Presentation、Editor→Game/Battle、両Tests→Game/Battle/Presentationなど既存の参照を維持。ProjectBuilderはNewScene前のisTitle確定を保ち、NewScene後にsetup／rules／stage／bindings／presentation／audio／flowを再読み込み。保存後はApplication.dataPathから両シーンの実在を確認する。
