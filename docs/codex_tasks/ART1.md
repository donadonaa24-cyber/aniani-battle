# ART1 指示書：ドット絵スプライト制作パイプライン＋千鶴（パイロット）（Claude → Codex）

## 目的
透真・蓮・千鶴を **実際にゲーム画面で動くドット絵スプライト** にする。まず千鶴で「生成 → 加工 → Unity取り込み → 戦闘状態に連動した再生」の全工程を完成させ、Claudeのレビュー後に透真・蓮へ展開する（ART2）。

## 参考資料（`docs/art_reference/`）
- 千鶴：`characters/chizuru_casual_sheet.webp`、`characters/chizuru_casual_fullbody.webp`（**正装は私服**：茶色のボブ、茶色の目、袖を少しまくったオレンジの長袖セーター、紺のフレアスカート、白いスニーカー）。`characters/chizuru_chibi_rpg_sheet.webp` は色の参考のみ（等身・向きは使わない）。
  - **シェフ服（コック帽・白いコックコート・緑のスカーフ）は将来の「覚醒／シェフモード」用に温存**。今回のスプライト・カットインには使わない。既存のカットイン画像 `chizuru-battle-mode-icon.png`（シェフ服）は私服の `chizuru_casual_fullbody.webp` ベースへ差し替える。シェフ服画像は `Art/Characters/Awakening/` 等へ移し「将来用」と `docs/ASSET_SOURCES.md` に記録。
- 透真：`characters/toma_sheet.webp`、`characters/toma_keyvisual.webp`（ユーザー提供の正式設定画。ART2で使用）
- 蓮：`characters/ren_dog_rook.png`（ART2で使用）
- 画風参考：`_style_local/pixel_style_reference.webp` は**他者のファンアート**。画風（高密度・濃い輪郭・セル陰影）の参考として見るのは可、**キャラ・ポーズ・構図を写さない**。このファイルはGit管理外。Assetsへコピー禁止。

## Claudeによる試作の結果（採用）
`docs/art_wip/chizuru_idle_test.png`（2172×724、4フレーム、マゼンタ背景）。キャラの一貫性・足元揃え・画風とも良好。この指示（下記プロンプト規約）で量産する。

## 1. 生成（画像生成ツール）
- 生画像の保存先：`art_source/raw/chizuru/<anim>.png`（**リポジトリ直下。Unityの Assets 外**。Gitには含めてよいがサイズに注意）。
- 1アニメ＝1枚の横一列ストリップ、**最大6フレーム**。毎回、千鶴の参考画像2枚＋採用済み `idle` を参照画像として渡し、同一キャラを保つ。
- プロンプト規約（全ストリップ共通）：
  - 2D横スクロール格闘ゲーム用、**画面右向き**の真横〜斜め横、約5〜6頭身（ちびにしない）
  - 90年代アーケード格闘ゲーム風の高密度ドット絵、濃い輪郭線、3〜4段階のセル陰影、限定パレット、アンチエイリアスなし
  - 全フレームで同じ大きさ・同じ足元の高さ（地上アニメ）、フレーム間は等間隔で十分な余白
  - 背景は単色マゼンタ #FF00FF、文字・枠・影・エフェクト文字なし（エフェクトは別スプライト）
- 千鶴のアニメ一覧（フレーム数は目安）：
  idle 4 / walk 6 / dash 4 / jump 3 / fall 2 / land 2 / attack1 3（ジャブ）/ attack2 3（フック）/ attack3 4（フィニッシュの蹴り）/ air_attack 3 /
  special_neutral 4（爆弾おにぎり投げ）/ special_side 4（フライパン突撃：フライパンを構えて突進）/ special_up 4（おたま旋風：おたまを持って回転上昇）/ special_down 4（ひとくち休憩：おにぎりを食べる）/
  ultimate 6（満腹カレー！：カレー鍋をかかげる→ふるまう）/ guard 2 / dodge 3 / counter 3（構え→成功の払い）/ hitstun 2 / knockback 2 / ko 3 / win 4 / lose 2
- エフェクト・小物（別スプライト、マゼンタ背景）：爆弾おにぎり（回転4フレーム）、もぐもぐ（麻痺中アイコン）、カレー鍋の湯気／波。
- 生成に失敗した・別人に見えるフレームは作り直す。**品質が足りないものを採用しない**。

