# 実装状況：フェーズ6B完了・担当交代（2026-10-12）

[PHASE6B](docs/codex_tasks/PHASE6B.md)は完了済み。**BuildAll成功・EditMode320/320・PlayMode29/29・Windowsビルド成功**を保存済み証跡で確認。実装コミット **9560ed4**（6A・6B）はGitHub origin mainに反映済み。ユーザー指定によりCodexが実装・レビュー・Unity検証・取りまとめを担当し、Claudeへも随時交代できる。

今回の作業は[PHASE6B_CLOSEOUT](docs/codex_tasks/PHASE6B_CLOSEOUT.md)に沿った完了記録・担当分担の更新。コード・アセット・テストは変更せず、Unityを再実行していない。6Aの305/305・24/26は過去履歴であり、現在の6B合格結果は上記。

## 6Bの変更

- PlayMode失敗のDualSense入力を、破棄されるDeltaStateEventから**実際のUSB HID全状態レポート**へ変更。×・□・OPTIONS／A・X／Spaceの旧アサーションを維持。USBの全8行動入力と中立スティックも追加検査する。
- デバイスは個別try/finallyとUnityTearDownで削除。旧Flow／Runner／Inputを止めてからfixtureを復元し、次のSetUpでも旧シーンを先に停止。スティック・十字キーは入力更新番号ごとに一度読み、メニューと戦闘で共有。DualSense表記と後続CPU選択の旧失敗2件を含め、6BのPlayMode全29件が合格した。
- 内蔵image_genで背景3枚と中割り原画3アトラス（6姿勢/人）を生成。背景・サムネ6PNGを接続し、遠景・中景・床を分離。推し駒の6×6セルと外側消失、AI警告、文化祭の消灯・閉場表示を戦闘状態から描画する。原画と最終プロンプトは[phase6b](art_source/phase6b/generation-log.md)。すべて本作独自表現。
- walkに2、dashに4の**描き下ろし姿勢**を各人追加。旧原画は保持し、専用のraw素材とmanifestへ接続。前後コマの混合なし。各8F・15fps、全106クリップ584Fの構成を維持。低優先度のidleの目元・模様の描き直しは行っていない。
- カメラは表示専用CameraFraming。開始は場外ライン2u内側まで、対戦中は上足場・両者、上足場消失後は残存床と両者へ寄る。比率変更時にも再計算し、追従中の縮尺拡大は必要範囲を即確保。地形・Hurtbox・60Hz処理を変更しない。
- 左右2回ダッシュON/OFF（既定ON、両者共通）をメインメニュー・ポーズ・キーコンフィグへ追加。PlayerPrefs保存、説明・技表・チュートリアルへ反映。OFFでも専用ボタン有効。設定はFrameInputへ含め、ラッチ・追いつきtick・リプレイでも保持。OFF時に古い自動ダッシュ予約と1回目の入力履歴を破棄する。
- ステージ選択は左右の割り当てを表示し、上下でも選択可と明記。既存画面の操作表記・停止・設定行の対応を確認。README、ルール、調整値、構造・拡張資料を更新。

## 6Bの検証

| 検証 | 結果 / 証跡 |
|---|---|
| 純C# NUnit | **307成功・0失敗・0スキップ**。既存292＋新規15ケース。TestResults/phase6b-dotnet-battle.xml |
| Editor／全テストのUnity API参照コンパイル | **警告0・エラー0**。phase6b-dotnet-unity-compile.log |
| Player参照コンパイル | **警告0・エラー0**。phase6b-dotnet-player-compile.log |
| asmdef | **既存6件の参照検査合格、新規なし**。両テストのPresentation参照を維持 |
| CPU | **810試合、両者攻撃・ハート減少、自滅近似0**。ふつう48.09秒・縮小23.7%・超必殺65.6%。phase6b-cpu-final.*。純C#既定値であり実SO集計ではない |
| 描画比率 | 1920×1080・1920×1200・1440×1080、開始/対戦/縮小/旧カメラ計12ケース。phase6b-camera-frames.json。包含テスト合格、実GPU未確認 |
| 素材 | 全106GIF・584F、二値アルファ・32色・足元・控えめな結果姿勢・再生成一致を検査。phase6b-animation-verification.* |
| Unity BuildAll | **成功（exit 0）**。phase6b_runner.txt・phase6b_buildall.log。Title.unity/Battle.unityの実在も確認 |
| Unity EditMode | **320成功・0失敗・0スキップ**。TestResults/phase6b_EditMode.xml（2026-10-12 21:14 JST終了） |
| Unity PlayMode | **29成功・0失敗・0スキップ**。TestResults/phase6b_PlayMode.xml（同21:15 JST終了）。旧失敗2件とUSB/設定/カメラ回帰を含む |
| Windowsビルド | **成功（exit 0・Success）**。phase6b_build.log・Build/phase6b/AniBato.exe。phase6a/phase6bの2つを保持 |

## 原本12章：必須テスト1〜24の対応表

下表の自動テストは6B Unity EditMode320/320・PlayMode29/29で合格。物理DualSense・実GPUの手動所見や6B実画面撮影の記録は今回確認できていないため、自動テストの合格と分けて記載する。テストの削除・アサーション緩和は行っていない。

