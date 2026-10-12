# あにあにバトル！！（あにバト！！）

オリジナル2D横スクロール対戦アクション。透真・蓮・千鶴によるCPU対戦・同一PCの2人対戦、練習とチュートリアルを備えます。現行ルールは[GAME_DESIGN](GAME_DESIGN.md)、原本[SPEC_ORIGINAL](docs/SPEC_ORIGINAL.md)は履歴として保持しています。**ART3B完了：3人の新しい呼吸待機、BuildAll成功・Unity EditMode323/323・PlayMode29/29・Windowsビルド成功。** 最新手順とCodex/Claudeの交代記録は[RESUME](docs/RESUME.md)。

待機中は足を接地させたまま体と頭が上下±2px動きます。全員224×160px/コマ、8F/6fps。[動くプレビュー](docs/art_wip/art3b_previews.md)、[実ゲーム撮影](docs/screenshots/art3b_previews.md)、[実装用PNGとGIF](art_source/art3_idle/breathing/README.md)。今後も[同じ制作規格](docs/SPRITE_SPEC.md)を使います。待機以外の動作は既存の絵柄を保持しています。

メインメニューの「練習モード」は、相手を棒立ち/ガード/ジャンプ/ふつうCPUから選び、ハート・時間固定を切り替えられます（ポーズ→練習設定）。入力と技名を表示します。「チュートリアル」は14段階の実操作課題。技表はキャラ選択のTab（パッドSHARE/BACK）かポーズから開き、左右でキャラ・上下でページを切り替えます。「キーコンフィグ」はP1/P2・キーボード/パッド別に変更・保存・初期化できます。重複は同じ設定内で入替。ボタン表記は最後に使った機器と現在の割り当てに自動追従します。

| 追加通常 | 入力（前後はキャラの向き基準） | 特徴 |
|---|---|---|
| 足払い | 地上↓＋通常 | ガード崩し＋被弾カウント1 |
| 背面打ち | 地上背後＋通常 | 背後へ打撃、発生やや遅め |
| 空中ニュートラル | 方向なし＋通常 | 周囲へ打撃 |
| 空中前/後ろ | 前/背後＋通常 | 前は前進、後ろは向きを維持 |
| 空中上/下 | ↑/↓＋通常 | 下は着地隙18F、メテオなし |
| 空中斜め前上/下 | 前＋↑/↓＋通常 | 両軸0.45超、前進を伴う。下は着地隙14F |

下入力でしゃがみ、上足場から降りるには下を18F長押し。空中では向きを変えません。スタミナは透真100/蓮85/千鶴125。地上ジャンプは不足でも可能、上復帰は消費なし。ガードは全攻撃を防ぎ、保持6/秒＋成立時 通常8/特殊20/超必殺65を消費。通常/特殊で不足すると無被弾ブレイク、超必殺で不足すると貫通しハート2。しゃがみ攻撃はジャストガードも崩し、ジャストガードはその他の技を消費0で防ぎます。対戦の基本操作ヒントは最初5秒だけ（ポーズでOFF可）。

オフライン再検証：`powershell -NoProfile -ExecutionPolicy Bypass -File tools/Validation/RunPhase6B.ps1`。307件成功、参照コンパイル2系統は警告/エラー0、asmdef6件。CPU810試合の自滅近似0、ふつう平均48.09秒・縮小23.7%・超必殺65.6%。素材584F・106GIF。[背景/カメラ比較とwalk/dash](docs/screenshots/phase6b_previews.md)、[全GIF一覧](docs/art_wip/phase6a_previews.md)。

## 起動

検証済みWindows版は **`Build/art3b/AniBato.exe`** を起動します。ビルドはGit管理外で、検証したPC上に保存されます。最新2ビルドのみ保持するため、過去の実行ファイルは削除されることがあります。

1. Unity Hubから **Unity 6000.6.0f1** で `AniBato/` を開きます。
2. 更新後はメニュー「**あにバト → プロジェクト生成（フェーズ6B）**」を実行して、方向AttackData・スタミナ・アニメ・新背景/サムネ・タイトルと対戦シーンを生成します。
3. `Assets/_Project/Scenes/Title.unity` を開き、再生します。Windowsの起動シーンもTitleです。
4. 任意キーでメニューへ。W/Sで「2人対戦」を選びEnter。P1はA/D→J、P2は←/→→Deleteでキャラを決定します。
5. ステージをA/Dまたは←/→で選びEnter。3、2、1、FIGHTの後に70秒・ハート3で対戦します。試合終了後はリザルトの「再戦」を選びます。