## 2. 加工ツール（自分で実行・反復できること）
`tools/SpriteForge/`（.NET コンソール or PowerShell＋System.Drawing。外部パッケージのダウンロード禁止）：
1. マゼンタのクロマキー除去（許容幅あり）＋エッジのマゼンタかぶり除去（despill）
2. フレーム分割：不透明ピクセルの列の切れ目で自動検出（フレーム数はマニフェストで検証）
3. 足元合わせ：各フレームの最下端の不透明ピクセルを基準線に揃え、水平は「アニメ内で固定のアンカー」（idle の体の中心）に揃える。空中アニメは体の中心で揃える。
4. **本物のドット化**：キャラの身長が目標ピクセル数（千鶴 ≈ 112px）になるよう、最頻色ブロック方式などで縮小（バイリニア禁止）。全アニメで同じ縮尺。
5. 減色：キャラ単位の共通パレット（≤ 32色）へ量子化。アルファは0/255の2値。
6. 1px の濃い輪郭が欠けた箇所を補う（任意）。
7. 出力：`AniBato/Assets/_Project/Art/Sprites/chizuru/<anim>.png`（固定セルのシート、例 160×160、pivot 下中央）＋ `<anim>.json`（フレーム数、セルサイズ、pivot、推奨fps）。
8. 確認用：`docs/art_wip/chizuru_contact.png`（全アニメを並べた一覧、2倍拡大、ニアレスト）を出力。
- マニフェスト `art_source/chizuru.manifest.json` に、アニメ名・フレーム数・fps・ループ・アンカー種別・目標身長を記述し、ツールはそれを読む。透真・蓮も同じツールで処理できる汎用設計にする。

## 3. Unity取り込みと再生
- Editorスクリプト（`ProjectBuilder.BuildAll` から呼べる）：スプライトのインポート設定（Point、圧縮なし、Mipなし、PPUはキャラ間で統一）、JSONに従ったスライス、`CharacterAnimationSet`（ScriptableObject：アニメ名→Sprite配列・fps・ループ）を生成し `CharacterData` に紐づける。
- `FighterView`：**シミュレーション状態から決定的にフレームを選ぶ**（Animator不要）。
  - 状態→アニメの対応表をデータ化（Idle/Move/Dash/Jump(上昇)/Fall/Attack(段数・空中)/Special(4種)/Ultimate/Guard/Dodge/Counter/Hitstun/Knockback/KO/勝利/敗北、着地）。
  - 攻撃系は **攻撃の経過フレーム（発生・持続・硬直の進行度）に同期**して絵を選ぶ（持続中に最も伸びた絵が出るように）。
  - 向きは flipX。ヒットストップ中は絵を止める。被弾点滅など既存の演出は維持。
  - スプライトが無いキャラ（透真・蓮）は従来の仮図形にフォールバック。
- 見た目と当たり判定のずれ確認：デバッグ表示（F9 などで Hurtbox／Hitbox の枠を重ねる）を追加。必要なら千鶴の Hurtbox サイズをスプライトに合わせて微調整し `BALANCE_CONFIG.md` に記録（ゲームバランスを大きく変えないこと）。
- 頭上カウンターの位置をスプライトの頭の上に合わせる。
- カットイン：千鶴は私服イラストに差し替え。

## 4. 併せて直すこと：PlayMode テスト（Claude調査結果）
Unity実機で PlayMode 8件すべてが SetUp の `Assert.That(keyboard.enabled, ...)`（BattleSceneTests.cs:45）で失敗。原因は `InputTestFixture` が差し替える **テスト用ランタイムがフォーカス無し扱い**で、`Application.runInBackground` ではなく **`runtime.runInBackground`（InputTestRuntime のプロパティ）** と **`runtime.focusState`** を見ているため。
- `base.Setup()` の後で `runtime.runInBackground = true;` と `runtime.focusState = InputRuntime/FocusFlags の ApplicationFocus`（パッケージ `Library/PackageCache/com.unity.inputsystem@*/Tests/TestFixture/InputTestRuntime.cs` の `focusState` / `ScheduleFocusChangedEvent(true)` を参照）を設定し、必要なら `InputSystem.Update()` を1回呼んでからデバイスを追加する。
- `Press(keyboard.dKey)` 等の InputTestFixture のAPIを使うこと。アサーションを弱めない。

## テスト
- EditMode：状態→アニメ選択（攻撃進行度に同期、ヒットストップで停止、向き反転）、マニフェストとシートのフレーム数一致、全アニメのSpriteが存在。
- SpriteForge：自前の小さな検証（マゼンタ0ピクセル残り、2値アルファ、身長が目標±2px、全フレーム足元が基準線）。

## 報告
最終メッセージに：生成した枚数／作り直した枚数、`docs/art_wip/chizuru_contact.png` のパス、加工パラメータ、Claudeに Unity で確認してほしい点。git commit はしない。