| # | 原本項目 / 現行検査 | 自動テスト名 | Unity・実機確認 | ユーザー決定による旧→新 |
|---:|---|---|---|---|
| 1 | 通常1回＝1/3 | DamageTests.ThreeSeparateNormalsAccumulateAndBreakOneHeart（1回目） | 6B EditMode合格 / 手動所見は記録未確認 | 変更なし |
| 2 | 通常2回＝2/3 | 同テスト（2回目） | 同上 | 変更なし |
| 3 | 通常3回＝ハート−1・0/3 | 同テスト（3回目）、QueuedThreeAttackChainUsesUniqueIdsAndEnds | 同上 | 変更なし |
| 4 | コンボ切れ後も蓄積保持 | DamageTests.AccumulationPersistsThroughTimeMoveJumpAndGuardInput | 同上 | 変更なし |
| 5 | 特殊後も通常蓄積保持 | PhaseTwoAttackTests.Spec05_SpecialLosesOneHeartAndKeepsNormalAccumulation | 同上 | 変更なし |
| 6 | 3コンボ吹っ飛び・追撃禁止 | PhaseTwoAttackTests.Spec06_ThreeHitComboFinishesAndBlocksSameAttackerThenExpires、TempoTests.ThreeNormalsBreakOneHeartAndComboLimitStillHoldsAfterRecovery | 同上 | 変更なし |
| 7 | 通常ガード・しゃがみ崩し・不足時ブレイク | DefenseTests.UserDecision_NormalIsBlockedByBothFreshAndOldGuard、PhaseSixBattleTests.OrdinaryGuardBlocksEveryKindAndSpendsExactCost／CrouchBreaksEvenFreshGuardAndAddsExactlyOne | 同上、実機でスタミナと蓄積を確認 | **2026-10-12：通常貫通→全攻撃ガード＋消費。しゃがみはジャストも崩し蓄積1** |
| 8 | 特殊ガード | DefenseTests.Spec08_SpecialIsGuardedWithDataDrivenStunAndPush、PhaseSixBattleTests.OrdinaryGuardBlocksEveryKindAndSpendsExactCost | 同上 | 防御可を保持、スタミナ消費を追加 |
| 9 | 近距離通常へカウンター同tick優先 | DefenseTests.Spec09_CounterWinsSameTickInEitherPlayerOrder | 同上 | 変更なし |
| 10 | カウンター失敗の隙 | DefenseTests.Spec10_CounterMissHasFullRecoveryAndCooldown | 同上 | 変更なし |
| 11 | カウンターで蓄積1 | DefenseTests.Spec11_CounterBreaksAtThirdAccumulatedHitAndInvokesHookOnce | 同上 | 変更なし |
| 12 | 超必殺ハート2 | PhaseTwoAttackTests.Spec12_UltimateCosts100LosesTwoHeartsAndPreservesAccumulation、CharacterTests.UltimateCosts100StartsOneCutInAndLosesTwoHeartsOnce | 同上 | 変更なし |
| 13 | 超必殺ガードの十分/不足 | DefenseTests.UserDecision_UltimateIsGuardedButPiercesNormalDodge、PhaseSixBattleTests.EmptyStaminaBreaksNonUltimateWithoutDamageAndUltimateHitsTwo | 同上、実機で65以上/未満を確認 | **2026-10-12：通常ガード不可→必要スタミナがあれば防御、不足時のみハート2貫通** |
| 14 | 超必殺ジャストガード/回避 | DefenseTests.Spec14_JustDefenseAvoidsUltimateAndRewardsGauge、PhaseSixBattleTests.JustGuardWithEmptyStaminaStillBlocksUltimateWithoutCost | 同上 | 防御可能を保持。ジャストガード成立消費0 |
| 15 | 場外即敗北 | PhaseTwoAttackTests.Spec15_KnockbackCrossingBlastLineLosesImmediately、MatchTests.RingOutHasPriorityOverHeartKOAndTimeUp | 6B EditMode／RingOutResult PlayMode合格 | 変更なし |
| 16 | 70秒＝4200tick | MatchTests.Exactly4200TicksIs70SecondsAndFinishedMatchDoesNotAdvance | 6B EditMode合格 / 手動所見は記録未確認 | カットイン/ポーズを停止、ヒットストップは時計進行を保持 |
| 17 | 残り10秒縮小 | StageShrinkTests.WarningAt15SecondsAndStartAtExactly10Seconds／EveryRemovalHasFullWarningAndFinalShapeIsOnlyCentral30Percent | 6B EditMode合格 / 背景演出の手動所見は記録未確認 | 変更なし |
| 18 | 0秒の正しい勝敗 | MatchTests.TimeUpUsesHeartsThenFewerAccumulatedHitsThenDraw、StageShrinkTests.TimeUpComparesHeartsThenAccumulationWithShrinkEnabled | 同上 | ハート→蓄積→引分の暫定規則を保持 |
| 19 | 3キャラ操作可能 | CharacterTests.EachCharacterHasIndependentValidMovesAndGroundAirNormals、BattleSceneTests.Spec19_AllThreeCharactersMoveAndUseNormalsAndSpecialsOnBothPlayerSlots | 6B EditMode/PlayMode合格 | 方向通常・スタミナを追加。歩行等の旧検査を維持 |
| 20 | CPU対戦成立 | CpuTests.CpuMatchesAttackLoseHeartsAndFinishAcrossRoster、GameFlowSceneTests.TitleCpuBattleAdvancesToResultWithAudioAndEffectsEnabled | 6B EditMode/PlayMode合格。CpuSelectionも合格 | 新ガード/スタミナへCPUを追従（6A） |
| 21 | ローカル2人成立 | BattleSceneTests.TwoKeyboardPlayersMoveIndependentlyAndRRestartsFinishedMatch、GameFlowSceneTests.TitleToLocalBattleUsesSelectedCharactersAndStage／GamepadsNavigateBothCharacterSlotsAndStartPausesBattle | 6B PlayMode合格 / 物理パッド2台の記録未確認 | 変更なし、キーコンフィグとダッシュOFFを追加 |
| 22 | リザルト再戦 | GameFlowSceneTests.ResultRematchResetsEntireMatchAndKeepsSelection | 6B PlayMode合格 | 変更なし |
| 23 | 頭上カウンター追従 | BattleSceneTests.HeadCountersFollowMovingFighters／GeneratedSceneRunsAndCreatesBothFightersAndHeadCounters | 6B PlayMode合格 / 3比率・144Hz手動所見は記録未確認 | 位置補間を5Bから保持 |
| 24 | 多重ヒット防止・高FPS | DamageTests.SameAttackOverlappingManyTicksOnlyHitsOnce、InputAndClockTests.PhaseTwoCombatAndEventsAreIdenticalAcrossRenderPartitions、PhaseSixBattleTests.StaminaAndGuardEventsAreIdenticalAcrossRenderRates、PhaseSixBTests.DashOptionSwitchesRemainDeterministicAcrossRenderRates | 6B EditMode決定性・144Hz描画PlayMode合格 / 実GPU手動所見は記録未確認 | インスタンスID・対象集合とtick契約を保持 |