メインメニューとポーズの「フレームレート」でモニター同期／60／120／144／無制限を選択できます。既定はVSync同期、設定は端末保存。ハートを失った直後は75Fの立て直し無敵を点滅表示します。確定ダメージは通常3回でハート1、特殊1、超必殺2のままです。

5Bの過去集計は平均45.51秒・縮小21.9%・超必殺56.7%。6Bでも6Aの戦闘数値とCPU集計を保持。[CPU_BALANCE_REPORT](docs/CPU_BALANCE_REPORT.md)、実機確認は[phase6b_final](docs/codex_logs/phase6b_final.md)を参照してください。

再生成は既存の調整用ScriptableObjectの値を保持します。シーン構成は `ProjectBuilder` と `GameFlowBuilder` に記述してください。Build SettingsはTitleが先頭、Battleが2番目です。Battleは戦闘回帰テスト用の直接起動シーンとして残しています。

初期選択は **P1=透真、P2=蓮**。キャラ選択はK/Endで決定解除、未決定ならメニューへ戻ります。パッドは各自の左スティック/十字キー＋南ボタンで決定、東ボタンで解除/戻る。CPU対戦はメニューの「CPU対戦」から始めます。P1 W/Sで自分・CPU・難易度の行を選び、A/Dで変更、Enterで決定します。やさしい／ふつう／むずかしいを選べ、P2はCPUが操作します。CPU戦中のP2キーボード／パッド入力は戦闘に渡しません。

EscまたはパッドStartでポーズ。再開／操作説明／キャラ選択／タイトルを選べます。ポーズ中と操作説明中は戦闘tickを止め、復帰時に停止分を追い付き処理しません。リザルトは再戦／キャラ選択／タイトル。再戦は同じキャラ・ステージを使い、ハート・ゲージ・被弾・縮小を初期化します。ランダム選択も再戦時は決まった同じステージを使います。

3ステージは星の終わり／推し駒Battle／Battle à la carte。開始55秒で予告、60秒で縮小開始、61.8・63.6・65.4・67.2・69秒で左右の床が消えます。各消滅の1秒前から対象区画が点滅します。上足場は63.6秒で消え、最後は中央幅10.8の足場のみ。床の消失は落下を起こし、場外ラインを超えて敗北します。

## 操作

以下は既定の割り当てです。ゲーム内の操作説明は保存済みの現在の割り当てに追従します。「左右2回入力のダッシュ」はメインメニュー・ポーズ・キーコンフィグからON/OFFを切り替えられます（既定ON、P1/P2共通、端末保存）。OFFでもF／右Alt／L3の専用ボタンは有効です。割り当て初期化はこのON/OFF設定を変更しません。ステージ選択は左右、上下でも選択できます。

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
| しゃがみ / 急降下 / 小足場から降りる | S（降下は18F長押し） | ↓（同じ） | — | 下入力（同じ） |
| ポーズ・再開 | Esc | Backspace | — | OPTIONS / Start |
| メニュー決定 | Enter / J / Space | Delete / 右Ctrl | テンキー0/1 | 南ボタン |
| メニュー戻る・決定解除 | Esc / K | End | テンキー2 | 東ボタン |
| 判定枠表示（Hurtbox緑/Hitbox赤/弾黄） | F9 | 共通 | — | キーボードF9 |
| 全画面 / ウィンドウ（プレイヤー） | F11 | 共通 | — | キーボードF11 |

地上ダッシュは進行方向の保持で継続します。空中ダッシュは着地まで1回。上特殊も着地まで1回。P2はテンキーなしで操作でき、ノートPCによってDelete/Home等にはFn併用が必要です。`InputBindings.asset` の各PrimaryとAlternate配列で1行動に複数キーを指定できます。キーボードの同時押し制限は機種に依存します。

