# 素材の出典・用途

## 最新：千鶴の全紙動作をゲームへ接続（2026-10-14）

既存の自作原画34PNGから30人物姿勢と4効果を接続。元は内蔵image_gen、原画と全文条件はart_source/paper_cutout/generated/、参照の役割とSHAはruntime-manifest。手元参考1PNGはworking/に保持。第三者素材の追加なし。人物は共通PPU532.5893、1304×1450の透明枠へ余白梱包だけを行い、拡縮／切断なしを実Unityで全画素照合。原画自体の部位の不揃い／手の修正残は最終承認済みとせず明記して先行接続。

ゲーム用はAniBato/Assets/_Project/Art/Paper/pose_*とeffect_*、元待機も同一バイトで保持。確認フォルダ35コピーは原画同一、実Unity19PNGはdocs/screenshots/paper_chizuru_all_unity/で別物。実カメラ描画／Renderer拡大で、モニター撮影やOSキー送信なし。BuildAll／Edit447/447／Play38/38／Windowsビルド成功。

## 以下はゲーム接続前の素材記録

## 現在：千鶴の承認済み手元と空中前方v8（2026-10-14）

内蔵image_genで自作の前方v7を編集対象、ユーザー承認済みの自作手元参考working/air_forward_left_fist.pngを形状参考としてv8を生成。掌側4指と下の親指を反映し全身候補として保存、全文日本語条件と参照の役割は同名prompt.json。全身／ゲーム採用は未承認。実寸1243×1265、透明角と端切れなしを確認、部位倍率／共通セル／Unity接続は未審査。斜め下v3の左拳は別の未修正項目。第三者素材や手動の反転加工は使っていない。

ユーザーが修正前画像の削除を明示指定。旧原画41＋管理済み確認コピー41を累計削除し、相対パスとSHAをdeleted-old-images.jsonへ記録（分類変更の旧コピーを含む）。現在generated/は34PNG、working/の承認済み手元参考1PNG、確認フォルダは35コピー＋診断一覧1PNG。旧版フォルダは再生成しない。条件JSONは保持。診断一覧はUnityスクショではない。今回Unity追加実行0回。

## 以下は旧画像削除前の履歴

最新の追加修正（2026-10-14）：内蔵image_genで自作AirForward v4→v5→v6（掌側の左拳、親指欠落の指摘後に下側へ追加）、AirForwardDown v1→v2→v3（背中構図、指腹の指摘後に甲が見える左拳へ）を生成。旧原画と全文条件を保持し合計73PNG／73件。日本語名の確認用コピー74枚と35画像の縮小一覧を更新。部位倍率／共通セル／Unity接続の審査前の未採用候補。原画の加工・移動・削除なし、今回は実Unity未実行。

## 千鶴の画像名付き確認フォルダ（2026-10-14）

自作原画69PNG／69全文条件から、日本語の動作名付きの70コピーをイラスト確認/千鶴/へ作成。待機と歩きは同じPNGのためコピーが1枚多い。コピーを加工せずSHA照合、原画の移動／削除なし。35画像の縮小確認一覧は自作の名前・状態枠を添えた診断資料で、ゲーム用素材やUnityスクショとして扱わない。対応表review-catalogと再作成スクリプトを保存、ローカル複製はGit管理外。内蔵image_genでBackAttack v8の紙縁／AirForward v4の持ち手を追加修正し、旧版と全文条件を保持。新素材のゲーム接続／Unity検証は未完了。

## 千鶴の紙イラスト・持ち手の再修正（2026-10-14）

再開後に内蔵image_genで自作の保存済み原画を修正し、現在は67PNG／67件の全文日本語条件を保持。最新BackAttack v7はユーザー指定の上側の親指と甲が見える左手へ局所修正、カバンは右手。原画・参照の役割・全文条件をgenerated/、実寸／SHAをsource-index、採否をhand-reviewに保存。旧候補を削除せず、ゲーム差し替えと部位審査は未完了。今回はUnity未実行。以下の43件は停止時の記録。

## 千鶴の紙人形全アクション・停止時保存（2026-10-14）

`art_source/paper_cutout/generated/`に43PNGと43件の全文日本語プロンプトを保存。採用済み紙待機を本人／画風／部位寸法の参照として内蔵image_genで28姿勢・走り3版・4効果・7修正版を生成し、待機1枚も保持した。新しい原画は本作独自の演出候補で、部位比較／正式採否／Unity接続は未完了。修正前の原画も保存している。実寸／透明／SHAはsource-index.json、全条件はall-actions-plan.json、状態はdocs/codex_logs/paper_chizuru_all_checkpoint.md。

