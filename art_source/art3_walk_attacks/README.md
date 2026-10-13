# ART3E：歩き・通常攻撃・走りの左右交互

最新（2026-10-13）：歩き/走り/通常15クリップ102Fと投げカバン4Fの素材接続/検査済み。全107クリップ588F。原画90PNG/採用19件、頭部/靴/共通倍率と対象外91PNGを維持。実Unity BuildAll成功/EditMode424/424/PlayMode32/32/Windowsビルド成功、詳しい証跡はdocs/codex_logs/art3e_final.md。

最終通常：透真右パンチ→左パンチ→右キック（通常中だけ端末をしまう）、蓮跳び込み頭突き→頭支持カポエイラ→両腕で床を押す前方両足蹴り、千鶴同じ右手のカバン打撃→体をひねる二撃→投げて手元を空にする。蓮attack1/2/3はv13/v2/v3、千鶴はv3/v3/v2、bag_throwはv2。以下の旧通常案/全106という記載は制作開始時の履歴。

bag_throwは4F/12fps/loop、224×160/PPU50/pivot(0.5,0.35)、原画先頭幅20由来の共通倍率と既存32色で4回転を正規化。prebuiltOnlyで旧rawパレットを変えない。地上3段だけ一度放出（速度12/寿命24tick/判定0.4×0.36）、通常分類/前進0.18/被弾追加後退0.18を保持。既存特殊rice_ball/カレーは表示設定を変えない。

全身/頭/腕/靴/各段の接続のレビューはdocs/art_wip/art3e_<character>_attack_review.png（4原画の等倍2x）、同接頭辞のattack1/2/3_preview.gif。ユーザーの手描き/痛み参考はGit管理外のdocs/art_reference/_motion_local/だけに保存、公開キットへ入れない。右肩から肘/手首への連続を自作ガイドで確認。

全3人224×160px/コマ、足元159、PPU50。旧待機の立ち124/100/112px由来の共通倍率・32色を維持する。頭部帯幅は待機37/58/43pxとの差±2px以内、靴も整数拡大で確認する。生成原画は内蔵image_gen、参照は採用済み自作キャラPNG。

- walk：8枚の別姿勢、15fps loop。左接触→左沈み→左支え/右脚曲げ→左上がり→右接触→右沈み→右支え/左脚曲げ→右上がり。基本4キーと新規間4キー、物差しの立ちは歩行枚数に含めない。
- dash：左右を交互にした4キーを[1,1,2,2,3,3,4,4]の8F/15fps loopへ。以前のART3D原画は保持。
- 通常：透真/蓮はパンチ→パンチ→キック、千鶴はカバン3段。カバンは攻撃時だけ。各6F/15fps/nonloop、予備[1,1]→打撃2→戻り[3,3,3]、ActiveFrame2。
- 最新指定は透真/蓮とも右パンチ→左パンチ→右キック、腰/肩の回転と踏み込み/体軸移動を必須。透真は通常攻撃中だけ端末をしまい、両手で打つ。以前の胴固定候補は不採用、立ち倍率/頭/靴を保持して描き直す。
- 今回は地上3段。空中/しゃがみ/後方の方向別攻撃は後続。性能/当たり判定/Timingは変更しない。

全文の実プロンプトと参照の役割はgeneration-log.json、採用版はsource-selection.json、原画の寸法/SHA/採否はsource-inventory.json。normalized/に実マスク分割/共通倍率/頭部の記録、ready/に固定PNG/横シート/JSON/GIF。gait-review.mdに左右の脚とループの目視所見を記録する。pose-guides/は自作の骨格入力図でありゲームアートではない。

```powershell
# 歩きだけの制作中の区切り
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3WalkAttacks.ps1 -WalkOnly
# 歩き完成後、通常制作前の走り接続/確認
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3WalkAttacks.ps1 -GaitsOnly
# 最終全対象・既存待機/しゃがみ/基本動作の回帰
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3WalkAttacks.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3WalkAttacks.ps1 -VerifyOnly
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/RenderGaitReview.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildSpriteGenerationKit.ps1
```

再生成は元リポジトリのmanifest/旧raw/forge-reportが必要。最終対象15クリップ102F、全106クリップ584Fと既存sprite IDを維持。91枚の対象外PNGは変更前SHAと照合する。Unityはrun_unity.ps1 -Tag art3eで全4工程を外部承認で確認し、実行件数はRESUME/最終報告を参照する。
