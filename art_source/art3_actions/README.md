# ART3D：3人のジャンプ・ガード・回避・ダッシュ

実装用はready/<キャラ>/の透明PNG・横シート・JSON。全コマ224×160、PPU50、pivot(0.5,0)、地上の足y159。等倍GIFは素材確認用。詳細は[再制作指示書](../../docs/SPRITE_GENERATION_GUIDE.md)§10。

- generated/：内蔵image_genの原画19枚（採用15、不採用4）。generation-log.jsonへ実際の全文プロンプト/参照の役割を保存。source-selection.jsonで再生成した採用版を指定。他はv1。原画を削除しない。
- normalized/：立ちの物差しから共通倍率で51キーを配置、各グループの*-scale.jsonへ原画寸法/実マスクの矩形/共通倍率/頭部帯幅を記録。人物の欠け/接触・頭の拡大は再生成。頭部帯は全キーで待機基準±2px以内。
- ready/：jump6・fall6・land2・guard6・dodge3・dash8、計31F/人93F。キーを混合しない。landの最後は採用待機の画素と完全一致。
- 地上ジャンプは2tickの浅いかがみから踏切。着地8tickは最初4tickかがみ/次4tick待機。空中2段と踏切後の初速/重力を維持。地上予備は既存landへ接続するため全106クリップ584Fのまま。
- 検査結果：verification.json。既存待機/しゃがみの回帰検査も行う。再現はWindows PowerShellでtools/SpriteForge/BuildArt3Actions.ps1 -RebuildCheck。

蓮dodge v1は画像端のマント切れ、v2は頭周辺幅超過、dash v1はフード幅超過で不採用。千鶴dash v1も頭部幅超過で不採用。採用版は蓮dodge v3/dash v2、千鶴dash v2。検査の許容値は緩めていない。

頭/髪・顔・衣装・靴/関節を等倍と3倍で目視確認。空中や走る足は投影/接地足が変わり、地上待機の下8行の幅と直接同じ値にはならない。全キー共通の倍率と頭部の数値検査に加え、描画上の部位サイズを比較する。ポーズは本作独自の表現。
