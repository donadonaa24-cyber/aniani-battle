# 千鶴の紙人形風2D・待機試作

## 最新：全動作をゲームへ先行接続（2026-10-14）

ユーザー指定で現在の30姿勢／全35状態と4効果を接続済み。runtime-manifest.jsonがゲーム用の対応表、review-catalogは現在の35確認画像。確認コピーは全て01_ゲームで採用済みへ。手や部位サイズの修正残はartReview／artNoteに保持し、ゲーム使用と絵の最終承認を混同しない。

ゲーム用人物は全て1304×1450の透明枠、共通PPU532.5893／立ち2.24u。元の待機1246×1263を基準に、余白を追加するだけで人物画素の拡縮・切断なし。元PNGの寸法差はruntime-manifestに記録。原画SHA、靴の足元・頭の座標、空中支点、状態の別名を同表へ。再現はbuild_paper_runtime_manifest.cjs→Unity BuildAll。絵として未修正の画像もユーザー指定で先行使用する。

今回実Unity：BuildAll成功・Edit447/447・Play38/38・Windowsビルド成功、失敗／スキップ0。実描画19PNGはdocs/screenshots/paper_chizuru_all_unity/、詳細paper_chizuru_all_2_final。採用フォルダの更新は実検証後にadopt_paper_review.cjs <タグ>→remove_old_paper_images→build_paper_review_folder→build_paper_review_contact。旧画像は再生成しない。

## 以下はゲーム接続前の履歴

## 現在：承認済み手元を空中前方v8へ反映

ユーザー承認済みworking/air_forward_left_fist.pngを内蔵image_genで前方v8へ反映。掌側4指と下の親指が見える全身候補で、ゲーム未採用。実寸1243×1265、角alpha0／端切れ0、部位倍率と共通セルは未審査。斜め下v3は甲側の左拳の再修正が必要。2ポーズを混同しない。

旧PNGはユーザー指定で削除済み（原画41＋管理済みコピー41、分類変更の旧コピーを含む累計）。generated/は現参照34PNG、working/に承認済み手元参考1PNG。条件JSONとdeleted-old-images.jsonは保持。確認コピー35枚は採用2／候補26／要修正7、PNG一覧1枚、旧版フォルダは再生成しない。keepPreviousImages=false、整理はinspect_paper_sources→remove_old_paper_images→inspect_paper_sources→build_paper_review_folder→build_paper_review_contactの順。採用済み待機と他のコードを削除しない。今回Unity追加実行0回、実機スクショ0件、未commit／未push。次は全身確認と残る手／部位審査→接続と実Unity全4工程。

## 以下は削除方針変更前の履歴

最新の空中攻撃修正：原画73PNG／全文73件。前方v6は下側の親指が見える掌側の左拳、斜め下v3は背中構図で甲が見える左拳へ。前方v4／v5と斜め下v1／v2はユーザー指摘で不採用。review-catalogと確認フォルダを更新し、採用済み2／候補27／要修正6／旧版39、原画同一コピー74枚＋縮小一覧1枚を保存。新しい2枚は部位倍率／共通セル／Unity接続の審査前で未採用。以下は前回の整理記録。

最新（2026-10-14）：69PNG／69全文条件を保持。日本語名と状態の対応表はreview-catalog.json。ユーザー向け確認用コピーはプロジェクト直下のイラスト確認/千鶴/に、採用済み2・未採用候補27・要修正6・旧版35を分けて保存。現在の画像一覧.pngで35画像の名前と状態を確認できる。70コピーは原画のSHAと一致し、コピーだけをGit管理外にしている。再作成はtools/verify/build_paper_review_folder.cjs、縮小一覧はbuild_paper_review_contact.cjs。BackAttack v8は紙縁、AirForward v4は右腕カバンの修正候補。部位寸法／共通セル／Unity接続の審査は未完了。