## 原本15章：成果物12項目

| # | 成果物 | 実装状態 / 残り |
|---:|---|---|
| 1 | Unityプロジェクト | AniBato/、Unity6000.6.0f1。6BのBuildAll・生成シーン検査合格、Title/Battle実在 |
| 2 | 戦闘システム | 純C#60Hz、自前AABB、攻防・スタミナ・勝敗を実装。6B Unity回帰合格 |
| 3 | 初期3キャラ | 透真・蓮・千鶴、方向通常・特殊・超必殺・106クリップ。6B自動検証合格、新中割りの手動所見は記録未確認 |
| 4 | 初期3ステージ | 共通地形、縮小、背景/サムネ。6B自動検証合格、実画面と消失演出の手動所見は記録未確認 |
| 5 | CPU対戦 | 3難易度・seed再現・810試合。.NET既定値確認済み、実SO集計待ち |
| 6 | ローカル2人 | キーボード/パッド個別、再接続予約。6B自動検証合格、物理パッド2台の記録未確認 |
| 7 | UI・対戦演出 | 選択・ポーズ・結果・頭上UI・音・補間。6B自動検証合格、比率/144Hz/音の手動所見は記録未確認 |
| 8 | Windowsビルド | **Build/phase6b/AniBato.exe生成・ビルド成功**。phase6a/phase6bの最新2つを保持 |
| 9 | 操作説明 | 機器表記・再割当・練習・14課題・技表・ダッシュ設定。全14課題の実機通し記録未確認 |
| 10 | 実装済み/未実装一覧 | 本書。サポート・追加6キャラ・時間帯差分・2対2/4人・オンライン等は未実装 |
| 11 | テスト結果 | TestResults/phase6b_EditMode.xml（320/320）・PlayMode.xml（29/29）・各.log。別に.NET証跡も保持 |
| 12 | 拡張設計資料 | docs/ARCHITECTURE.mdのキャラ追加・サポート・時間帯差分・入力/オンライン分離を更新 |

## 改善候補・手動確認の記録・Claudeへの交代

- **カメラ要実機調整**：開始の全体表示と近距離拡大を段階化した。全場外ライン内側を常時表示しつつ全比率で従来よりキャラを大きくする要望は、地形・絵の比率を維持する限り同時には満たせない。現在は上足場の収まりと縮小後の拡大を優先。1080p中央の透真は旧20.7%→対戦17.4%→縮小23.8%、蓮16.7%→14.0%→19.2%。対戦中の全体常時表示・常時拡大は未達として扱う。
- スクショは[phase6b一覧](docs/screenshots/phase6b_previews.md)。6枚のサムネbefore/afterと12枚の**オフライン投影図**で代替し、Unityの実画面とは扱わない。背景中景・HUD・消失・追従の時間挙動は実機で確認する。
- 今後のUnity検証はCodexがサンドボックス外実行を承認申請し、tools/verify/run_unity.ps1 -Tag <作業名>で実行。今回の資料更新では再実行していない。6Bの既存全合格を再検証待ちへ戻さない。
- Claudeへ交代する際はdocs/RESUME.md冒頭から再開。同時編集を避け、git status・指示書・証跡を確認する。手動確認の記録が今回見つからなかった対象は、実DualSense/2台再接続、3比率・144Hzのカメラ/HUD・縮小・新中割り、全14課題、音、実SO810試合集計。確認できた所見を追記する。
- 次はユーザーの実プレイ所見から具体的な修正を選び、指示書を作ってから着手。低優先度idleの目元/柄、サポート・追加6キャラ・時間帯差分・多人数・オンライン・正式音源は改善候補/未実装。

---

## 以下はフェーズ6A提出時の履歴（Unity結果は6B冒頭を参照）

