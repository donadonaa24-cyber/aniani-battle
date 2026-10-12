# ピクセルアート制作規格（2026-10-12、ART3B以降）

ユーザー指定「今後もすべて同じサイズ」を、生成物をゲームへ渡す共通規格として固定する。

ART3Dのジャンプ/下降/浅い予備・着地/ガード/回避/ダッシュもこの規格。個々のフレーム数/fps/キー割当・空中アンカー・地上2tick予備は[再制作指示書](SPRITE_GENERATION_GUIDE.md)§10、原画/倍率/出力はart_source/art3_actions/。頭部帯幅は全51キーで採用待機±2px以内、靴の向き/投影も目視確認。既存106クリップ584Fを維持。

別サービス/別担当へ渡す詳しい条件・キャラ別設定・コピペ用プロンプト・JSON・加工/検査手順は[再制作指示書](SPRITE_GENERATION_GUIDE.md)。参照PNG/パレット/全文プロンプトを含む自己完結した[生成キット](../art_source/sprite_generation_kit/README.md)と[ZIP](../art_source/sprite_generation_kit.zip)を用意済み。今後の生成でもこのセットを渡す。

| 項目 | 固定値 |
|---|---|
| 全キャラ・全モーションの1コマ | **224×160px**、透過PNG、同じ余白込みキャンバス |
| 地上接地位置 | 足元の最下端y=159（画像上端がy=0）。靴は呼吸中も固定 |
| 解剖学的な水平基準 | 胴中心をx=112へ。伸ばした手足で絵全体を中央寄せしない |
| Unityの倍率 | PPU50、地上キャラpivot(0.5,0)。クリップやコマごとの拡縮なし |
| ピクセル・色 | 正方形の整数ピクセル、キャラ全クリップ共通32色以下、alphaは0/255 |
| インポート | Point、Mipなし、圧縮なし、NPOT縮小なし |
| 身長基準 | 透真124px、蓮100px、千鶴112px。共通サイズは画像セルであり、人物の身長差は保つ |
| ART3B待機 | 8F、6fps、約1.33秒ループ。胴/頭の変位0,-1,-2,-1,0,+1,+2,+1px、靴の下12行は完全固定 |
| 同じ長さのシート | 待機は全員1792×160px（224×8）。単体フレームは全員224×160px |

生成サービスの原画寸法は揃わない場合があるため、原画を保管してからSpriteForgeで上記寸法へ出力する。ユーザーに渡す実装用PNGは必ず共通セルへ正規化する。プレビューの3倍表示は672×480pxで、ゲーム用データを拡大保存する意味ではない。

## ART3Bの再生成と接続

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Idle.ps1 -RebuildCheck
```

基準はart_source/art3_idle/{toma,ren,chizuru}_idle_v1.png。内蔵image_genによる呼吸4キー/人はbreathing/generated/に保管する。キー間には足・輪郭の細かな差があるため、最終アニメは採用済みの1枚絵から整数行移動で作る。顔/衣装は1枚の画素をそのまま上下へ動かし、下腿だけを接地位置へつなぐ。前後の絵の混合、半透明、全身の伸縮は使わない。

ゲーム用idle.png/idle.jsonとAnimationSetへ接続する。旧raw idleは他の全モーションの縮尺・共通パレット基準として残し、新idleだけをマニフェストのprebuiltSheetで差し替える。他の動作の新絵柄への再制作は次の作業。headHeightsは各フレームの実上端から更新し、頭上カウンターも呼吸に追従する。

制作物：art_source/art3_idle/breathing/ready/<キャラ>/の8枚・シート・JSON・GIF。最終プロンプトはbreathing/generation-log.json。原画の保存、既存Timing/独自設定/スプライトIDの維持、Unity全4工程の検証を行う。

## 今後の生成プロンプトに含める共通条件

> Same original character and costume as the approved idle reference. Every exported frame must use a 224x160 transparent canvas, PPU50, feet baseline y159, torso anchor x112, unchanged character proportions and pixel scale. Crisp square pixel clusters, coherent limited palette, no crossfade, ghost limbs, gradients or blurred outlines. Use the same standing-height reference across all motions. Preserve enough clear transparent margin so no extremity clips the cell. Never rescale the entire character independently for each frame.

SpriteForgeとUnityインポータはセル寸法・PPU・基準位置の違反をエラーにする。体の上下動・靴固定・顔/胴の厳密な画素移動・二値alpha・色数・headHeights・GIF復号と再生成一致を検査する。新キャラでも同じセル/PPUを使用し、身長だけを設計した値にする。

## ART3Cしゃがみ

新待機の容姿/衣装を維持し、同倍率の立ち/しゃがみ2姿勢を生成。立ち外接高さを物差しに、しゃがみへ同じ倍率を適用する。頭や靴を縮小せず膝と腰を曲げる。全3人8F/6fps、224×160、足固定の体±2px呼吸。中立の実高さは透真91/蓮77/千鶴84px。原画/分割領域/倍率はart_source/art3_crouch/、実装用8枚・横シート・JSON・GIFはready/<キャラ>/。

再現はtools/SpriteForge/BuildArt3Crouch.ps1（-RebuildCheckまたは-VerifyOnly）。idleは保持し、crouchだけprebuiltSheetで接続する。再制作条件は上の詳細指示書へ集約し、新モーションも同セル/同倍率/共通パレットを使う。

追加ポーズだけ人物が大きくなることを防ぐため、待機との頭/髪・靴の部分寸法比較と等倍目視を必須とする。今回の頭部帯幅差は0〜1px・靴の下8行の幅差は0〜3px。測定領域と基準値は詳細指示書§3とart_source/art3_crouch/proportion-check.json、再検査はCompareArt3Proportions.ps1。別角度は投影幅が変わるため、数値だけで頭や足を拡縮しない。
