# Unityが起動できない場合の代替検証

ART3E追加のWalkNormalAnimationTestsはUnityの生成SO/Sprite/TextureとAssetDatabaseに依存するため、既存のアセットテストと同様に純C#代替プロジェクトの対象外。UnityCompileValidationでコンパイルし、実際のUnity EditModeで全6ケースを実行する。純C#317ケースは継続実行し、Unity検査は省略しない。

Unityに同梱済みのNUnitと参照DLLを利用し、追加パッケージや外部通信なしで検証します。**UnityのEditMode/PlayMode実行や画面検証の代わりにはなりません。** .NET 10 SDKと、このプロジェクトの既存Libraryキャッシュが必要です。

リポジトリルートのPowerShellから実行します。

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) 'TestResults/dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
dotnet restore tools/Validation/BattleValidation.csproj --configfile tools/Validation/NuGet.Config
dotnet run --project tools/Validation/BattleValidation.csproj --no-restore -- TestResults/phase3-dotnet-battle.xml
dotnet restore tools/Validation/UnityCompileValidation.csproj --configfile tools/Validation/NuGet.Config
dotnet build tools/Validation/UnityCompileValidation.csproj --no-restore
```

`BattleValidation` はUnity参照を持たない戦闘コードと同じEditModeテストソースをコンパイルして実行します。生成アセットに依存するテストは含みません。`UnityCompileValidation` はGame/Editor/PlayModeテストを含めUnity API参照でコンパイルするだけで、Unityのネイティブ処理やシーンを実行しません。異なるキャッシュID/インストール先を使う場合はcsprojの参照先を調整してください。

フェーズ3は143件成功。UnityCompileValidationはローカルの`Unity.InputSystem.TestFramework.dll`も参照して、InputTestFixtureを使うPlayModeテストをコンパイルします。Unityでの入力配送・Focus・Spriteインポート・シーン生成はこの代替検証では実行しません。ClaudeのUnity検証はEditMode145件、PlayMode8件が標準設定の想定です。

`Directory.Build.props` でビルド出力を `TestResults/dotnet/<プロジェクト名>/bin` と `obj` に集約します。ここにある約63MBのDLL群は、ローカルのUnityインストールと既存Library/PackageCacheからMSBuildが参照・コピーした検証用出力です。.NETランタイムをネットからダウンロードしたフォルダではありません。この作業ではパッケージを新規取得していません。NuGet.Configはパッケージソースをクリアしています。UnityEditor.CoreModule.dllはローカル原本とSHA-256一致を確認済みです。

`TestResults/` 全体を `.gitignore` で除外し、`git check-ignore TestResults/dotnet` と `git ls-files TestResults/dotnet` で除外済み・追跡なしを確認しました。過去の検証出力は削除せず残しています。代替結果とClaudeのUnity XMLは別ファイル名で保管してください。

ART1追加：AnimationSelectionTestsを純C#代替実行に含め、AnimationAssetTestsはUnity専用として除外しています。UnityCompileValidationはSprite Editor Data Provider用に既存Unity.2D.Sprite.Editor.dllを参照します。外部取得は不要です。ART1の代替実行は178件、Unity標準EditModeはアセット4件を含む182件、PlayModeは10件の想定です。結果はart1-*名で保存し、Unity実行結果と区別します。

ART2：純C#代替183件成功、全ソースコンパイル警告0/エラー0。Unity標準想定はEditMode194件（183＋アニメアセット9＋生成アセット2）/PlayMode10件です。AnimationAssetTestsは3キャラの全Sprite、状態対応、停止/反転、headHeights、エフェクト参照、再取り込みID/Timingを検査します。BattleSceneTestsはClaudeのUnitySetUp/UnityTearDownの分離構造を維持し、Press/Release配送と実際のtick/描画/HUDの反映を待ちます。ネイティブPlayMode実行は行っていません。

結果はTestResults/art2-dotnet-battle.xml/.logとart2-dotnet-unity-compile.logです。SpriteForge全245F検査/再現性はtools/SpriteForge/ValidateArt2.ps1、手順はdocs/codex_logs/art2_final.md。Unity用のXML名art2_EditMode.xml/art2_PlayMode.xmlとは区別してください。既存ART1 Unity結果はClaudeの182/182・4/10で、ART2の合格扱いにはしません。
フェーズ4：純C#代替192件成功。Unity標準想定はEditMode204/PlayMode17（未実行）。GameFlowSceneTestsは入力分離をUnitySetUpで明示実行し、正式Titleの遷移・停止・再戦・選択保持・実Sprite参照を検査する。BuildAllをClaudeが実行するまでTitle等の生成物は更新されない。

プレイヤー側の条件コンパイル分岐を確認する追加コマンド（UNITY_EDITORなし、実Windowsビルドの代替ではない）：

```powershell
dotnet restore tools/Validation/GamePlayerCompileValidation.csproj --configfile tools/Validation/NuGet.Config
dotnet build tools/Validation/GamePlayerCompileValidation.csproj --no-restore
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Validation/ValidateAssemblyReferences.ps1
```

asmdef参照スクリプトはプロジェクト内の6アセンブリだけを静的確認する。UnityEngineやパッケージの依存解決・生成アセットのインポートはUnityで確認する。フェーズ4では新規asmdefはない。PlayModeTestsのAniBato.Presentation参照は維持した。


## フェーズ5（2026-10-12）

一括のオフライン再現コマンド：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Validation/RunPhase5.ps1
```