[PHASE6A](docs/codex_tasks/PHASE6A.md)の冒頭「追加のユーザー決定」を反映し、ソース・素材・オフライン検証を完了。**6AのUnity BuildAll／EditMode／PlayMode／Windowsビルド／実機は未実行、Claudeへ引き継ぐ。commit・pushなし。** 5BはClaudeがBuildAll成功・EditMode243/243・PlayMode23/23・Windowsビルド成功を確認済み。

## 実装済み

- 純C#の方向通常27技（3人×地上足払い/背面/空中7方向）。Kind=Normal、近距離カウンター対象、蓄積・3ヒット・コンボ上限・インスタンスIDを共有。空中の前/斜めは移動を伴い、背面の判定/吹っ飛びは実際の打撃方向へ向く。空中の向き固定、データ化した斜め閾値/未登録時フォールバック、下系の着地隙。
- しゃがみ：足位置固定でHurtbox高さ55%、移動不可。上足場から降りるのは下18F長押し。単純な下/しゃがみ攻撃との衝突を回避。
- 整数単位のStaminaSystem。各キャラの最大/回復/遅延/行動消費/ガード費用。拒否時のHUD赤点滅。地上ジャンプと上復帰の救済を実際の着地までテスト。
- ユーザー決定のガード：全種類防御、保持/成立で消費、不足した通常/特殊は無被弾ブレイク、超必殺は貫通。ジャスト成立は消費0。しゃがみはジャストも崩して蓄積1、18F硬直・36Fガード不能。GuardBrokenイベント、HUD・SE。
- 練習（棒立ち/ガード/ジャンプ/ふつうCPU）、ハート/時間の個別固定、場外の練習専用救済、入力/最後の技表示。14段階チュートリアル、2ページ技表、キャラ選択/ポーズから起動。
- DualSense/PlayStation・Xbox・キーボードの表記切替、現在の割り当てに追従。キーコンフィグはP1/P2・機器別、変更/初期化/ローカル保存/重複入替。旧AlternateはPrimary変更時に無効、初期化で復元。対戦ヒントは開始5秒のみでOFF可。
- CPUの通常ガード/しゃがみ崩し/スタミナ・上復帰温存、向き固定に合わせた突進安全判定。旧向きで蓮の構えから場外へ飛ぶケースと復帰資源を使い切るケースを回帰テスト化。
- image_genで方向原画30キー、SpriteForgeで新規10クリップ/人。旧14主要クリップ/人の混合を廃止、綺麗なキー保持で再生。Win/Loseは単一idle原画を基準に胴/足固定・頭1〜2px。計106シート584F、106GIF。
- BuildAllのSchema6移行・方向AttackData27個生成・新アニメ接続。既存数値・独自調整・Timing・NewScene前判定/後の再読込を維持。新asmdefなし。

## 検証結果

| 検証 | 結果 | 証跡 |
|---|---|---|
| 純C# NUnit・全既存回帰＋新規62ケース | **292成功／0失敗／0スキップ** | TestResults/phase6a-dotnet-battle.xml・.log |
| Game/Editor/全テストのUnity API参照コンパイル | 警告0／エラー0 | phase6a-dotnet-unity-compile.log |
| UNITY_EDITORなしのプレイヤー参照コンパイル | 警告0／エラー0 | phase6a-dotnet-player-compile.log |
| asmdef参照 | 既存6件合格、新規なし | phase6a-assembly-references.json |
| CPU全9組×30seed×3難易度 | **810試合、全試合両者攻撃/ハート減少、自滅近似0** | phase6a-cpu-final.csv・docs/CPU_BALANCE_REPORT.md |
| 30/60/144/240Hz決定性 | 状態/スタミナ/イベント一致 | PhaseSixBattleTests・TempoTests |
| 素材・GIF・再現性 | **584F、106GIF全復号、351ファイルSHA-256一致** | phase6a-animation-verification.json・.log |
| 6AのUnity実行・実機 | **未実行** | Claude担当。EditMode305/PlayMode26は想定件数のみ |

旧ルールの検査を新しい具体的な期待値へ置換し、既存テストの削除・アサーション緩和はなし。[置換一覧と必須24項目](docs/PHASE6A_TEST_CHANGES.md)。PlayModeには実DualSense/XInputテストデバイス、設定UI→保存→初期化、練習/技表停止の3件を追加。入力分離/復元の既存構造を維持。

| 難易度 | 平均決着秒 | 縮小 | 超必殺あり | 自滅近似 |
|---|---:|---:|---:|---:|
| やさしい | 23.94 | 0% | 33.0% | 0 |
| ふつう | **48.09** | **23.7%** | **65.6%** | **0** |
| むずかしい | 47.66 | 28.1% | 67.4% | 0 |

純C#既定値での集計。Unity生成済みSOの集計と対人難易度・相性の評価は未実行。

## 変更ファイルと次の作業

Battle：StaminaSystem/DirectionalAttackSelector/PhaseSixADefaults/PracticeSession新規、定義・状態・FSM・移動・防御・接触・CPU。Game：ControlConfiguration/LearningText新規、入力・画面進行・表示・Runner/HUD。Presentation：ControlProfile、方向アニメ・控えめな結果・GuardBroken音。Editor：Schema移行/AttackData生成/スタミナHUD/CPU集計。EditMode/PlayMode、SpriteForge、Validation、資料・原画・PNG/JSON/GIF。