ユーザーの走り参考画像はポーズの考え方だけに用い、第三者のキャラ・透かし・元画像はゲーム素材や公開原画へ入れていない。新候補を旧ピクセル素材の224×160とは混同しない。共通1246×1263／待機由来PPUで、人物の拡縮なしに採用を確認する。今回の停止保存では生成・Unity検証を追加実行していない。

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

## ART3C：新しゃがみ・持ち出し用再制作資料（2026-10-13）

| 保存先 | 元資料・作成方法 | 用途 |
|---|---|---|
| art_source/art3_crouch/generated/{toma,ren,chizuru}_crouch_pair_v1.png | 各人のart3_idle/<キャラ>_idle_v1.pngを容姿/衣装、breathing/ready/<キャラ>/idle_00.pngを倍率/色の参照にして内蔵image_gen | 同倍率の立ち/しゃがみ2姿勢原画。立ちを倍率の物差しにする |
| art3_crouch/generation-log.json・normalization.json | 最終全文プロンプト/参照の役割、実分割矩形/共通倍率/配置の記録 | 再制作・Claudeへの交代資料 |
| art3_crouch/normalized/・ready/<キャラ>/、Art/Sprites/<キャラ>/crouch.png/json | ArtThreeCrouch/SpriteForgeによる同倍率正規化、既存全クリップ共通32色、単一姿勢の整数行移動 | 各8F/6fps、224×160/コマ。靴の下12行固定・体/頭±2px |
| docs/art_wip/art3c_*_crouch_preview.gif・art3c_pose_comparison.png | 採用済みゲームPNGの整数3倍表示 | 動作と待機/しゃがみの同倍率比較。Unity実画面ではない |
| art_source/sprite_generation_kit/・同.zip | 上記自作原画/採用済み待機/しゃがみのコピーと制作条件/パレット/プロンプト/検査用SHA-256 | 他環境向け自己完結した再制作キット。元ファイルは保持 |

画像生成は内蔵image_genを使用し、APIキー・有料アセット・出典不明の第三者素材は追加していない。参考のSNS比較画像や騎士のキャラを転載していない。容姿/衣装は採用済み自作資料を維持し、本作のしゃがみ/呼吸は独自表現。原画と旧素材を保存、立ち由来の倍率でしゃがみの頭/靴を保持。最終24Fは各1枚の画素から作る。[詳細な再制作指示書](SPRITE_GENERATION_GUIDE.md)、[実装用出力](../art_source/art3_crouch/README.md)。

## ART3D：ジャンプ・ガード・回避・ダッシュ（2026-10-13）

内蔵image_genで採用済みART3待機原画と224×160ゲーム待機を参照し、全3人の動作原画15枚を採用。不採用4枚を含む19枚をart_source/art3_actions/generated/へ保持。全文プロンプト/参照の役割はgeneration-log.json、採用版はsource-selection.json。容姿・衣装は既存自作資料、動作は本作独自表現。第三者キャラ/装備/素材は追加していない。

AtlasPoseSplitter/ActionAtlasBuilder/ArtThreeActionsで実マスク分離、立ち由来の共通倍率、既存32色で51キーを正規化。頭部帯幅±2px以内の検査と目視を行い、6クリップ/人93FをArt/Spritesへ接続。ready/に固定セルPNG/シート/JSON/GIF。新予備状態はlandと同じ浅い姿勢を使い、全106クリップ584Fを保持。[素材比較](art_wip/art3d_previews.md)、[再制作条件](SPRITE_GENERATION_GUIDE.md)§10、持ち出しキットへ保存。原画の端切れ/頭部幅超過を再生成し、検査値の緩和や部分拡縮は行っていない。

## フェーズ5B：既存キーフレームからの中割り・idle整合（履歴、6Aで混合を廃止）

