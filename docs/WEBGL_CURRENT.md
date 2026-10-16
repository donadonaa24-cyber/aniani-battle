2026-10-17現在：paper_single_title_3で全4合格（Edit543／Play68）。空中・下は各1枚、通常は共有4原画。千鶴の握りと集合絵v5を反映。paper_single_title_webも49.4MBで実ビルド・内蔵ブラウザ確認済み。次はPAPER_MINATO_TWELVE_KEYS。追加差分は追加指示後。

# WebGL版の実装と起動（2026-10-16）

2026-10-16最新：paper_release_5全4合格（Edit537／Play66）、投げ終わりと2段選択・掌側の二丁拳銃v3を反映。WebGLもpaper_release_web成功・約47.7MB、ブラウザ確認済み。次は蓮の残10原画の審査／接続。全9人の攻撃・小物・溜め・被弾ゴールは継続。詳細はRESUME冒頭とpaper_release_final。

R5の生成3舞台・9人集合と同じゲームを、PCブラウザ向けに実装。成果物は `Build/WebGL/`。外部公開はしていない。ソースはGitで保存し、生成済みビルドと一時キャッシュはGit管理外。

## 起動と再生成

プロジェクトのルートで実行する。Unityをこのプロジェクトで開いている場合は先に閉じる。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify/build_webgl.ps1 -Tag webgl -RestoreWindows
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify/start_webgl.ps1
```

既存ビルドを遊ぶだけなら2行目のみ。表示された `http://127.0.0.1:8799/` を開き、「ゲームを読み込む」→「音を有効にして開演！」。ファイルを直接開かずHTTPを使う。サーバーはループバック限定、非表示で起動する。停止する場合は `TestResults/webgl_server.pid` のプロセスを確認して止める。他のアプリのプロセスを終了しない。

UnityビルドはライセンスIPCのためサンドボックス外で実行する。`-nographics` なし。Editorの「あにバト/WebGLをビルド」でも再生成可能。Windowsの全4工程は従来どおり `tools/verify/run_unity.ps1 -Tag <作業名>`。

## ブラウザの操作

| 操作 | P1 | P2 |
|---|---|---|
| 移動 | WASD | 矢印 |
| ジャンプ | Space | テンキー0 |
| 通常／特殊 | J／K | テンキー1／2 |
| ガード／回避／カウンター | L／左Shift／I | テンキー3／4／5 |
| 超必殺／ダッシュ | U／F | テンキー6／テンキーEnter |
| ポーズ | P | O |

選択中Mで技表。対戦中はポーズ→技表。練習は1速度・2静止・3コマ送り・4判定表示。決定JまたはEnter、取消KまたはEsc。ブラウザ予約キーと修飾キーをWeb側の割当候補から除外し、Windowsの割当は保持。Webのキー保存は別のPlayerPrefsキーを使う。

上部の全画面ボタンは舞台と解除ボタンを一緒に広げる。Fullscreen APIが使えない埋め込み環境ではページ内の拡大表示を使う。Escまたは上の解除ボタンで戻る。キャンバスからフォーカスが外れると試合を停止し、入力ラッチをクリアする。再開は画面クリック後P／メニュー決定で明示する。スマートフォン用タッチ操作は未実装。

音はユーザー操作後にAudioContextを開始。既存の自作BGM／SEのPCMをAudioClipへ入れて使う。音量・キー等はPlayerPrefsとIndexedDBへ保存し、テンプレートは `autoSyncPersistentDataPath` を有効にする。WebのFPS設定はブラウザ同期を表示する。日本語は再配布可能なNoto Sans JPを同梱し、ライセンスを出力にもコピーする。

## 画像と容量

WebGLビルド時だけScriptableObjectとSpriteのコピーを作り、未使用の旧方向攻撃・紙絵で置き換わった旧アニメ・カバン投射参照を外す。元アセットと予備カバン、戦闘定義は変更しない。

1304×1450など4の倍数でない原画は、右と上に透明画素を足したキャッシュへ梱包する。人物の再サンプルはせず、Sprite.rect・支点・PPU・ワールド寸法を維持。顔領域と盤面36セルはテクスチャ全体でなく元Sprite.rectを使う。キャッシュは `AniBato/Assets/_WebGLPacked/`（Git管理外、再生成可能）。274画像の梱包前後RGBAを全照合済み。GPU用DXT5は非可逆圧縮であり、実表示の全画素一致を主張するものではない。

最終成果物は約47.4MB、全6ファイル。Unityのgzip＋decompression fallback、ハッシュ付きファイル名、データキャッシュ、初期メモリ256MB／最大2048MB。ローカル配信はwasm／JSのMIMEを設定し、`.unityweb` はUnity側で展開するためContent-Encodingを付けない。通常の `.gz`／`.br` を配信する場合のヘッダーも用意してある。

## 実動確認と範囲

実Unity `webgl_native_3` はBuildAll成功、EditMode **533/533**、PlayMode **61/61**、Windowsビルド成功。失敗・スキップ0。Web用の追加6検査で予約キー、フォント字形、複製前後の戦闘不変、RGBA透明梱包、支点と寸法、顔領域を検査した。

実ブラウザはCodex内蔵のChromiumで確認した。検証表示は `?verify=1` のときだけ表示し、実C#状態を読む。シミュレーションを注入して成功扱いにはしていない。

- タイトル→同じ集合絵のトップ／9人選択、作品別の位置、P1とP2各3人、次ラウンドと2対1の決着、再戦。
- 開始中tick0の操作禁止、両キーボードの移動／通常、ポーズ停止、場外KO、70秒の時間切れと縮小5段階。
- 未来／将棋／文化祭の3舞台。将棋6×6と生成足場、カレー超必殺。練習の静止と1tick送り、超必殺を繰り返し使用。
- 音声開始状態running、BGM変更とキーJ→Hを保存、ページ再読込後に保持。試験後はBGM0.35・通常Jへ戻した。キャッシュ済み再起動は約2秒（操作ツール往復を含む参考値、初回ダウンロード時間とは別）。
- 仮想の標準Gamepad APIで接続／選択／切断、その後キーボードへ復帰。これは物理機器の確認ではない。
- 1280×900／1280×960／1440×900の窓、全画面の入力と解除、フォーカス停止と明示再開。描画は約138〜144fpsの環境で、戦闘は固定60Hz。

**個別のデスクトップChrome／Edgeと、実機のXbox／DualSenseは未確認。** ブラウザ接続機能がこの環境で利用できず、ネイティブアプリ操作も承認待ちのタイムアウトとなったため、内蔵Chromiumで検証した。別ブラウザ／実機パッドでの互換性確認は残る。外部サイトへの公開・ポータル変更は実施していない。

スクリーンショットは `docs/screenshots/rework_r6/`。全画面切替の旧実装で描画停止を発見し、UnityのSetFullscreen呼び出しからDOM側の切替へ修正。その他の試験中の待機タイムアウトは画面の状態通知待ちと超必殺モード名の取り違えであり、実行時例外とは区別する。詳細と最終タグは `codex_logs/rework_r6_final.md`。
