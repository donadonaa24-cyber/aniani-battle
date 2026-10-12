# 実装状況：フェーズ5B（2026-10-12）

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
