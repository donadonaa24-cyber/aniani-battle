# ART3B：共通サイズの呼吸待機

`BuildArt3Idle.ps1` は採用済みART3静止絵から体と頭が上下±2px動く8F/6fpsの待機を作り、全3キャラのidleへ接続する。靴の下12行を固定し、顔・衣装は整数行移動で保持する。生成した呼吸4キーは動きの参考として保存する。前後画像の混合はしない。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1 -RebuildCheck
```

全キャラ・全モーションのセルは224×160px、足元159、PPU50に固定。[制作規格](../../docs/SPRITE_SPEC.md)と[実装用出力](../../art_source/art3_idle/breathing/README.md)を参照。`prebuiltSheet` は規格済みシートを1クリップだけ差し替える。旧raw idleの縮尺・共通パレット基準は残すため、他のモーションの出力は変わらない。再生成後はUnity BuildAllが必要。BuildPhase6A/5Bは過去フェーズ再現用であり、現行待機を戻したい場合はBuildArt3Idleを最後に実行する。

# フェーズ6Bの生成経路（履歴）

`BuildPhase6B.ps1` は生成済み `art_source/phase6b` の背景3枚・中割り3アトラスを読み、背景1280×720・サムネ480×270とwalk/dashの追加原画を生成する。旧rawは保持し、各人walk2/dash4の独立した新姿勢を旧キーと順次配置する。混合・クロスフェードなし。manifestのsourceClipで新素材へ接続し、全106シート584F/106GIFを出力する。

`VerifyPhase6A.ps1 -Tag phase6b -Rebuild` は既存の品質検査に加え、6クリップの新素材接続・8F・dash8種類/walk6種類以上を検査し、351ファイルのSHA-256一致を確認する。プレビュー比較は `RenderPhase6BPreviews.ps1`、生成プロンプトは `art_source/phase6b/generation-log.md`。原画を作り直す必要はなく、同梱素材から再現できる。

## 6Aの方向通常生成（履歴・原画保持）

`BuildPhase6A.ps1` は `art_source/phase6a` の内蔵image_gen原画を連結成分で分割し、新規10クリップ/キャラを生成する。元の顔・衣装参照は採用idleだけ。前後のコマを重ね合わせる処理は使用しない。通常モーションは1つの綺麗なキーを保持して再生し、idle/crouch/win/loseは単一原画から上部を1pxだけ動かす。勝敗の頭位置差を含めidle比2px以内、胴と足は固定する。

全106シート584F、106GIF。`VerifyPhase6A.ps1 -Rebuild` は寸法・二値α・共有32色・足元・単一原画の胴/足/頭・控えめな結果姿勢・全GIF復号・SHA-256再現性を検査する。プロンプトは `art_source/phase6a/prompts.json`、一覧は `docs/art_wip/phase6a_previews.md`。以下の古い中割り説明は履歴で、現行生成経路へ再導入しない。

# SpriteForge（Windows・外部パッケージ不要）

リポジトリ直下で Windows PowerShell 5.1 から実行します。C#の画像処理をAdd-Typeでコンパイルし、System.Drawing／System.Web.Extensionsだけを使います。Unityやネットワーク、APIキーは不要です。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/SpriteForge.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/SpriteForge.ps1 -VerifyOnly
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/SpriteForge.ps1 -SelfTest
```

既定の入力は `art_source/chizuru.manifest.json`。別キャラは `-Manifest art_source/toma.manifest.json` 等で処理できます。マニフェストのsourceDirectory/outputDirectory/contactPathはリポジトリ相対で、フォルダ外への書き込みは拒否します。原画は読み取りのみです。

処理はクロマキー→小さな孤立点の除去→不透明列の切れ目による自動分割→共通縮尺でのブロック最頻色縮小→キャラ共通の重み付きmedian-cutパレット→アンカー配置です。分割数が違えば停止します。セル数から推測して切りません。輪郭が多い縮小ブロックは暗色を優先し、バイリニア補間・ディザ・半透明は使いません。

千鶴の設定は224×160セル、足元y=159、PPU=50、基準idle身長112px、キー許容幅90、パレット32色。idle原画の基準身長675pxから **112/675 = 0.1659259259** を求め、すべてのアニメ・小物へ同じ縮尺を適用します。フレームやクリップごとの拡縮はありません。空中・小物は中心で配置します。キャラはpivot(0.5,0)、小物は中心を原点にするpivot(0.5,0.35)です。セル幅224は伸ばした空中蹴りを切らずに収めるためです。

水平アンカーは、胴体の色領域から検出した同じ解剖学的基準点をセル中央へ配置します。伸ばした手足で変わるシルエットの中央は使いません。`anchorRgbMin`／`anchorRgbMax`／`anchorRedGreenRatio`をキャラごとに設定できます。特殊な配色や小物には、各検出フレーム左端からのピクセル座標を `sourceAnchors` で指定できます。小物はシルエットの中心を使います。

