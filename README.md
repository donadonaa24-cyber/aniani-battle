# あにあにバトル！！（あにバト！！）

オリジナル2D横スクロール対戦アクション。透真・蓮・千鶴によるCPU対戦・同一PCの2人対戦です。原本は [SPEC_ORIGINAL](docs/SPEC_ORIGINAL.md)、現行は [PHASE5B](docs/codex_tasks/PHASE5B.md)。入力回帰・144Hz描画補間・アニメ・試合テンポを改善し、.NET230件と素材検査を完了。**5BのUnity生成・テスト・Windowsビルド・実機は未確認**です。最新手順は [RESUME](docs/RESUME.md)。以下のフェーズ5検証記述は過去の履歴です。

## 起動

1. Unity Hubから **Unity 6000.6.0f1** で `AniBato/` を開きます。
2. 更新後はメニュー「**あにバト → プロジェクト生成（フェーズ5B）**」を実行して、CPU難易度・音設定・3テーマ・タイトルと対戦シーンを生成します。
3. `Assets/_Project/Scenes/Title.unity` を開き、再生します。Windowsの起動シーンもTitleです。
4. 任意キーでメニューへ。W/Sで「2人対戦」を選びEnter。P1はA/D→J、P2は←/→→Deleteでキャラを決定します。
5. ステージをA/Dまたは←/→で選びEnter。3、2、1、FIGHTの後に70秒・ハート3で対戦します。試合終了後はリザルトの「再戦」を選びます。

メインメニューとポーズの「フレームレート」でモニター同期／60／120／144／無制限を選択できます。既定はVSync同期、設定は端末保存。ハートを失った直後は75Fの立て直し無敵を点滅表示します。確定ダメージは通常3回でハート1、特殊1、超必殺2のままです。

5Bのオフライン再検証は tools/Validation/RunPhase5B.ps1。ふつうCPUの平均4.83→45.51秒、縮小0→21.9%、超必殺67.8→56.7%。42アニメGIFは docs/art_wip/<character>_<clip>_preview.gif、更新素材は計368F。実機確認は [phase5b_final](docs/codex_logs/phase5b_final.md)。

再生成は既存の調整用ScriptableObjectの値を保持します。シーン構成は `ProjectBuilder` と `GameFlowBuilder` に記述してください。Build SettingsはTitleが先頭、Battleが2番目です。Battleは戦闘回帰テスト用の直接起動シーンとして残しています。

初期選択は **P1=透真、P2=蓮**。キャラ選択はK/Endで決定解除、未決定ならメニューへ戻ります。パッドは各自の左スティック/十字キー＋南ボタンで決定、東ボタンで解除/戻る。CPU対戦はメニューの「CPU対戦」から始めます。P1 W/Sで自分・CPU・難易度の行を選び、A/Dで変更、Enterで決定します。やさしい／ふつう／むずかしいを選べ、P2はCPUが操作します。CPU戦中のP2キーボード／パッド入力は戦闘に渡しません。

EscまたはパッドStartでポーズ。再開／操作説明／キャラ選択／タイトルを選べます。ポーズ中と操作説明中は戦闘tickを止め、復帰時に停止分を追い付き処理しません。リザルトは再戦／キャラ選択／タイトル。再戦は同じキャラ・ステージを使い、ハート・ゲージ・被弾・縮小を初期化します。ランダム選択も再戦時は決まった同じステージを使います。

3ステージは星の終わり／推し駒Battle／Battle à la carte。開始55秒で予告、60秒で縮小開始、61.8・63.6・65.4・67.2・69秒で左右の床が消えます。各消滅の1秒前から対象区画が点滅します。上足場は63.6秒で消え、最後は中央幅10.8の足場のみ。床の消失は落下を起こし、場外ラインを超えて敗北します。

## 操作

| 操作 | P1 | P2（テンキー不要） | P2代替 | ゲームパッド |
| --- | --- | --- | --- | --- |
| 左右移動 | A / D | ← / → | — | 左スティック / 十字キー |
| 上 / 下 | W / S | ↑ / ↓ | — | 左スティック / 十字キー |
| ジャンプ・2段ジャンプ | Space | 右Ctrl | テンキー0 | 南ボタン |
| 通常（連打予約で最大3段） | J | Delete | テンキー1 | 西ボタン |
| 特殊（ニュートラル・横・上・下） | 方向＋K | 方向＋End | 方向＋テンキー2 | 方向＋北ボタン |
| ガード（保持） | L | PageDown | テンキー3 | 左トリガー |
| 回避 | 左Shift | 右Shift | テンキー4 | 右トリガー |
| カウンター | I | PageUp | テンキー5 | 右肩ボタン |
| 超必殺（LINK 100、1試合1回） | U | Home | テンキー6 | 左肩ボタン |
| ダッシュ・空中ダッシュ | F / 左右2回タップ | 右Alt / 左右2回タップ | テンキーEnter | 左スティック押し込み / 左右2回タップ |
| 急降下・小足場から降りる | S | ↓ | — | 下入力 |
| ポーズ・再開 | Esc | 共通 | — | Start |
| メニュー決定 | Enter / J / Space | Delete / 右Ctrl | テンキー0/1 | 南ボタン |
| メニュー戻る・決定解除 | Esc / K | End | テンキー2 | 東ボタン |
| 判定枠表示（Hurtbox緑/Hitbox赤/弾黄） | F9 | 共通 | — | キーボードF9 |
| 全画面 / ウィンドウ（プレイヤー） | F11 | 共通 | — | キーボードF11 |