ClaudeはUnityを閉じて `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase6a`、次にCpuBalanceReportBuilder.Writeで実SO810試合を保存する。まずTitle/Battle実在・Schema6・方向27技・全34状態を確認。DualSenseの表記と割り当て、全14課題を順番にプレイ、スタミナのガード貫通/崩し、下長押し、空中斜め/後ろ、Win/Loseを確認。[詳細手順](docs/codex_logs/phase6a_final.md)。

既知の限界：新規攻撃は描き下ろした綺麗な1キーを保持するため、連続した動作の描き足しは今後の改善余地。実機で絵とF9判定・新規原画の顔/衣装・144Hz操作感を確認する。シーン/新SO/.meta/実行ファイルは今回Unityで生成していない。背景/カメラの最終仕上げ、サポート・オンライン・正式音源は残作業。

---

## 以下はフェーズ5B提出時の履歴（その後ClaudeがUnity全合格を確認）

指示書：[PHASE5B](docs/codex_tasks/PHASE5B.md)。**0 → A → Bのソース実装、素材生成、オフライン検証を完了。UnityでのBuildAll・EditMode・PlayMode・Windowsビルド・144Hz実機確認は未実行です。** commit／push／公開なし。

## 今回の変更

- 0：画面入力はボタン・デバイス単位で押下を管理。遷移を起こした操作だけを解放まで遮断し、自動Countdown→Battle／Battle→Result／フォーカス喪失では新しいガードを作らない。キャラのREADY操作も個別に保護。Esc／Start・別キー・別パッドは即有効。再開時はその決定キーだけを戦闘ラッチから除外する。
- Input Systemの押下フラグを画面間／フレーム間で再使用せず、物理的な立ち上がりを1描画フレーム1回にする。押して離す短い入力はInputState.updateCountで重複を避けながら拾う。Removedとして通知されるパッド切断でも予約機器を保持し、逆順の再接続で席を入れ替えない。
- 既存PlayModeの5失敗のアサーションを維持。HeldConfirmテストはP1の実際のREADYキーEnterを保持する準備へ変更した（旧「無関係のEnterも遮断」はPHASE5Bの個別ガード仕様と矛盾）。無関係のEnter／Escと、別パッド決定／Startを即受け付ける新規テストを追加。5件のUnity合格は未確認。
- 同じ入力更新で決定と取消が観測されても、実際の遷移に使った種類だけを遮断する。CPUキャラ選択の確定はP1だけを対象とする。千鶴ジャブの持続姿勢テストは6F化の正確な添字2とSprite名attack1_02へ更新し、厳密な一致検査を維持。
- A：純C#の表示専用RenderSnapshotsをBattleRunnerがtick前後で更新。位置だけをprev/currentとFractionで補間し、FighterView、ProjectileView、頭上数値・ポップアップ、もぐもぐ・波・フィールド・火花、カメラ目標へ接続。弾は攻撃IDで対応し、リスト添字の使い回しで補間しない。
- ヒットストップ／ポーズ／カットイン／KO／再戦・リセット／大きな位置ジャンプ／向き反転は補間を停止。アニメと向きは現在の固定tickのSampleを読む。従来のSprite30Hz間引きは撤去し、ジャスト演出は火花の表示スローを維持。
- 既定VSync ON・targetFrameRate=-1。メインメニュー／ポーズにモニター同期・60・120・144・無制限、端末保存。追いつき上限5tick/フレームと残時間持ち越し。上限と位置ジャンプ閾値はPresentation.assetから設定。
- A素材：各キャラ14アニメ、計42クリップを更新。idle/walk/dashは8F（idle12fps、walk/dash15fps）、jump/fall/attack1〜3/air_attack/特殊4種/guardは6F・15fps。合計245→368F（透真125、蓮118、千鶴125）。
- SpriteForgeに2キーポーズの不透明ランごとの中割り、頭頂・胴・足の微小整合、共通32色への減色、単一基準idleの±1px呼吸、GIF出力を追加。ローカルの手続き生成で、画像生成AI／外部送信は使用していない。手足の透過隙間を伸ばさない。原画は保持し、旧加工結果はart_source/phase5b_keys/へ保存。
- B：現行の作成既定値はPhaseFiveBDefaults。特殊の発生+12F／硬直+18F／再使用120F／ガード時48F以上。上復帰は発生を維持し再使用90F、食事240Fは保持。1人1発の画面内弾制限。
- ハートを失った生存者へ75F無敵＋最低(12,8)の吹っ飛び。攻撃されても無敵は延長せず、無敵中の攻撃入力でも解除・延長しない。カウンターによるハート減少にも適用。原本のハート3、通常3回→1、特殊1、超必殺2、コンボ上限・通常ガード不可は保持。
- CPUは特殊の予備動作への防御を難易度別に増やし、攻撃後は距離を作って立て直す。相手の無敵点滅中は無駄撃ちしない。空振り／防御が続いた場合は間合いを詰める。ゲージ14/10/20/18→12/8/16/14（命中/被弾/カウンター/ジャスト）。
- BuildAllでルール・CPU・キャラをSchemaVersion5へ移行。旧既定値に一致する項目だけ更新し、独自調整・アニメTiming・NewScene前判定／後の再読込を保持。新規asmdefなし。

## 今回の検証