純C#212件成功、全ソース／プレイヤー用API参照コンパイル警告0・エラー0、asmdef6件確認。Unity標準想定225／20件は未実行。前述のフェーズ4検証待ち表記は当時の履歴で、Claudeはフェーズ4を204/204・16/17まで検証済み。

BattleValidationはBattle/Cpu以下もコンパイルし、全9組×30seed×3難易度=810試合の回帰を実行する。生成アセットを使うProjectAssetTestsは.NET実行から除外し、Unity側で実データ270試合も検査する。音合成とBattleFeedbackは純C# Presentationで検証する。

Programの追加オプション：

```powershell
dotnet run --project tools/Validation/BattleValidation.csproj --no-restore -- --cpu-report-before TestResults/phase5-cpu-gauge-before
dotnet run --project tools/Validation/BattleValidation.csproj --no-restore -- --cpu-report TestResults/phase5-cpu-final
dotnet run --project tools/Validation/BattleValidation.csproj --no-restore -- --cpu-trace 0 2 19
```

beforeは最終CPUで旧ゲージ12/8だけを使用。通常は14/10。traceはむずかしい同士の指定キャラ0/1/2・seedを診断する。CSVは試合ごとのtick・勝敗・理由・攻撃開始・ハート減少・超必殺発動／命中・自滅近似を保存。Markdown表は勝率・平均秒・終了理由と超必殺試合率を出す。攻撃開始は接触前イベントなので、同tickで被弾中断しても数える。

一括スクリプトは最終表をdocs/CPU_BALANCE_REPORT.mdへコピーする。Unityの実アセット結果はCpuBalanceReportBuilder.WriteでTestResults/phase5-cpu-unity.*へ別保存する。どちらの結果か明示すること。.NETの結果をUnityでの音・画面・入力配送の合格とは扱わない。

## フェーズ5B（現行）

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Validation/RunPhase5B.ps1
```

純C#230件、Unity API参照の全ソース／プレイヤー分岐コンパイル、asmdef6件、現行純C#既定値のCPU810試合、368F画像／42GIFの検査をまとめて実行する。外部パッケージ取得・Unity起動はしない。結果は`TestResults/phase5b-*`。Unity想定EditMode243／PlayMode23はネイティブ実行の成功数ではない。入力配送・実FighterView計測・生成アセットはClaudeのUnity検証が必要。

CPU afterは`PhaseFiveBDefaults`・現行`CpuDefinition`。beforeは保存済みフェーズ5最終CSVを`phase5b-cpu-before.csv`へ保管した値で、全9組・seed1〜30の一致を報告書生成時に確認する。上の旧`--cpu-report-before`は現在のCPUでゲージだけを変更する診断用であり、フェーズ5B変更前の再現には使わない。現行報告は`phase5b-cpu-final.csv/.md`と`docs/CPU_BALANCE_REPORT.md`、実SOは`CpuBalanceReportBuilder.Write`で`phase5b-cpu-unity.csv/.md`へ別保存する。

`phase5b-render-144hz-dotnet.csv`は純C#描画補間の差分。PlayModeの`ActualFighterViewMovesUniformlyWith144HzFrameTimeSequence`は実FighterView経路で`phase5b-render-144hz-unity.csv`を保存する。どちらも144Hz相当の入力時間列であり、実モニター/GPUのフレーム時間測定と区別する。原本の必須戦闘テストと既存回帰のアサーションは維持した。