地上ダッシュは進行方向の保持で継続します。空中ダッシュは着地まで1回。上特殊も着地まで1回。P2はテンキーなしで操作でき、ノートPCによってDelete/Home等にはFn併用が必要です。`InputBindings.asset` の各PrimaryとAlternate配列で1行動に複数キーを指定できます。キーボードの同時押し制限は機種に依存します。

通常はガードを貫通、特殊はガード可能。カウンターは近距離通常に有効。超必殺は通常ガード／回避を貫通し、直前5Fのジャストガード／回避で防げます。ガード解除後などには硬直があります。3連通常でCOMBO FINISHと大吹っ飛び、同じ相手からの追撃を短時間禁止します。

ゲームパッドは接続順にP1/P2へ個別割り当てし、キャラ選択画面に機器名を表示します。切断した機器の枠を予約し、同じ機器を再接続すると元の側へ戻します。片方のパッドを両者へ割り当てません。XboxはA=ジャンプ／決定、X=通常、Y=特殊、LT=ガード、RT=回避、RB=カウンター、LB=超必殺、左スティック押込=ダッシュ、Start=ポーズ。PSは×/□/△/L2/R2/R1/L1に対応し、○=メニュー戻るです。操作説明画面にも表示します。画面遷移を起こしたボタン／機器だけを離すまで無効にし、他のボタン・パッド・Esc／Startは直ちに使えます。物理パッドでの確認は別途必要です。旧F1〜F4/Rは直接起動Battleの回帰確認だけで使用でき、Titleから始める正式画面では無効です。

メニューと3ステージに自作の短いBGMループ、ヒット・防御・ジャスト・カウンター・BREAK・COMBO FINISH・超必殺・決着・縮小などに自作SEがあります。Esc/StartのポーズでBGM／SE音量を左右で調整し、画面揺れをON/OFFにできます。設定は端末内に保存します。ジャスト時のスローはエフェクトの表示演出です。Spriteの30Hz間引きは撤去しました。戦闘tick・タイマー・当たり判定は変えません。

## キャラクターの技

| キャラ | 方向なし特殊 | 横特殊 | 上特殊 | 下特殊 | 超必殺 | カウンター成功 |
| --- | --- | --- | --- | --- | --- | --- |
| 透真 | 観測パルス：短い飛び道具・解析マーク | 遮断ステップ：短距離移動の終盤で打撃 | 緊急上昇：小判定の縦復帰 | 遮断フィールド：近くの敵弾を消去 | 境界突破：前方高速突進 | 3秒間、移動1.25倍・ダッシュ1.3倍 |
| 蓮 | 飛車ライン：長い直線判定 | 直線番長：大きな突進、ガードされると48Fの隙 | 縦抜け：真上へ復帰 | 飛車の構え・急襲：構え後に通常/特殊を再度押して急襲 | 十字突破：縦横の十字判定 | 吹っ飛ばし2倍 |
| 千鶴 | 爆弾おにぎり：放物線・30Fもぐもぐ麻痺 | フライパン突撃：敵弾を1個消して突進 | おたま旋風：上昇・1ヒット | ひとくち休憩：24F後に蓄積-1、4秒クールダウン | 満腹カレー！：広い範囲攻撃 | 自分の蓄積-1 |

千鶴の下特殊は補助技で、相手への攻撃・ハート回復はありません。食事中に被弾すると回復前なら中断し、再使用制限は残ります。蓮の構えは最大60F待機し、追加入力がなければ空振り硬直へ移ります。構えにカウンター判定はありません。上特殊は全員着地まで1回。アーマーはありません。

超必殺は名前・技名・キャラ色と3キャラのカットインを表示。30F（0.5秒）の暗転中は試合タイマー・物理・効果時間を停止し、押下入力を破棄します。通常のヒットストップ中はタイマーが進みます。技の全数値・ゲージ到達想定は [BALANCE_CONFIG.md](BALANCE_CONFIG.md)。原作で確認できた事実と本作独自の表現は [CHARACTER_REFERENCE.md](docs/CHARACTER_REFERENCE.md)、素材出典は [ASSET_SOURCES.md](docs/ASSET_SOURCES.md)。