| 検証 | 結果 | 証跡 |
| --- | --- | --- |
| 純C# NUnit・原本1〜18の既存回帰を含む | 230成功／0失敗／0スキップ | TestResults/phase5b-dotnet-battle.xml・.log |
| Game／Editor／両TestsのAPI参照コンパイル | 警告0／エラー0 | phase5b-dotnet-unity-compile.log |
| UNITY_EDITORなしのプレイヤーコンパイル | 警告0／エラー0 | phase5b-dotnet-player-compile.log |
| asmdef参照 | 6件合格、既存Presentation参照を維持 | phase5b-assembly-references.json |
| CPU全9組×30seed×3難易度 | 810試合、全試合両者攻撃・ハート減少、自滅近似0 | phase5b-cpu-final.csv・docs/CPU_BALANCE_REPORT.md |
| 144Hz相当・表示スナップショット計測 | 単調、差分0.06597185〜0.06597233u、補間なしの対照は停止フレームあり | phase5b-render-144hz-dotnet.csv |
| 全素材・GIF復号・idle整合 | 368F、32色以下、二値α、足元、42GIFの全コマ復号、idle胴/足固定・頭頂±1px | phase5b-animation-verification.json・.log |
| SpriteForge再生成の再現性 | PNG/JSON/contact/GIF全217ファイルでSHA-256一致 | phase5b-spriteforge-reproducibility.json・.log |
| PHASE5BのUnity実行／実画面 | **未実行** | Claude担当。Unity想定EditMode243／PlayMode23は合格件数ではない |

純C#追加検査は補間・弾ID・停止／リセット・時間持ち越し、無敵／再使用／1発制限、独自設定の保持と再移行、現行データで30/60/144/240Hzの状態・イベント一致、seed1〜20のふつう180試合の目標範囲を含む。従来Phase1〜3の単独ルールテストは元の短いフレーム定義で保持し、現行ルールのハート数・ダメージ・コンボ上限・無敵をTempoTestsで別途検証する。

## CPU before → after

純C#の作成既定値。beforeは保存済みフェーズ5最終CSV、afterは現行データ。実ScriptableObjectの集計はUnity起動が必要なため未実行。

| 難易度 | 平均秒 | 縮小到達率 | 超必殺発動試合率 | 自滅近似 |
| --- | --- | --- | --- | --- |
| やさしい | 7.23→23.36 | 0→0% | 75.2→45.6% | 0→0 |
| ふつう | **4.83→45.51** | **0→21.9%** | **67.8→56.7%** | 0→0 |
| むずかしい | 4.26→44.28 | 0→17.0% | 55.9→59.3% | 0→0 |

ふつうは指定の平均35〜55秒、縮小20〜40%、超必殺半数前後を満たす。270試合中59縮小、26時間切れ、153超必殺あり（162発動／151命中）。ハート・場外・時間の優先順位と70秒を変更していない。

## 素材・残課題・Claudeへの確認

- GIFはdocs/art_wip/{toma,ren,chizuru}_{idle,walk,dash,jump,fall,attack1,attack2,attack3,air_attack,special_neutral,special_side,special_up,special_down,guard}_preview.gif。[42クリップのリンク一覧](docs/art_wip/phase5b_previews.md)と各キャラcontact.png・分割ページを用意。透真idle/移動、蓮攻撃、千鶴攻撃の一覧を目視し、中割りの手足引き伸ばしを修正した。
- ローカル中割りは描き下ろしの骨格アニメではない。大きく姿勢が変わる箇所は近いキーポーズを使うため、原画間の顔・衣装・ポーズ差は一部残る。idleのボイリングは単一絵からの生成で抑制。動作全体の快適さは144Hz実機待ち。
- 5回帰はソース修正済みだがUnity InputTestFixture配送・DualSense2台・再戦・フォーカスの実確認が必要。キャッシュ／シーン／.asset／.metaとWindows実行ファイルは今回Unityで更新していない。まずBuildAllを実行する。
- CPUの対人難易度・キャラ相性・超必殺命中率（ふつう151/162）は実機調整課題。集計は公平性の保証ではない。サポート／オンライン、背景・カメラの最終調整、PS表記・キーコンフィグはフェーズ6へ残る。
- 次の手順はdocs/RESUME.mdとdocs/codex_logs/phase5b_final.md。Unityを閉じて、外部実行可能なClaudeがtools/verify/run_unity.ps1 -Tag phase5b、実アセットCPU集計、144Hz・DualSense・点滅・硬直・再戦を確認する。HANDOFF §2とAGENTS.mdのIPC制約のためCodexはUnityを実行していない。

---

## 以下はフェーズ5提出時の履歴（最新の判断には上のフェーズ5Bを使用）

Claudeの後続確認はBuildAll成功／EditMode225/225／PlayMode15/20／Windowsビルド成功。以下の「未実行」は当時のCodex提出時点の記録であり、上記Claude結果へ更新されている。

更新日：2026-10-12（JST）。指示書：`docs/codex_tasks/PHASE5.md`。原本：`docs/SPEC_ORIGINAL.md`。

**フェーズ4検証後の修正とフェーズ5のソース実装・.NET代替検証を完了。フェーズ5のUnity BuildAll／EditMode／PlayMode／Windowsビルド／実画面・実音は未実行です。** git commit／push／公開はしていません。

## 前フェーズの確認済み結果と維持したもの

