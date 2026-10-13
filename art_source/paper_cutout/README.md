# 千鶴の紙人形風2D・待機試作

**Unity接続済み：待機・歩行だけ新しい一枚絵（2026-10-14）。** 実装指示書はdocs/codex_tasks/PAPER_CHIZURU_UNITY.md。PaperCharacterMotionの設定で千鶴のIdle/Moveだけ使い、立ち高さ2.24u、歩行±5度/8fps/上下0.025u、待機±0.75度/4fps。足元ピボットと左右の足幅で沈みを補正し、顔/服を変形しない。EnabledをOFFにすると保存済みの旧ピクセル表示へ戻る。

実Unity：BuildAll成功、EditMode441/441、PlayMode36/36、Windowsビルド成功（失敗/スキップ0）。証跡TestResults/paper_chizuru_1_*、最新Build/paper_chizuru_1/AniBato.exe。Unity実カメラの18PNGと左右各8コマのGIFはdocs/screenshots/paper_chizuru_unity/。PNGは実レンダリング、GIFはPNGの再描画なしの形式変換。実機のOSキー送信は行っていない。以下の未接続は初回試作時の履歴。

2026-10-14、ユーザー指定でピクセルからアニメ塗りの紙の切り抜き風へ試作。千鶴1体だけ、茶色ボブ/オレンジのセーター/紺スカート/白スニーカー、右向きの斜め横、両手両足が見える全身。内蔵image_genで制作した自作素材。

- 原画：generated/chizuru_paper_idle_v1.png
- 全文日本語プロンプトと参照の役割：generated/chizuru_paper_idle_v1.prompt.json
- 実寸/alpha形式/SHA/接続状況：chizuru_idle_prototype.json
- 作業方針/一枚絵の歩行案：docs/codex_tasks/PAPER_CUTOUT_PROTOTYPE.md

原画は高解像度の試作であり、旧224×160のゲーム素材と寸法を取り違えない。既存原画/ピクセル素材/未コミット差分は保持、今回はゲームへ未接続、Unity検証未実行。ユーザー提供の紙芝居風ゲーム画面は画風/角度の理解だけに用い、元のキャラ/UI/背景や画像そのものは公開素材へ入れない。

歩行は一枚の画像を足元支点に少し左右へ傾け、段階的な角度で切り替える案。人物の顔/服/部位が同じ画素のまま動くため、新規の左右脚8枚を描く必要がない。ゲームの座標/当たり判定は動作の絵と分離して維持できる。採用後に共通セル/PPU/角度/更新頻度を固定し、実Unityで検証する。
