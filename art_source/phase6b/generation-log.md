# フェーズ6B画像生成記録（2026-10-12）

内蔵image_genを使用（CLI/APIキーなし）。新規背景3枚・参照付き中割り3アトラス、採用6枚。中断前に生成した素材を再開時に全復号・品質確認し、追加の生成をせず再利用した。透過アトラスのアルファと全6人型を検査し、顔・服・手足・足元を確認。低優先度のidleは維持。

保存：backgrounds/city.png・board.png・festival.png、motion/toma.png・ren.png・chizuru.png。プロジェクト用の加工済み背景・サムネは `AniBato/Assets/_Project/Art/Stages/phase6b_*_background.png` / `*_thumbnail.png`。中割りの加工済みPNG/JSONは `Art/Sprites/*/{walk,dash}`、GIFは `docs/art_wip/*_{walk,dash}_preview.gif`。旧raw原画は保存し、生成された中間姿勢をwalk_phase6b/dash_phase6bへ追加。1コマに複数原画を混ぜない。

## 最終プロンプト：背景（transparent_background=false、参照画像なし）

city:

> Use case: stylized-concept. Asset type: final 16:9 2D side-view fighting-game background for original Japanese game あにあにバトル！！, stage 星の終わり. Paint a richly detailed near-future blue-gray city at luminous dusk/night: distant layered towers, curved observatory launch spire, restrained cyan electronic windows, middle-distance broad transit deck with data conduits. No people, no characters, no text, no logos, no combat platforms. Horizontal frontal side-view, wide panorama, softened architectural detail in the central lower fighting area, clear separation between far skyline and middle buildings. Moderately bright muted colors; character sprites will be brighter. Atmosphere with soft blue sky gradient and pale horizon, crisp pixel-art inspired hand-painted anime environment. Center lower third uncluttered for fighters. Produce one landscape image.

board:

> Use case: stylized-concept. Asset type: final 16:9 2D fighting-game scenery, original game stage 推し駒Battle. Wide frontal side-view of a monumental tabletop arena: a clearly readable SIX BY SIX checkerboard seen in shallow perspective extending from lower foreground into center middle distance, exactly six columns and six rows alternating warm ivory and muted bronze-gray, thin visible grid lines. Giant original sculptural chess/shogi-like game pieces stand at both sides with simple crown silhouettes, layered stone arena terraces and distant audience-free arches, pale warm daylight sky. No people, no recognizable third-party characters, no text, no logos. Muted colors and medium brightness for bright fighter visibility. Center uncluttered. Platform geometry will be separately drawn in code, do not add elevated fighting platforms. Crisp pixel-art inspired hand-painted anime environment. Produce one wide landscape.

festival:

> Use case: stylized-concept. Asset type: final 16:9 side-view 2D fighting-game scenery for original stage Battle à la carte. Bright, warm Japanese school festival cooking stalls at early evening: distant school windows and courtyard trees, middle-distance orange and cream awnings, cooking pots, neatly stacked plates, rice bowls, worktops, string of paper pennants, empty kitchen booths. No people, no text, no logos, no third-party characters. Frontal wide horizontal camera, central lower third with broad uncluttered cooking counter region for foreground fighters and code-drawn platform. Distinct distant school, middle stall and near utensils, softened detail and restrained saturation, brighter than a dark silhouette placeholder but less saturated than character sprites. Crisp pixel-art inspired hand-painted anime environment. Produce one wide landscape.

## 最終プロンプト：中割り（transparent_background=true）

各呼出の参照1＝`art_source/raw/<character>/walk.png`、参照2＝同`dash.png`。別キャラの画風参考や第三者ファンアートは入力していない。下記共通文＋各キャラの末尾文を1回ずつ送った。

> Use case: identity-preserve. Asset type: newly DRAWN animation in-between poses for an original 2D fighter. Image 1 reference is the existing walk strip; image 2 reference is existing dash strip. Produce ONE atlas containing exactly SIX complete figures in a precise 2-column by 3-row layout, all facing RIGHT, all separated with large clear gutters, full head to shoes, consistent identity and costume and linework. Top row: two new WALK passing/weight-transfer poses between the reference contact poses, one left-foot passing and one right-foot passing. Middle row: two new DASH running poses with opposite knees driving, torso leaned forward. Bottom row: two new DASH transition poses between contact and flight, opposite feet returning under hips. Arms swing opposite legs. Same scale and head proportions throughout. Every pose must have exactly two arms and two legs, crisp opaque continuous outlines, NO ghost limbs, NO blended images, NO crossfade, NO motion blur, NO transparency within body, NO smear, no text or numbering. Draw all figures from scratch as unique intermediate poses; use references only for identity/style. Transparent background.

- toma: `Preserve black tousled hair, blue eyes, black suit, white shirt, purple tie and handheld black rectangular device.`
- ren: `Preserve dog hood, brown hair, teal eyes, white/orange/teal outfit with dog paw patterns, shorts, sneakers, cape and tail. Keep patterns consistent and legible.`
- chizuru: `Preserve brown bob, orange sweater, navy flared skirt, white sneakers. This is casual clothing, no chef costume.`

新6姿勢は各アトラスの独立した人物から分離。walkへ2、dashへ4を追加し、共通倍率と32色・二値アルファ・足元を整える。隣接原画の画素を混ぜたり、不透明度を重ねたりしていない。蓮walkは6種類の綺麗な絵を8Fへ保持再生、他walk/dashは8種類。アニメの発生/持続/硬直とidleの胴/足固定を維持。

背景の正史、透真の新装備、蓮の追加設定等は確定しない。城郭・月・夕景・屋台や動作は本作独自のクロスオーバー表現。原本を改変しない。