Claudeのフェーズ4結果はBuildAll成功（Title生成確認）、EditMode204/204、PlayMode16/17、Windowsビルド成功（Build/phase4）。今回も保存済みのphase4_PlayMode.xmlで17件中16成功・1失敗を確認しました。これをフェーズ5の合格とは扱いません。

ProjectBuilder.BuildSceneの「NewScene前にisTitleを決定、NewScene後にアセットを読み直す」構造を維持し、CPU／音設定を含む全シーン依存アセットを読み直します。保存後にはApplication.dataPathからTitle/Battle両ファイルの実在も検査。3キャラの画像、SpriteForge、AnimationSetBuilder、技の性能は保持。BattleSceneTests/GameFlowSceneTestsのUnitySetUp先頭の入力分離とUnityTearDown末尾の復元を維持。新しいasmdefはなく、6アセンブリの参照を検査しました。

## フェーズ4検証後の修正

- メニューの決定入力持ち越し：Clear()が消すのは戦闘用FrameInputLatchだけで、Input SystemのwasPressedThisFrameは同じ更新フレーム中に再読されます。次フレームまで残る押下状態も含め、次画面で決定として再利用されていました。
- Show()で全員の決定・キャンセル・Start／Escの解放待ちを開始。保持中や押下が観測されたフレームは解除せず、全キー・パッドが解放された後の新しい押下を受け付けます。戦闘入力のCaptureも解放まで停止し、ポーズ再開ボタンが攻撃へ漏れるのを防止。
- 既存失敗テストGamepadsNavigateBothCharacterSlotsAndStartPausesBattleのStageSelectアサーションは維持。キーボード保持／全員解放／Esc保持／再開J保持のPlayMode回帰テストを追加。
- 縮小ありの時間切れテストを既存9件内で確認。さらにハート差・蓄積差・同数の3ケースを追加して、縮小5段後の時間切れ比較を明示検査。

## フェーズ5の実装済み項目

- 純C# ICpuBrain/CpuBrain/CpuDefinition。相手の状態を反応遅延付きの履歴から読み、距離・攻撃・ガード・カウンター・空中・硬直、自分のゲージ／ハート、床と縮小予告を判断。シード付きxorshiftで再現可能。状態を書き換えずFrameInputだけを返す。
- 通常3段の予約、4方向の特殊（弾・突進・上復帰・弾消去／構え／食事）、歩行・ダッシュ・ジャンプ・2段ジャンプ・復帰用空中ダッシュ、ガード・回避・カウンター・超必殺。飛び道具は飛行時間までガードを保持。硬直末尾の入力バッファで反撃し、カウンターには特殊、ガードには通常を選ぶ機会を設けた。
- 場端は中央へ戻り、落下時は残ったジャンプ・上特殊・空中ダッシュを使う。突進／回避／ダッシュは終了位置を予測して床外への開始を避ける。縮小予告時は次の床幅を使う。
- CpuEasy/Normal/Hard.assetのScriptableObjectをEditorで生成し、BattleSetupから参照。キャラ選択の難易度がP2 CPUへ反映。CPU戦のP2人間入力は消費後に破棄。再戦でCPUの履歴・乱数・予約を初期化する。
- パッドは接続順にP1/P2へ割当、キャラ選択に機器名を表示。切断した機器を予約し、逆順で再接続しても元の側へ戻す。Xbox／PS操作表と接続ルールを画面・READMEに記載。
- AudioManagerがBattleEventを購読。通常／特殊ヒット、ガード、ジャスト2種、カウンター、BREAK、COMBO FINISH、超必殺、KO／場外／時間切れ、縮小警告。カウントダウンとメニュー移動／決定／キャンセルも再生。メニュー＋3ステージに8秒の自作ループ。
- 全音はProceduralAudioのコード合成（16kHz・モノラル）。外部音源・第三者素材・パッケージは追加していない。ポーズでBGM/SE音量と揺れON/OFFを変更し、PlayerPrefsに保存。
- BattleEffectsView/BattleFeedback：ヒット／ガード火花、軽い画面揺れ、ジャストフラッシュ、Sprite更新を短く30Hzへ落とす表示スロー、カウンター／BREAK／COMBO FINISH既存表示、超必殺帯とPortraitの登場、KO／場外／時間切れバナー、既存win/lose＋結果テキスト。
- 演出は描画側だけ。Time.timeScale、戦闘tick、タイマー、位置・当たり判定は変更しない。ジャストのスローも画像と火花だけで、位置は正確に追従する。
- AttackStartedイベントを接触解決前に出し、同tickに開始して被弾中断した攻撃も集計。Hitに攻撃分類、MatchEndedに終了理由を添えてSE／決着表示を安定させた。

## 検証結果

| 検証 | 結果 | 証跡 |
| --- | --- | --- |
| 純C# EditModeソースの.NET代替 | **212成功 / 0失敗 / 0スキップ** | TestResults/phase5-dotnet-battle.xml・.log |
| Game/Editor/EditMode/PlayMode全ソースAPI参照コンパイル | **警告0 / エラー0** | TestResults/phase5-dotnet-unity-compile.log |
| UNITY_EDITORなしのプレイヤー用代替コンパイル | **警告0 / エラー0** | TestResults/phase5-dotnet-player-compile.log |
| プロジェクト内asmdef参照 | **6アセンブリ合格** | TestResults/phase5-assembly-references.json |
| CPU全9組×30seed×3難易度 | **810試合、全試合で両者攻撃／ハート減少／決着** | docs/CPU_BALANCE_REPORT.md、TestResults/phase5-cpu-final.csv |
| 同じ最終CPUでゲージ12/8比較 | **別810試合集計** | TestResults/phase5-cpu-gauge-before.csv・.md |
| フェーズ5 Unity生成／テスト／ビルド／実機 | **未実行** | Claudeへ引き継ぎ |