| 保存先 | 元資料・作成方法 | 用途 |
| --- | --- | --- |
| `art_source/phase5b_keys/{toma,ren,chizuru}/` | ART2までの加工済みPNG／JSONの保管コピー。raw原画は変更しない | 変更前のキーポーズ比較・復元資料 |
| `tools/SpriteForge/AnimationSmoothing.cs` | 既存原画を従来と同じ縮尺・32色へ加工した後、2キーの局所不透明ランを対応付けて中割り。足元・微小な頭／胴ずれを整合。大きな形状差は近いキーポーズを保持 | 各14クリップを6〜8Fへ増量。idleは1枚の基準から頭・腕など上部を±1px、胴と足は固定 |
| `AniBato/Assets/_Project/Art/Sprites/{toma,ren,chizuru}/` | 上記自作C#処理、二値アルファ・共通パレット・PPU50・224×160セルを維持 | 透真125F／蓮118F／千鶴125F（全368F） |
| `docs/art_wip/{toma,ren,chizuru}_<clip>_preview.gif`・各contact PNG | 同じ加工済みセルから自作GIFエンコーダ・ニアレスト一覧生成 | 更新42クリップの確認用。ゲーム内でGIFを使用しない |

今回の新規画像生成API利用・外部送信・第三者素材導入はない。衣装・キャラ設定は既存資料のまま。呼吸・格闘の中割りは本作独自の演出。局所形状の対応が難しい箇所にはキーポーズ切替が残り、原画自体の細部差まで解消したとは扱わない。

## ART3E：歩き・走り・通常3段・投げカバン（2026-10-13）

採用済み本作キャラを参照して内蔵image_genで90原画を生成し、採用19件を記録。歩き/走り/通常15クリップ102Fとカバン4回転4Fへ接続、全107クリップ588F。容姿/衣装は既存本作資料、攻撃/足運び/反動は本作独自表現。透真右/左パンチ/右キック、蓮跳び込み頭突き/頭支持蹴り/腕押し両足蹴り、千鶴同じ右手二撃/投げ。原画/全文条件/参照の役割/採否/寸法/SHAはart_source/art3_walk_attacks/、SpriteForgeの立ち由来共通倍率/224×160/PPU50/32色と頭/靴検査を維持。ユーザー動作参考はローカル閲覧だけで公開Assets/持ち出しZIPに含めない。ポーズ指示に使う骨格ガイドは自作の図。
# 2026-10-14 紙人形風2D・千鶴の待機試作

## 2026-10-15 全9人待機の区切りとフライパン採用

ユーザー指定で残る8人（透真・湊・シロ・蓮・結衣・黒獅子王・暁・龍太）の待機全身を内蔵image_genで1人ずつ生成。本人の容姿画像と千鶴の画風を別の役割で参照。原画／全文条件はart_source/paper_roster_idle/generated/、参照の出典はreference-sources.json、共通透明枠1304×1450の確認版とSHA／画素一致はmanifest.json。元作品のフォルダは読み取りだけ。参照6枚はdocs/art_reference/roster_idle/へ保存、透真／蓮は既存資料を使用。湊の見えない下半身は本作用の仮補完で正史として確定しない。新規8枚はゲーム未接続、キャラ別の表示倍率と手・部位の審査が残る。千鶴と並べた確認一覧はイラスト確認/全9人_待機/。

千鶴は既存フライパン比較から横1段／横2段／上下の最大3段と方向別の2原画をゲーム採用へ進める。生成画素の拡縮／描き換えはしない。本人の左手フライパン、背面攻撃は全身Spriteの向きを攻撃側へ返す。カバン原画13件と以前の第三段の定義はvariants/へ保護して残す。実Unity結果と採用の確定は今回の最終報告を参照。

## 2026-10-15 フライパン比較とオリジナル音

フライパンは内蔵image_genで本作の千鶴の採用済み絵を参照し制作。art_source/paper_cutout/variants/fryingpan/に15生成原画と全文条件、うち横6／上下6の12候補をcomparison-manifestで指定。未採用。技術的な透明余白追加だけで1304×1450／足元1328へ揃え、原画の全画素一致を検査。画像全体や手を反転加工していない。頭／靴等の部位寸法と左右の最終審査は別途必要。カバン版はvariants/bag-backup.jsonの13原画を明示削除まで保護。

ArcadeAudio.csはオリジナルの手続き合成。180BPMのメニュー＋3ステージBGM、打撃／防御／開始／結果など19SE。録音、既存曲の旋律、第三者のミーム音声は不使用。用語の解釈の調査先はPAPER_THEATRE_DIRECTIONにリンクし、音素材として取り込んでいない。TestResults/arcade_audio/のWAVは同じC#生成結果の形式保存。第三者音源・新しい外部サービス契約はない。