`anchor`はfeet/center/effect。各アニメのframes/fps/loop/bindings（状態・クリップ・持続時の絵番号）を出力JSONへ転記します。`headHeights`は配置後の不透明な上端で、頭上UI位置に使います。`forge-report.json`に全原画範囲、縮尺、パレット、アンカー、配置量、各絵の全高を記録します。`validateHeight`の±2px判定は基準idleに適用します。屈伸・空中姿勢・頭上の鍋やおたままで全高112pxへ強制しません。

検証は全85フレームのマゼンタ残り0、二値アルファ、空フレームなし、キャラ全体で32色以下、地上全フレームの足元、基準idle身長、PNG寸法とマニフェスト・JSONの一致を確認します。合格後に `docs/art_wip/chizuru_contact.png` と5行ごとの分割一覧を2倍ニアレストで生成します。

原画・マニフェスト・処理コードを保存してあるため再加工できます。画像生成自体は確率的です。最終プロンプトと生成履歴は `art_source/generation-log.json`。生成リトライの原画はUnity外の `art_source/rejected/` に保持し、Git対象から除外しています。

## ART2

透真はtoma.manifest.json、蓮はren.manifest.json。目標idle124/100px、共通縮尺124/682と100/663。224×160、足元159、PPU50、キャラ共通32色です。水平の色アンカーは白シャツ/クリーム上着に設定。透真26シート85F、蓮24シート75F。蓮walk4Fとattack3 3Fで重なりを避けました。全一覧toma_contact.png/ren_contact.pngと分割ページを出力します。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/SpriteForge.ps1 -Manifest art_source/toma.manifest.json
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/SpriteForge.ps1 -Manifest art_source/ren.manifest.json
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/ValidateArt2.ps1 -Rebuild
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt2Previews.ps1
```

ValidateArt2は既存千鶴も含む245Fの原画分割、24状態の一意対応、持続姿勢の添字、headHeightsと実上端を検査。-Rebuildは透真/蓮の102PNG/JSON/報告ファイルを再加工してSHA-256一致を検証します（千鶴は検査のみ）。docs/art_wip/art2_inventory.jsonに原画/出力のサイズ・ハッシュを出力。再現性の証跡はTestResults/art2-spriteforge-reproducibility.jsonです。

BuildArt2Previewsは3キャラのidle先頭を同じ3倍ニアレスト・同じ足元へ横並びにして、docs/art_wip/all_characters_lineup.pngを生成。個々のキャラを同じ身長に拡縮しません。またOS既存WIC WebPデコーダで正式透真キービジュアルの512×768カットインコピーを生成します。追加ダウンロードは不要です。加工済み画像は同梱しているため、Unity BuildAll自体はWICに依存しません。

制作枚数・全プロンプトはart_source/art2-generation-log.json。Unity確認はdocs/codex_logs/art2_final.mdを参照。SpriteForgeの画像検査とUnityインポート/テスト成功は別です。

## フェーズ5B（現行）

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildPhase5B.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase5B.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase5B.ps1 -Rebuild
```

`frames`は原画の分割数、`inbetweenFrames`は加工後のコマ数（最大8）。両者を混同しない。`BuildPhase5B`は旧加工済み素材を`art_source/phase5b_keys/`へ初回だけ保管し、3キャラを再加工する。各idle/walk/dashは8F、jump/fall/attack1〜3/air_attack/特殊4種/guardは6F、idle12fps・他15fps。変更42クリップ、全出力は透真125F／蓮118F／千鶴125F、計368F。上記ART1/ART2の85/75/245Fは当時の記録。

中割りは`AnimationSmoothing.cs`の自作画像処理で、隣接キーの同じ不透明ランを対応付けて生成する。透明な隙間をまたぐ変形や色の混合を避け、体の微小なずれを整合する。idleは先頭1キーから上部±1pxの呼吸だけを作り、胴・足を固定する。原画の大きな姿勢差やランの数が変わる箇所は近いキーを保持するため、すべての輪郭変化が連続になるわけではない。AI新規生成は行わない。

持続姿勢の添字・headHeightsを出力コマ数に合わせて再計算し、TimingはUnity側の既存データを維持する。32色、二値アルファ、地上の足元、原画分割数、状態対応の既存検査を保持。`VerifyPhase5B`はさらにidleの胴・足完全一致／頭頂±1pxと、42GIFの全フレームの実デコードを検査する。`-Rebuild`は全PNG/JSON/contact/GIFを再生成しSHA-256一致を確認し、`TestResults/phase5b-spriteforge-reproducibility.json`へ記録する。

確認用GIFは`docs/art_wip/<キャラ>_<アニメ>_preview.gif`、contactは`<キャラ>_contact.png`と分割ページ。ゲームはPNGのみをインポートする。現在はCodexがUnity BuildAll／実アニメを検証し、Claudeへも交代できる。
