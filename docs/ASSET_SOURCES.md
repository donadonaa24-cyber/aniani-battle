# 素材の出典・用途

## フェーズ6B（2026-10-12）

内蔵image_gen（APIキー/CLIなし）で背景3枚と中割り3アトラスを生成。背景は参照画像なし、本作独自の都市・巨大駒闘技場・文化祭。原作正史を追加確定しない。中割りは各キャラの所有walk/dash原画を参照し、同じ服・顔・等身の独立6姿勢/人を描いた。前後コマの混合・クロスフェードなし。他者ファンアート・第三者/有料素材は未使用。

| 保存先 | 元画像・加工 | 用途 |
|---|---|---|
| art_source/phase6b/backgrounds/{city,board,festival}.png | 内蔵image_gen新規生成、原画を保持 | 3ステージ背景 |
| Art/Stages/phase6b_*_background.png・*_thumbnail.png | 原画の縮小と自作足場サムネ、1280×720/480×270 | FarArtwork/Thumbnail。旧サムネ保持 |
| art_source/phase6b/motion/{toma,ren,chizuru}.png | 所有rawのwalk/dashを参照したimage_gen、透過6姿勢/人 | walk2/dash4の中間姿勢 |
| art_source/raw/*/{walk,dash}_phase6b.png | 新姿勢の分離・倍率整合、旧キーと順次配置 | SpriteForge入力。旧原画は上書きしない |
| Art/Sprites/*/walk・dash PNG/JSON、docs/art_wip/*_preview.gif | 共通32色・二値アルファ・足元整合、1コマ1原画 | 各8F・15fps。既存idle/結果方式を維持 |
| docs/screenshots/phase6b_*.png | 旧/新サムネ、実CameraFramingのオフライン投影 | Unity実画面の代替であることを明記 |

最終プロンプト・保存先・加工方針は[生成記録](../art_source/phase6b/generation-log.md)。再開時に破損・品質を確認して生成済み素材を再利用した。背景/中割り以外の原画・ユーザー提供Portraitは維持。

2026-10-12（JST）。原作側の資料はユーザー提供・本人作品です。元の別ゲームフォルダには書き込んでいません。新たな第三者アセット・有料アセットは導入していません。

| 現在の保存先 | 元資料 | 用途 |
| --- | --- | --- |
| AniBato/Assets/_Project/Art/Characters/ren-dog-rook.png | 推し駒battle/Assets/Art/Characters/ren-dog-rook.png（ユーザー本人作品） | 蓮の超必殺カットイン。従来のコピーを維持 |
| AniBato/Assets/_Project/Art/Characters/Awakening/chizuru-battle-mode-icon.png | battle-a-la-carte - ギットハブ版 ユニティ/assets/images/battle-mode-icons/chizuru-battle-mode-icon.png（ユーザー本人作品） | シェフ服。将来の覚醒/シェフモード用。元PNGと.metaを移動し、通常のカットインには使用しない |
| AniBato/Assets/_Project/Art/Characters/Awakening/chizuru-chef-future.png | 上と同じ画像の保管コピー | 将来用。今回のランタイムから参照しない |
| art_source/raw/chizuru/idle.png | docs/art_wip/chizuru_idle_test.png（Claude試作、ユーザー採用済み） | 4フレームの待機。元画像の完全コピー |
| art_source/raw/chizuru/その他25PNG | docs/art_reference/characters/chizuru_casual_sheet.webp、chizuru_casual_fullbody.webp、採用済みidleの同じ3枚を毎回参照。内蔵image_gen生成 | 千鶴の私服22アニメ＋小物3種の原画 |
| AniBato/Assets/_Project/Art/Sprites/chizuru/26PNG＋JSON | 上記原画をtools/SpriteForgeで加工 | 実際の戦闘Sprite（85フレーム）。32色・二値アルファ・ドット化 |
| AniBato/Assets/_Project/Art/Characters/chizuru-casual-cutin.png | 千鶴のユーザー提供私服fullbody/sheetを基に内蔵image_genで新規生成 | 私服の通常超必殺カットイン。透過イラスト |

参照原画2枚が指定する茶色ボブ/茶色の目/袖を少しまくったオレンジの長袖/紺のフレアスカート/白いスニーカーを採用。顔・服・等身はその資料に従います。ジャブ・フック・蹴り・料理を使う戦闘モーション・各効果・勝敗ポーズは **本作独自のクロスオーバー表現** であり、原作の正史設定を追加確定していません。

`docs/art_reference/_style_local/pixel_style_reference.webp` は他者のファンアートとしてGit管理外・Assetsコピー禁止。画像生成入力・加工入力には使用していません。ART1では透真/蓮の設定画は未使用で、ART2から使用しています。千鶴のchibi資料も等身やポーズの入力に使用していません。

新規採用26枚、作り直し10枚、ツール出力計36枚。プロンプト・参照・生成履歴は `art_source/generation-log.json`。不採用原画は `art_source/rejected/chizuru` に10枚を保持し、Unity外/Git管理外です。原画は外部パッケージを使わず、System.Drawingによる処理でドット化しました。

SHA-256の保管確認：

- 蓮の従来コピー：`FE44D5D5FD75723153D7E65A9E252F2B0CF420B9B0528C9E7DB765B70B7B0FA1`（フェーズ3で元画像一致を確認）。
- Awakeningへ移したシェフ服：`7828151F8EEBE524F108C7FEF5CA5C7661656564334B277CEF4B6A8A57061C23`（ART1で変更なしを確認）。
- 採用済みidleとrawコピー：`EA24C8D831D02D98767DC931AE1618A02098CBBEF1A76E368B394B77E36B073C`（ART1で一致を確認）。

ART1の全原画・加工済みPNG・私服Portraitのサイズ/ハッシュは `docs/art_wip/chizuru_inventory.json`。透真/蓮はART2で実Spriteへ接続。背景・足場等は従来のProceduralSpritesの自作仮素材を維持。千鶴の仮コック帽は非表示です。フォントはUnity組込LegacyRuntime.ttf（仮）とOS既存のYu Gothic/Meiryo（日本語ゲームUI）、Consolas（制作一覧のみ）。音はフェーズ5で自作のコード合成を導入。

## ART2：透真・蓮

| 保存先 | 元資料・作成方法 | 用途 |
| --- | --- | --- |
| art_source/raw/toma/26PNG | toma_sheet.webp、toma_keyvisual.webp、採用idleを参照して内蔵image_gen。初回idleは千鶴の採用試作を画風参考に使用 | 透真23アニメ＋pulse/field/boundary_glow原画 |
| art_source/raw/ren/24PNG | ren_dog_rook.png、採用idle、千鶴の採用試作（画風のみ）を参照して内蔵image_gen | 蓮23アニメ＋cross_light原画 |
| AniBato/Assets/_Project/Art/Sprites/toma・ren | 上記原画をART1と同じSpriteForgeで共通倍率ドット化・32色・二値アルファに加工 | 透真85F、蓮75Fの実Sprite用PNG/JSON |
| AniBato/Assets/_Project/Art/Characters/toma-keyvisual-cutin.png | ユーザー提供toma_keyvisual.webpをWindows WICでデコードし半分の512×768へ縮小コピー | 透真の超必殺カットイン。元構図・絵を保持、AI新規生成ではない |
| docs/art_wip/all_characters_lineup.png | 加工済み3キャラidle先頭を共通3倍ニアレストで合成 | 身長差124/112/100pxの確認 |

Tomaのスーツ・端末とRenの犬耳フード/衣装/マント/尻尾は正式資料に従います。格闘・勝敗・端末パルス/解析フィールド/突進光/十字光は**本作独自のクロスオーバー演出**で、原作の超能力や正史設定として確定しません。エフェクト生成は採用idleの画風・パレットだけを参照し、キャラを描きません。Renのhitstun/koは原画の浮遊記号のみを内蔵image_genで除去する編集を行いました。他者のファンアートや第三者アセットは入力・Assetsに使用していません。

中断前6原画はSHA-256一致で再利用。採用50枚（再開後新規44枚）・作り直し9枚・ART2累計ツール出力59枚（再開後53枚）。不採用9枚はart_source/rejected/内にコピー保管し、Unity外/Git管理外です。蓮の従来カットイン、千鶴の原画・Portrait・シェフ服保管は保持。最終プロンプトと採否理由はart_source/art2-generation-log.json、全サイズ/ハッシュはdocs/art_wip/art2_inventory.json。

## フェーズ4：自作ステージ・UI

| 保存先 / 実装 | 作成方法 | 用途 |
| --- | --- | --- |
| Scripts/Game/StageBackgroundView.cs・StageView.cs | ProceduralSpritesによる独自シェイプ | 都市・6×6闘技盤面・文化祭調理場の遠景/中景/床。外部画像なし |
| Art/Stages/stage_0.png〜stage_2.png（BuildAllで生成予定） | Scripts/Editor/StageThemeBuilder.csがTexture2Dへ矩形描画 | 480×270のオリジナル仮サムネ。Point、無圧縮、Mipなし |
| タイトル・メニュー・操作説明・リザルト | GameFlowBuilder / MenuScreenViewのテキスト・uGUI | ロゴも文字。人物は既存ART2 idle/win/loseスプライトを再使用 |
| RuntimeFonts.Japanese | OSインストール済みYu Gothic → Meiryo → Arial | Windowsでの日本語UI表示。フォントファイルをコピー/再配布しない |

今回は画像生成ツール、原作背景画像、第三者素材を新規使用していない。背景の塔・駒・屋台・装飾・AI警告は**本作独自の仮表現**で、資料にない原作設定を正史として確定しない。BGMキーは用意するが音素材は未追加。

## フェーズ5：自作の音・描画

| 実装 | 作成方法 | 用途 |
| --- | --- | --- |
| Scripts/Presentation/ProceduralAudio.cs | 正弦波・倍音・固定seedノイズをコードで合成。外部録音なし | SE15種、メニューと3ステージの8秒ループ、16kHzモノラル |
| Scripts/Game/AudioManager.cs | 起動時に上記float波形をAudioClip.Create/SetDataへ渡す | イベント再生、BGM切替、音量 |
| Scripts/Game/BattleEffectsView.cs | 既存ProceduralSpritesの自作シェイプとUI文字 | ヒット／ガード火花、淡いフラッシュ、決着表示 |
| Scripts/Game/UltimateCutInView.cs | 既存ユーザー提供／採用PortraitとUI帯の拡大・登場 | 超必殺カットイン強化 |

メロディとSEは本作独自の仮素材で、原作の楽曲・設定として確定しない。新しい画像・音ファイル、第三者素材、有料アセット、外部パッケージは導入していない。音の原本はコードで、.NETで波形の範囲・再現性を検証。Unityでの実音は未検証。3キャラ画像・原画と既存Portraitは保持した。

## フェーズ6A：方向キー原画と控えめな勝敗（2026-10-12）

| 保存先 | 元資料・作成方法 | 用途 |
|---|---|---|
| art_source/phase6a/{toma,ren,chizuru}-atlas.png | 各キャラの自作/ユーザー資料に基づく採用idle原画だけを参照して内蔵image_genで新規キーを描画 | しゃがみ・足払い・背面・空中5方向・斜め前上、各9ポーズ |
| art_source/phase6a/diagonal-down.png | 同じ3人の採用idleを参照して内蔵image_gen | 斜め前下の3キー |
| art_source/phase6a/prompts.json | 実際のプロンプトと参考/採用ファイルを記録 | 再制作・出典の確認 |
| art_source/raw/<character>/新規10PNG | PhaseSixArtが不透明な連結成分から各ポーズを分離 | 原画の保管、前後の絵を混合しない |
| Art/Sprites/<character>/全PNG・JSON | SpriteForge、既存14クリップの綺麗なキー保持＋idleの単一原画呼吸 | 計106シート584F。旧混合フレームを廃止 |
| Win/Lose・docs/art_wip/*_preview.gif | idle原画の最初の1キーから胴/足を固定、頭位置±1px＋呼吸±1px | 結果画面と終了時の静かなポーズ、全106GIF |

画像生成は内蔵ツールだけを使用し、APIキー/有料アセット/第三者ファンアートは使用していない。格闘・スタミナ・勝敗の表現は本作独自で、原作の正史設定として追加しない。既存原画・以前の加工バックアップは保管した。新規技のコマは1つの綺麗なキーを保持し、既存モーションも一度に1キーだけ使う。旧処理は下の履歴に残すが、現行生成経路では使用しない。

## ART3・ART3B：採用済み待機原画と呼吸アニメ（2026-10-12）

| 保存先 | 元資料・作成方法 | 用途 |
|---|---|---|
| art_source/art3_idle/toma_idle_v1.png | docs/art_reference/characters/toma_sheet.webpを容姿/衣装の参照にして内蔵image_gen | 透真の右向き静止待機。ART3Bの基準絵 |
| art_source/art3_idle/ren_idle_v1.png | docs/art_reference/characters/ren_dog_rook.pngを容姿/衣装の参照にして内蔵image_gen | 蓮の右向き静止待機。ART3Bの基準絵 |
| art_source/art3_idle/chizuru_idle_v1.png | docs/art_reference/characters/chizuru_casual_sheet.webpを容姿/衣装の参照にして内蔵image_gen | 千鶴の私服・右向き静止待機。ART3Bの基準絵 |
| art_source/art3_idle/breathing/generated/*_breathing_atlas.png | 上記静止絵を参照した内蔵image_gen。最終プロンプトはbreathing/generation-log.json | 各4キー、呼吸の動きの参考。ゲームへ直接使用しない |
| Art/Sprites/<character>/idle.png・idle.json、breathing/ready/<character>/ | 上記静止絵をSpriteForgeで整数行移動、既存共通パレットへ減色 | 各8F/6fps、224×160/コマ、32色以内・二値alpha、靴固定・体±2px |
| docs/art_wip/art3b_*_idle_preview.gif・art3b_idle_lineup.png | ゲーム用PNGの整数3倍表示 | 全員同倍率・同足位置の確認用。ゲーム素材は等倍 |
| art_source/art3_idle/generation-log.json・README.md | 最終プロンプト・参照の役割、寸法とSHA-256を記録 | 再制作・Claudeへの交代資料 |

共通の画風参考はユーザー添付のピクセルアート比較画像。ドットの輪郭・色面・陰影だけを参考にし、画像の騎士・剣・衣装・SNS画面は転載していない。比較画像そのものをリポジトリへ保存・Assetsへコピーしていない。生成原画は保持し、ART3Bでゲーム用idleへ接続。生成4キーの細部変化を避け、最終8コマは各1枚絵から作る。旧rawは他モーションの縮尺・パレット基準として保持。衣装と容姿は正式資料、本作の待機姿勢と呼吸は独自表現。[固定制作規格](SPRITE_SPEC.md)、[実装用出力](../art_source/art3_idle/breathing/README.md)を参照。

## フェーズ5B：既存キーフレームからの中割り・idle整合（履歴、6Aで混合を廃止）

| 保存先 | 元資料・作成方法 | 用途 |
| --- | --- | --- |
| `art_source/phase5b_keys/{toma,ren,chizuru}/` | ART2までの加工済みPNG／JSONの保管コピー。raw原画は変更しない | 変更前のキーポーズ比較・復元資料 |
| `tools/SpriteForge/AnimationSmoothing.cs` | 既存原画を従来と同じ縮尺・32色へ加工した後、2キーの局所不透明ランを対応付けて中割り。足元・微小な頭／胴ずれを整合。大きな形状差は近いキーポーズを保持 | 各14クリップを6〜8Fへ増量。idleは1枚の基準から頭・腕など上部を±1px、胴と足は固定 |
| `AniBato/Assets/_Project/Art/Sprites/{toma,ren,chizuru}/` | 上記自作C#処理、二値アルファ・共通パレット・PPU50・224×160セルを維持 | 透真125F／蓮118F／千鶴125F（全368F） |
| `docs/art_wip/{toma,ren,chizuru}_<clip>_preview.gif`・各contact PNG | 同じ加工済みセルから自作GIFエンコーダ・ニアレスト一覧生成 | 更新42クリップの確認用。ゲーム内でGIFを使用しない |

今回の新規画像生成API利用・外部送信・第三者素材導入はない。衣装・キャラ設定は既存資料のまま。呼吸・格闘の中割りは本作独自の演出。局所形状の対応が難しい箇所にはキーポーズ切替が残り、原画自体の細部差まで解消したとは扱わない。