追加純C#20ケースはCPU12、音／演出5、縮小時間切れ3。CPU回帰は各難易度270試合を含む。同シード入力列と結果、場端・復帰・縮小、生存して全70秒を迎えるケース、超必殺ジャスト、音／演出の有無による決定性、波形の範囲／再現性を検査。

Unity標準想定は**EditMode225件（純C#212＋生成アセット13）／PlayMode20件（既存17＋新規3）**。想定件数であり合格結果ではありません。CPU選択テストは待機相手を前提にしていたため、同シードの独立シミュレーションとの一致でP2人間入力の無視を確認する内容へ更新。新規PlayModeは保持入力、パッド切断／逆順再接続、Title→CPU戦→リザルト。生成済みアセットを用いたふつうCPU270試合のEditModeも追加。

## CPUバランスの要点

同じ最終CPU・全seedを使い、ゲージだけ比較。命中12→14、被弾8→10、カウンター20／ジャスト18／上限100／1試合1回は保持。SchemaVersion4への初回移行は旧命中12・被弾8の組だけ変更し、独自調整値は保持する。

| 難易度 | 超必殺試合率 調整前→後 | 平均決着秒（後） | 発動数／命中数（後） | 自滅近似 |
| --- | --- | ---: | --- | ---: |
| やさしい | 56.3%→75.2% | 7.23 | 318／165 | 0 |
| ふつう | **47.0%→67.8%** | 4.83 | 317／117 | 0 |
| むずかしい | 38.9%→55.9% | 4.26 | 245／75 | 0 |

ふつうで「半数以上の試合で1回以上発動」の目安を満たした。810試合すべてハート決着（同時KO引き分け含む）、場外／時間切れ0。自滅近似は直近120tickに被弾・カウンターがない場外負けで、意図を直接証明しない。勝率・組合せ別の表はCPU_BALANCE_REPORT。Unity生成済みデータの集計は未実行で、CpuBalanceReportBuilder.Writeを用意した。

## 変更ファイル概要

Battle/Cpuの定義・判断・集計4ファイル、BattleEvent/BattleSimulation/MatchResolver/DamageSystem/PhaseOneDefaults/PhaseTwoDefaults。GameのCpuDifficultyData/AudioSettings/AudioManager/BattleEffectsView新規とBattleRunner/InputController/GameFlowController/MenuScreenView/CameraController/FighterView/UltimateCutInView/PresentationSettings/BattleSetup。PresentationのProceduralAudio/BattleFeedback、EditorのProjectBuilder/CpuBalanceReportBuilder。EditMode CpuTests/ProceduralAudioTests/StageShrinkTests/ProjectAssetTests、PlayMode GameFlowSceneTests/InputFixtureFocus。ValidationのProgram/CpuReport/csproj/RunPhase5と各資料。シーン・アセット・.metaの更新はUnity BuildAllで行う。

## 既知の問題・未実装

- フェーズ5の生成済みシーン／新CPU・音アセット／新.metaはまだ存在しない。既存Build/phase4は旧実装のまま。まずClaudeがBuildAllを実行する。
- 実パッド2台、再接続のデバイス同定、InputTestFixtureのネイティブ配送、既存の失敗テストが本当に通るかはUnity未確認。
- CPU対CPUは平均4〜7秒と短く、今回は縮小に届く試合がない。70秒の上限内に超必殺を使える検証であり、人間相手の難易度・試合の長さ・キャラ間の公平性を証明しない。特に千鶴の対むずかしい組合せ差とP1/P2差を実機で判断してほしい。
- 音は短い仮ループ／電子音。重なりの音量、繋ぎ目、スピーカーでの聴感、音設定表示と保存は未確認。
- ジャストの表示スローは世界全体の時間停止ではなくSprite／火花だけ。画面揺れ・フラッシュ・超必殺／決着バナーの強さ、絵と判定の一致、頭上表示の読める状態を実機で確認する。
- 接続順方式で、専用パッド再割当UIは未実装。別機器への交換時は予約枠が残ることがあり、再起動で割当を作り直す。
- サポート・オンライン・正式音源は未実装。透真の目元・蓮の見づらさは以前のユーザー判断どおり後回し。引き分け両者loseも暫定のまま。

## Claudeへの次の作業

Unityを閉じ、サンドボックス外で `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase5`。BuildAll→EditMode225想定→PlayMode20想定→Windowsビルド。次にCpuBalanceReportBuilder.Writeで実アセット810試合をTestResults/phase5-cpu-unity.*へ保存。確認順序・操作・注意は `docs/codex_logs/phase5_final.md`。Unity合格と画面／音の所見を本書・RESUMEへ追記してからフェーズ6へ。

現在はworkspace-write／承認不可でUnityライセンスIPCへ接続できないため、HANDOFFのサンドボックス外Unity実行は行えません。権限変更の確認は求めず、AGENTS.mdに従ってUnity実行をClaudeへ引き継ぎました。