## 開発・検証

Windows 11、Unity 6000.6.0f1、Input System 1.20.0、uGUI 2.6.0、Test Framework 1.8.0、2D Sprite 1.0.0。入力設定はBoth。戦闘はUnityEngineを参照しない純C#の60Hzシミュレーションです。ユーザー作品の画像2枚をコピーし、第三者アセットは追加していません。

リポジトリルートのPowerShellで実行します。Unityエディタで本プロジェクトを開いている間は、バッチモードを併用しないでください。

```powershell
$unityExe = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$projectPath = Join-Path (Get-Location) 'AniBato'
$resultPath = Join-Path (Get-Location) 'TestResults'
New-Item -ItemType Directory -Force -Path $resultPath | Out-Null
& $unityExe -batchmode -quit -projectPath $projectPath -executeMethod AniBato.EditorTools.ProjectBuilder.BuildAll -logFile (Join-Path $resultPath 'phase5b-build.log')
& $unityExe -batchmode -projectPath $projectPath -runTests -testPlatform EditMode -testResults (Join-Path $resultPath 'phase5b-editmode.xml') -logFile (Join-Path $resultPath 'phase5b-editmode.log')
& $unityExe -batchmode -projectPath $projectPath -runTests -testPlatform PlayMode -testResults (Join-Path $resultPath 'phase5b-playmode.xml') -logFile (Join-Path $resultPath 'phase5b-playmode.log')
```

`-runTests` には `-quit` を付けません。**`-nographics` は使用しません**（Personalライセンスのヘッドレス権限不足で失敗するため）。CodexのサンドボックスではUnityライセンスIPCに接続できないため、Unityの生成・テスト・ビルドはClaudeが実行します。代替検証は [tools/Validation/README.md](tools/Validation/README.md) を参照してください。手動の操作感・画面確認と自動テストの結果は区別します。

フェーズ5Bの.NET代替は230件成功、全C#とプレイヤー側の代替コンパイルは警告・エラー0。asmdef6件の参照確認済み。Unity標準想定はEditMode243／PlayMode23で、**5BのUnity BuildAll・テスト・Windowsビルド・実画面・音は未検証**です。前フェーズClaudeの225/225・15/20・Windows成功と混同しないでください。GameFlowSceneTestsとBattleSceneTestsのIsolateInput/RestoreInput構造は保持しています。

再検証は `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Validation/RunPhase5B.ps1`。CPUの全9組×30seed×3難易度=810試合集計は [CPU_BALANCE_REPORT.md](docs/CPU_BALANCE_REPORT.md)。ふつう同士の超必殺発動試合率56.7%。Unityで生成した実データの集計は「CPU対戦集計」メニュー、または `AniBato.EditorTools.CpuBalanceReportBuilder.Write` で別結果を保存できます。Claudeの実機確認手順は [phase5b_final.md](docs/codex_logs/phase5b_final.md)。

設計は [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)、初期値は [BALANCE_CONFIG.md](BALANCE_CONFIG.md)、確定・暫定ルールは [GAME_DESIGN.md](GAME_DESIGN.md)。コミット・公開はこの作業では行いません。

## ART2：3キャラのドット絵

全員23アニメ。5Bの中割り追加後は、透真が別エフェクトを含む26シート125フレーム、蓮24シート118フレーム、千鶴26シート125フレームです。作業結果は [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)、Unity検証手順は [phase5b_final.md](docs/codex_logs/phase5b_final.md)。一覧は [透真](docs/art_wip/toma_contact.png)・[蓮](docs/art_wip/ren_contact.png)・[千鶴](docs/art_wip/chizuru_contact.png)、身長差は [3人の並び](docs/art_wip/all_characters_lineup.png) で確認できます。シェフ服Portraitは将来のAwakening用、千鶴は私服です。将来キャラ用に仮図形フォールバックを残しています。

加工済みPNGは同梱済みです。原画変更後は、リポジトリ直下で次を実行してからBuildAllしてください。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildPhase5B.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase5B.ps1 -Rebuild
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt2Previews.ps1
```

ツールの詳細は [tools/SpriteForge/README.md](tools/SpriteForge/README.md)。基準idleは透真124px/千鶴112px/蓮100px、224×160セル、各キャラ共通32色、PPU50です。現行368フレームの加工検査、42GIFの全コマ復号、217ファイル再生成のSHA-256一致は合格。5BのUnityインポート／実動作はClaude待ちで、.NET成功をUnity合格とは扱っていません。