通常/特殊/超必殺は必要スタミナがあればガード可能、しゃがみだけがガード崩しです。カウンターは近距離通常に有効。超必殺は不足したガードと通常回避を貫通し、直前5Fのジャストガード／回避で防げます。ガード解除・ブレイク後などには硬直があります。3連通常でCOMBO FINISHと大吹っ飛び、同じ相手からの追撃を短時間禁止します。

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
& $unityExe -batchmode -quit -projectPath $projectPath -executeMethod AniBato.EditorTools.ProjectBuilder.BuildAll -logFile (Join-Path $resultPath 'phase6b-build.log')
& $unityExe -batchmode -projectPath $projectPath -runTests -testPlatform EditMode -testResults (Join-Path $resultPath 'phase6b-editmode.xml') -logFile (Join-Path $resultPath 'phase6b-editmode.log')
& $unityExe -batchmode -projectPath $projectPath -runTests -testPlatform PlayMode -testResults (Join-Path $resultPath 'phase6b-playmode.xml') -logFile (Join-Path $resultPath 'phase6b-playmode.log')
```

通常の全検証は `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag <作業名>` でBuildAll→EditMode→PlayMode→Windowsビルドを実行します。`-runTests` には `-quit` を付けません。**`-nographics` は使用しません**（Personalライセンスのヘッドレス権限不足で失敗するため）。Codexはサンドボックス外での実行を承認申請し、Claudeへ交代する場合も同じ手順・証跡を使います。代替検証は [tools/Validation/README.md](tools/Validation/README.md) を参照してください。.NETだけで完了扱いにせず、手動の操作感・画面確認と自動テストの結果を区別します。

フェーズ6Bの.NET代替は307件成功、全C#とプレイヤー側の参照コンパイルは警告・エラー0。asmdef6件の参照確認済み。**Unity実結果はEditMode320成功／PlayMode29成功、失敗・スキップ0**（TestResults/phase6b_EditMode.xml・phase6b_PlayMode.xml）。GameFlowSceneTestsとBattleSceneTestsの入力分離/復元構造を保持し、機器の後始末を補強しました。

再検証は `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Validation/RunPhase6B.ps1`。CPU全9組×30seed×3難易度=810試合集計は[CPU_BALANCE_REPORT.md](docs/CPU_BALANCE_REPORT.md)。Unity生成済みデータは「CPU対戦集計」メニュー、または `AniBato.EditorTools.CpuBalanceReportBuilder.Write` でphase6b-cpu-unity.*へ別保存できます。

設計は [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)、初期値は [BALANCE_CONFIG.md](BALANCE_CONFIG.md)、確定・暫定ルールは [GAME_DESIGN.md](GAME_DESIGN.md)。Unity全検証が合格した区切りでcommit・origin mainへpushします。次の作業前に指示書を作り、同じフォルダで同時編集しません。実キー入力を送るplay_and_capture.ps1の前は、必ずユーザーへ「PCに触らないで」と声をかけます。

## ART2：3キャラのドット絵

各キャラに方向10クリップを追加し、33アニメ・34状態に対応。別エフェクトを含め透真36シート197F、蓮34シート190F、千鶴36シート197Fです。Win/Loseをidle基準に作り直し、旧重ね合わせ処理を廃止しています。[106GIF](docs/art_wip/phase6a_previews.md)、[透真](docs/art_wip/toma_contact.png)・[蓮](docs/art_wip/ren_contact.png)・[千鶴](docs/art_wip/chizuru_contact.png)のcontactで確認できます。千鶴は私服、シェフ服Portraitは将来用。新規攻撃は描き下ろした1つの綺麗なキーを保持して再生するため、細かな動作の描き足しは今後の改善余地があります。

加工済みPNGは同梱済みです。原画変更後は、リポジトリ直下で次を実行してからBuildAllしてください。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildPhase6B.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase6A.ps1 -Tag phase6b -Rebuild
```

ツールの詳細は[tools/SpriteForge/README.md](tools/SpriteForge/README.md)。基準idleは透真124px/千鶴112px/蓮100px、224×160セル、各キャラ共通32色、PPU50です。現行584F、106GIFの全コマ復号、351ファイルの再生成SHA-256一致を検査。6BのUnity生成・アセットテスト・シーンテストは合格済み。新中割り・144Hz・物理パッドの手動所見は最新のRESUMEを参照してください。