art_source/paper_cutout/generated/chizuru_paper_idle_v1.pngは内蔵image_genによる本作の自作試作。本人の採用済み茶色ボブ/オレンジセーター/紺スカート/白スニーカーを、紙の切り抜きの輪郭とアニメ塗りへ描き直した。全文プロンプト/参照の役割/出力寸法/SHAを同フォルダに保存。ユーザー添付の第三者ゲーム画面は見せ方/角度の理解にのみ利用し、人物/衣装/UI/背景はコピーせず、画像そのものをリポジトリ/ゲーム/キットへ収録しない。

同一バイトの原画をAniBato/Assets/_Project/Art/Paper/idle_paper_chizuru.pngへ保存し、千鶴の待機/歩行だけ接続。足元を支点にUnityで段階的な傾き/上下を付け、原画/旧ピクセル素材を保持。docs/screenshots/paper_chizuru_unity/の18PNGはUnity実カメラの描画、左右GIFはそのPNGの形式変換で第三者素材なし。実モニター撮影ではない。BuildAll/EditMode441/441/PlayMode36/36/Windowsビルド成功。


## 2026-10-15 工作舞台と9人画面

PaperStageScenery／PaperPlatformViewで作る自作の平面シェイプ。街／管理AI画面／6×6盤／駒／塔／文化祭屋台／黒板／調理小道具と厚紙の縁は、本作独自の舞台演出で正史の追加ではない。タイトル・選択はart_source/paper_roster_idleの9待機をコピーし、Unityの文字・台紙・棒・テープで構成。第三者の画像やUI素材は転載していない。各キャラの参照出典と仮補完は同フォルダの出典表を参照。

## 2026-10-15 横振り／おにぎりの修正

千鶴の全通常はfryingpan/prepared/horizontal_1〜3の予備／有効6原画を再利用する。空中上下・斜めは全身の固定傾きで表示し、中割り／関節分割は追加しない。縦振り6原画とカバン13原画は予備として保持。通常特殊の手持ちおにぎりは内蔵image_genでv3を生成し、人物全体の再サンプルをせず透明枠へ。v2は人物寸法が変わったため不採用だがv3の参照資料として制作元だけに保管。全文条件は同名.prompt.json。飛翔は既存の紙版effect_rice_ball_v1を継続し、rice-size.jsonで手持ちと表示幅を揃える。絵の最終審査とゲーム接続を分ける。

## 2026-10-15 基本5ポーズ（順次制作）

art_source/paper_roster_core/の新規ポーズは内蔵image_genによる本作の自作素材。採用待機を容姿・部位寸法の直接参照とし、千鶴の該当姿勢と元作品資料は制作前に確認する。出典と未確認設定の仮補完はpaper_roster_idleの出典表を継承する。第三者の絵やゲームUIを素材として取り込まない。各人のprompts/に実行全文と参照の役割、manifestに原画SHA・採用版・接続／検証状態を残す。plannedは未生成であり、全員完成を意味しない。

透真はguard v3／crouch v2／counter v4／win v1／lose v1を選定。人物を拡縮せず1304×1450へ透明梱包、共通PPU532.5893。外周のぼやけへの明示指示に従いアルファしきい値128で縁を整理し、RGBと完全不透明の人物内部は変えない。処理前のgeneratedと処理後のedge_cleanを保持し、その両方のSHAを記録。元RGBAとの完全一致とは区別する。部位寸法の概算と左右の手の審査はpart-review.json。全身／手足の新しい生成表現は本作独自演出で、原作の正史設定の追加ではない。

## 2026-10-16 全員の基本5ポーズ原画

新8人の選定40種類は本人の採用待機を直接参照して内蔵image_genで生成。原画55候補から部位寸法・手・姿勢・切断を審査し40種類を選定。千鶴の既存5種類を合わせ全45種類とする。出典・全文条件・SHA・選定版・微差はart_source/paper_roster_core/の各manifestとpart-review、画素保存の照合はpixel-audit.json。修正候補は版を分け、採用中を上書きしない。RGBと不透明内部を保持した外周アルファ処理と透明余白のみで共通1304×1450／PPU532.5893／接地1328へ梱包。原画のRGBA全一致や全ポーズの部位輪郭完全一致は主張しない。第三者の画像・背景・UIは新たに転載していない。勝敗の立ち姿と受け流し構えは本作の独自演出。

## 透真の銃通常・指令と飛翔物（2026-10-16、R4）

