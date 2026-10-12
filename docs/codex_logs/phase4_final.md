# フェーズ4 実装・検証引き継ぎ（2026-10-12）

対象指示書：docs/codex_tasks/PHASE4.md。ソース実装と.NET代替検証は完了。**このフェーズのUnity生成・実行・実機確認は未実施**。git commit/pushはしていない。

## 変更概要

StageShrinkController / StageGeometry / MovementSystemで共通の段階縮小と支持解除を実装。StageGeometryDataに共通地形を置き、StageDataに3作品のテーマ・サムネ・時間帯・BGMキーを持たせた。StageView / StageBackgroundViewはシミュレーション状態から描画する自作仮素材。

GameFlowController / MenuScreenViewがTitle→メニュー→2人/CPU待機のキャラ選択→3ステージ/ランダム→カウントダウン→対戦→リザルト→再戦/戻るを担当する。ポーズと操作説明では戦闘を止め、再開時の決定入力を戦闘から隔離する。リザルトはUI内のwin/loseアニメで両者を表示する。

ProjectBuilder + StageThemeBuilder + GameFlowBuilderがシーン・テーマ・共通地形・サムネを生成。Titleを先頭、Battleを回帰確認用に2番目へ登録する。ART2画像と戦闘性能は変更していない。新規asmdefなし、ClaudeのPresentation参照を維持。

## 代替検証

- 純C# **192成功 / 0失敗 / 0スキップ**：TestResults/phase4-dotnet-battle.xml・.log。
- Unity API参照の全ソース：警告0/エラー0、TestResults/phase4-dotnet-unity-compile.log。
- UNITY_EDITORなしのプレイヤー用ソース：警告0/エラー0、TestResults/phase4-dotnet-player-compile.log。
- asmdef参照：6アセンブリ、TestResults/phase4-assembly-references.json。
- Unity標準想定：**EditMode204件 / PlayMode17件**。これは合格済み件数ではない。

テスト中、既存時間切れ比較テストがコンボで端へ飛ばされた後に縮小で場外になったため、そのテストは縮小を無効にして比較規則を単独検査するよう変更した。縮小後の時間切れ・場外優先は別テストで検証する。消える床とのAABB接触が僅かに残る位置のテストも、完全に支持を失う位置へ修正した。消滅tickの接地解除時には地上ジャンプを消費済みとし、空中ジャンプ1回のみ残すことを追加検査した。

## ClaudeのUnity検証

プロジェクトを別Unityで開かず、サンドボックス外で実行する。-nographicsは付けない。

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase4
```

BuildAll→EditMode→PlayMode→Windowsビルド。TestResults/phase4_buildall.log、phase4_EditMode.xml/.log、phase4_PlayMode.xml/.log、phase4_build.logへ保存する。実行ファイルはBuild/phase4/AniBato.exe。追加スクリプトの.meta、Title、テーマアセット、サムネPNGはこのBuildAllで生成する。現時点の既存生成物を新機能の実行確認に使わない。

## 実機で見てほしい画面と操作

1. **タイトル/メニュー**：WindowsビルドがTitleから起動し、日本語ロゴ・Press Any Keyが見える。Enter→W/SでCPU/2人/説明/終了を選び、Enter決定、Esc戻る。パッド南ボタン/左スティック/東ボタンでも同じ操作。終了がWindowsではアプリを閉じること。
2. **2人キャラ選択**：P1 A/Dで選ぶ→Jで決定、P2 ←/→で選ぶ→Deleteで決定。K/Endで解除、未決定から押すとメニューへ。透真・蓮・千鶴のidle、名前、作品、タイプが表示される。パッド2台が別々の枠を操作し、両者決定後だけ進むこと。
3. **CPU選択**：メニューのCPU対戦→P1 W/Sで自分/CPU/難易度の行、A/Dで値を変えてEnter。難易度3種を往復し、再戦/キャラ選択で値が保持される。今回のCPUは待機する説明があり、P2の操作入力で動かないこと。
4. **ステージ選択**：左右で星の終わり/推し駒Battle/Battle à la carte/ランダム→Enter。3サムネと名前、選択状態が読みやすい。各背景で遠景/中景/床、視差、キャラに対する暗さを比較する。ランダムの再戦が同じ作品になること。
5. **開始とポーズ**：3・2・1・FIGHTの間は70秒・キャラ位置が動かない。開始後Esc/Startで止め、操作説明を開いても時間/状態を維持。説明から戻り、J/パッド南で再開して勝手な通常攻撃が出ない。Esc/Startでの直接再開、フォーカスを外した時のポーズも確認。
6. **縮小**：残り15秒で作品別警告、残り10秒で開始。61.8/63.6/65.4/67.2/69秒の1秒前から、消える左右区画が点滅する。上足場は62.6秒から点滅し63.6秒に消える。中央幅10.8に収束する。外側に立ったキャラが床消失で落下し、場外線までは即敗北しない。ジャンプ/上必殺での復帰を試す。各作品の電子点滅/盤面マス/文化祭減光が警告を隠さないこと。
7. **リザルト/再戦**：KO、場外、時間切れ、引き分けをそれぞれ確認し、勝者win/敗者loseと理由が一致する。場外の敗者もUI内に出る。W/S→Enterで再戦し、キャラ/ステージ維持、ハート3・被弾0・LINK0・70秒・床/上足場の復元。キャラ選択へ/タイトルへ戻ってまた対戦できる。
8. **表示/実パッド**：1920×1080、16:10、4:3、小さめのウィンドウ、F11で文字・絵・操作表が重ならない。Yu Gothic/Meiryoの日本語、ハート、矢印、à、パッド記号の欠落を確認。パッドの切断/再接続で片方が両者を操作しない。F9で床端・Hurtbox・絵を比較する。

## 既知の問題・残課題

CPU本体・BGM/SE再生・パッド再割当UIはフェーズ5以降。背景は自作仮素材。引き分けは両者loseの暫定演出。日本語フォントと画面比率、実デバイス、Unityシリアライズ・サムネインポート・PlayMode入力配送は未検証。直接起動Battleに旧F1〜F4/Rを回帰用に残したが、Title経由では無効。透真の目元と蓮の模様はユーザー判断で後回し。

結果が返ったらIMPLEMENTATION_STATUSとRESUMEにUnity合否・スクリーンショットを追記する。実機で確認が済むまでフェーズ4の「実際に遊べる」完了条件を検証済みと扱わない。
