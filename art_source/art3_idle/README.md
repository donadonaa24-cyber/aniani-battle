# ART3：採用済み待機原画（2026-10-12）

内蔵image_genで透真・蓮・千鶴の待機ポーズを各1枚生成。全身・右向き・静止ポーズ・透過背景。追加依頼ART3Bで3枚とも呼吸アニメの基準絵として採用し、ゲーム用idleへ接続しました。旧rawは他モーションの縮尺・共通パレット基準として保持しています。

| キャラ | PNG | 生成寸法 | SHA-256 |
|---|---|---|---|
| 透真 | [toma_idle_v1.png](toma_idle_v1.png) | 1246×1262 | 980F597B24184CFDF18F768ECEE7974C13912644E1F74F2A936CD12947B74E38 |
| 蓮 | [ren_idle_v1.png](ren_idle_v1.png) | 1220×1289 | 329A3ECBC30677C16A26F7AC63E99518B26A159C43133CBDD416335880530D72 |
| 千鶴 | [chizuru_idle_v1.png](chizuru_idle_v1.png) | 1246×1263 | 1EC1AB11A9455E1F0BBA8154B763819C54066B1019F4C7658B1D932AE3A6F39F |

正式資料で容姿と衣装を維持。ユーザー添付の比較画像はドットの輪郭・色面・陰影の参考だけに使用し、騎士のデザインやSNS画面は転載していません。第三者の比較画像そのものはここに保存していません。

最終プロンプトと参照の役割は[generation-log.json](generation-log.json)。CLI/APIキーは使用せず、生成PNGをそのままコピーしてアルファを保持しました。隅のalpha=0とPNG復号を確認し、出力を目視確認済み。上記生成寸法は保存原画の寸法です。ゲーム用出力はSpriteForgeで全員224×160px/コマ、32色以内・二値alphaへ加工しています。

[呼吸アニメと実装用データ](breathing/README.md)、[制作規格](../../docs/SPRITE_SPEC.md)、[ART3B指示書](../../docs/codex_tasks/ART3_BREATHING.md)を参照してください。検証と引き継ぎの最新結果は[RESUME](../../docs/RESUME.md)に記録します。