内蔵image_genで自作した16全身原画（通常3段・上下・空中N・特殊・超必殺の予備と当てる）と銃弾／ドローン／ロケット3原画。参照は本作の透真の採用待機v2、低姿勢は本人の採用しゃがみv2、各予備→当てるは同じ予備を局所編集。第三者アセットの取り込みなし。art_source/paper_roster_action/toma/のmanifest・effects-manifest・promptsに全文条件／参照役割／SHA／選定版を保持する。部位目視はpart-review、枠内RGBA一致と枠外alpha1ノイズ56画素の例外はpixel-audit。原画自体は不変、人物の再サンプル0。正式美術承認とゲーム使用は別。通常／特殊／超必殺の性能はR3から変更しない。

## 2026-10-16 R5の生成舞台と集合絵

内蔵image_genで自作した舞台21レイヤーと集合4原画。art_source/paper_stages/およびtitle_paper/のmanifestに原寸・SHA・全文条件と参照の役割を保存。生成画像を無加工で保持し、ゲームではSpriteのUVとアルファしきい値shaderを使う。9人の外見は既存の採用待機を継承。AIの電子窓や工作セットは本作独自表現であり原作設定の追加ではない。原作の容姿が未確認のサポート人物は勝手に追加しない。詳細はGENERATED_STAGE_ART.md。

## 2026-10-16 WebGLの日本語フォントと画像梱包