最新（2026-10-14）：67PNG／67件の全文条件を保存。BackAttack v7は自由な左手の親指を上側、甲が見える形へ修正し、カバンは右手。v6の下向き親指はユーザー指摘で不採用。v7は1246×1263／両隅alpha0／alpha32端切れ0、全身と指を目視したが部位寸法の正式審査とUnity接続は未完了。Attack3 v6は腕の所属の再修正が必要。最新の採否はhand-review.json、実寸／SHAはsource-index.json。停止時43件の記録は以下に保持している。

2026-10-14再開後の最新指定：カバンは本人の右手、フライパンとおたまは左手に統一。画面の左右と混同せず、肩／肘／手首／指のつながりを全武器姿勢で確認する。最新指示書はdocs/codex_tasks/PAPER_CHIZURU_ALL_ACTIONS.md。

**2026-10-14：電源OFFのためユーザー指定で停止。** 全アクションの原画候補は43PNG（待機1＋走り3＋28姿勢＋4効果＋7修正版）、全文プロンプト43件を保存済み。部位／キャンバスの比較と正式採用、runtime-manifestによるゲーム接続は未完。`crouch_attack_v2`／`special_side_v2`は1129×1393で基準1246×1263と異なり未採用。原画をセルに合わせて拡縮しない。実寸／SHA／透明はsource-index.json、生成条件はall-actions-plan.jsonと各prompt.json。

次の「再開して」は[停止時引き継ぎ](../../docs/codex_logs/paper_chizuru_all_checkpoint.md)のサイズ確認から続ける。移動の最新指定は待機だけ揺れ／歩き15度・走り45度を進行方向へ固定。下記の441/36と歩行±5度は前の完了版60501c4の証跡で、新しい全アクションのUnity検証ではない。新しい差分は未コミット／未push。

**Unity接続済み：待機・歩行だけ新しい一枚絵（2026-10-14）。** 実装指示書はdocs/codex_tasks/PAPER_CHIZURU_UNITY.md。PaperCharacterMotionの設定で千鶴のIdle/Moveだけ使い、立ち高さ2.24u、歩行±5度/8fps/上下0.025u、待機±0.75度/4fps。足元ピボットと左右の足幅で沈みを補正し、顔/服を変形しない。EnabledをOFFにすると保存済みの旧ピクセル表示へ戻る。

実Unity：BuildAll成功、EditMode441/441、PlayMode36/36、Windowsビルド成功（失敗/スキップ0）。証跡TestResults/paper_chizuru_1_*、最新Build/paper_chizuru_1/AniBato.exe。Unity実カメラの18PNGと左右各8コマのGIFはdocs/screenshots/paper_chizuru_unity/。PNGは実レンダリング、GIFはPNGの再描画なしの形式変換。実機のOSキー送信は行っていない。以下の未接続は初回試作時の履歴。

2026-10-14、ユーザー指定でピクセルからアニメ塗りの紙の切り抜き風へ試作。千鶴1体だけ、茶色ボブ/オレンジのセーター/紺スカート/白スニーカー、右向きの斜め横、両手両足が見える全身。内蔵image_genで制作した自作素材。

- 原画：generated/chizuru_paper_idle_v1.png
- 全文日本語プロンプトと参照の役割：generated/chizuru_paper_idle_v1.prompt.json
- 実寸/alpha形式/SHA/接続状況：chizuru_idle_prototype.json
- 作業方針/一枚絵の歩行案：docs/codex_tasks/PAPER_CUTOUT_PROTOTYPE.md

原画は高解像度の試作であり、旧224×160のゲーム素材と寸法を取り違えない。既存原画/ピクセル素材/未コミット差分は保持、今回はゲームへ未接続、Unity検証未実行。ユーザー提供の紙芝居風ゲーム画面は画風/角度の理解だけに用い、元のキャラ/UI/背景や画像そのものは公開素材へ入れない。

歩行は一枚の画像を足元支点に少し左右へ傾け、段階的な角度で切り替える案。人物の顔/服/部位が同じ画素のまま動くため、新規の左右脚8枚を描く必要がない。ゲームの座標/当たり判定は動作の絵と分離して維持できる。採用後に共通セル/PPU/角度/更新頻度を固定し、実Unityで検証する。