Noto Sans JP Regularは[Noto CJK公式](https://github.com/notofonts/noto-cjk/blob/main/Sans/SubsetOTF/JP/NotoSansJP-Regular.otf)のSIL Open Font License 1.1の配布物を無改変で同梱。SHA-256はdff723ba59d57d136764a04b9b2d03205544f7cd785a711442d6d2d085ac5073。原本・ライセンス・条件はart_source/fonts/、ゲームはResources/NotoSansJP-Regular.otf。Windows／WebGL出力へThirdPartyNotices.txtをコピーする。OSのフォントは転載していない。

Web用に自作画像274件を右／上の透明余白で4の倍数へ梱包する。原画ファイルと元RGBA画素を保持し、再サンプルなし。ビルド専用キャッシュのDXT5は非可逆GPU圧縮であり、実表示の画素全一致とは区別する。元のSprite領域・PPU・支点と人物寸法を維持する。予備カバンと旧原画は削除せずWebの未使用参照だけを外す。詳細はWEBGL_CURRENT.md。

### 通常4原画の改訂（2026-10-16）

蓮の選定4枚は内蔵image_genで本人の紙待機を参照して生成。原画10枚と除外候補・全文条件はart_source/paper_roster_action/ren/。通常4枚だけゲーム使用、上／下／空中Nは未接続。透明余白梱包1304×1450、PPU532.5893、可視支点1328、再サンプル0。SHAと枠内RGBA監査はpixel-audit.json、部位審査と最終美術承認の区別はpart-review.json。透真と千鶴は通常1段の2枚を2段へ共有、旧独立2段とカバン予備を保持。実Unity paper_fourkeys_3全4合格、実描画48PNG。

### 投げ終わりと二丁拳銃の改訂（2026-10-16）

内蔵image_genによる千鶴の空手の投げ終わり1枚と、透真の二丁拳銃の予備／当てる2枚を接続。透真の当てるは上側左手の掌側v3を使用し、甲側のv2は不採用。原画と全文条件・参照役割・SHA・現在版はart_source/paper_cutout/chizuru-release-review.jsonとpaper_roster_action/toma/dual_gun/。人物画素は再拡縮せず1304×1450へ透明梱包、PPU532.5893。枠内RGBAの一致と、枠外のalpha1/255だけのノイズ除外をpixel-auditへ記録（原画は保持）。美術最終承認は別。採用はpaper_release_5全4合格、実描画18PNGとブラウザ5PNG。

蓮の特殊／超必殺4候補も追加保存し、14枠の原画候補が揃った。ゲームへ接続したのは前工程の通常4枚だけで、残10枚は部位審査・梱包・接続待ち。本人の紙待機を参照し、飛車は同じ赤縁・青緑帯の独自ゲーム表現。正史の追加ではない。

### 蓮の残10攻撃原画の接続（2026-10-17）

本人の採用紙待機を参照し内蔵image_genで生成した上／下／空中N／特殊／超必殺を追加接続。超必殺構えは部位修正v2。通常4枚を含む14枚、1304×1450、PPU532.5893、透明梱包・再サンプル0、枠内RGBA一致。枠外alpha1ノイズ16画素のみ除外し元画像を保持。原画・全文条件・SHAと部位審査はart_source/paper_roster_action/ren/。飛車は本作独自の赤縁／青緑帯で、正史の追加ではない。rework_r4_ren_2で全4合格、48実描画。

### 千鶴不足原画と集合絵の手の修正（2026-10-17）

内蔵image_genで千鶴の下／空中N／カレー決めと通常3段の左手を作成。採用はart_source/paper_roster_action/chizuru/のcurrent-adoptionへ記録し、未検証時は採用済みと扱わない。空中・下の予備候補は最新指定でゲーム不採用、原画は保存。人物の再サンプル0、1304×1450／PPU532.5893、透明梱包。カバン版13原画は保護。

集合絵は既存の本作9人絵を編集し、手と向きのみ改訂。ユーザー添付（2026-10-17）のBattle à la Carteカード背面を参照。コピーはart_source/title_paper/references/battle_a_la_carte_card_back_user_20261017.png、SHA256 6682f4c77064fee8d6329be912e4e267d7bdf35fd754771dac3a09f3ad87a1d5。参照元の他プロジェクトへ書き込みなし。全文条件・候補v2〜v5と採用selected.jsonを保存。v2は6指と保持の誤りで不採用、v3／v4は保持が残り不採用。v5で透真5指と、暁の左の掌側のカードつまみ持ちへ修正。細かい文字と飾りの完全再現、美術の最終承認は別。人物の原作設定を追加しない。

### 湊の攻撃12枠・固有効果3種（2026-10-17）

内蔵image_genを用い、本人の採用待機と採用しゃがみを容姿・部位の実寸の基準にした。原画、全文条件、参照役割、現在版とSHAはart_source/paper_roster_action/minato/。通常1・2段の右生身パンチ2枚＋3段の左義手2枚、上2枠、下1枠、空中N1枠、意識接続／瞬間移動2枠、意識解放2枠の12表示枠。上の予備は通常3予備、特殊の決めは通常1決めを原寸再利用し、独立した人物原画は10枚。

人物は1189×1323の生成原画を整数の平行移動と透明余白だけで1304×1450／PPU532.5893／可視足元1328へ梱包。12枠の枠内RGBA一致、再サンプル0、枠外ノイズ除外0。頭髪と靴の投影の差と美術最終承認はpart-review.jsonで区別。腕の所属混同、端切れ、低姿勢の部位拡大があった旧候補は不採用として保存。

接続の紙片、意識状態の切り絵の霧、ブラックホール状の持続場も内蔵image_genによる自作原画。effects-manifest.jsonが現在版と元PNGのSHAを指定。元RGBAを保ち、Unityの表示倍率のみで配置する。透過プレビューの透明RGBが光彩に見えた点は、別の背景へ合成した検査で区別した。能力・意識状態・渦の細部は本作独自のクロスオーバー演出で、原作正史とは断定しない。他作品のアセットや第三者の素材は追加していない。

### シロの攻撃12枠・EMPとALMA効果3種（2026-10-17）

内蔵image_genで本人の採用待機と採用しゃがみv2を容姿／部位寸法の基準に生成。原画・全文条件・参照役割・SHA・選定版と明示再利用はart_source/paper_roster_action/shiro/。通常1・2段の噛みつき共通2枚＋3段の大きい頭突き2枚、上2枠、下／空中N各1枠、EMP2枠、ALMA2枠の12表示枠。上予備は通常3予備、超必殺決めはEMP決めの遠吠えを再利用し、ユニークな人物原画は10枚。

1189×1323の原画画素は再サンプルせず整数移動と透明余白だけで1304×1450／PPU532.5893／可視足元1328へ梱包。RGBA画素監査と頭・耳・足先・尾・四肢4本の部位審査は別記録。端切れ・余分な小さい足・上姿勢の部位拡大のあった旧候補は不採用。投影の微差と下決めの尾の左余白10pxはpart-reviewへ明記し、美術最終承認とは分ける。

EMPの切り絵の輪、ALMA接続の空枠、六角形の核と左右へ広がる攻撃の3素材も内蔵image_genの自作原画。1536×1024、1536×1024、1672×941を再描画・画像変形せず保持し、ゲームのSprite表示倍率だけで配置。二進数は生成画像に焼かずゲームで正確な0・1の文字を重ねる。ALMAの抽象的な核や波は本作独自のクロスオーバー演出で、未確認の原作AIの外見を正史として確定しない。外部の第三者アセットは追加していない。
