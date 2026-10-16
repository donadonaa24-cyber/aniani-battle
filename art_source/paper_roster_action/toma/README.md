# 透真の攻撃原画を再生成・再接続する手順

現在版はmanifest.jsonのjobsで選定したversion。entriesには不採用候補も残す。生成ツールは内蔵image_gen。本文条件、参照の役割、出力SHAはprompts/とentriesに保存してある。最終美術承認とゲーム使用／Unity合格は別の状態。

1. 先にAGENTS・RESUME・REWORK_R4_TOMA_ACTIONSを確認し、別担当との同時編集を避ける。
2. 直接の人物参照は余白を加える前の採用待機generated/toma_idle_v2.png。共通梱包済み待機を参照すると人物が縮んだため、余白前を使用する。低姿勢は採用済みcrouch_v2を使う。他人の顔／体は混ぜない。
3. tools/verify/paper_action_generation.cjs job toma <id>で未生成の全文条件を読む。保存済みjobは重複生成を拒否する。版を増やす際は元画像を上書きせずjobsに新しいpromptFile/versionを指定する。
4. 内蔵image_genでtransparent_background=true。新規の弾は参照なし。人物の編集参照は先にview_imageで読む。生成直後、save toma <id> <ツール出力PNG>でworkspaceへ不変のコピーを保存する。飛翔物はsave-effect toma <id> <ツール出力PNG>。
5. pack tomaで16枚の共通枠を作る。人物画素の拡縮・手の反転加工はしない。可視足元を1328へ平行移動し、地上は採用待機の足幅中心、空中は空中予備の足幅中心を基準にする。はみ出すalpha>1はエラー。alpha1の枠外ノイズは数を報告し元原画に残す。
6. check_paper_action.cjsでSHA、枠内RGBA、余白、足元、枠外ノイズを独立監査する。一覧は確認用の縮小であり、ゲーム画像ではない。part-review.jsonと原寸表示で髪・靴・手を確認する。
7. PaperActionArtworkBuilderが基本5姿勢の後へ2コマ8技と背面の再利用をマージし、攻撃のダメージ開始で当てる絵へ切り替える。本文枠1304×1450、PPU532.5893、支点(652,122)。空中は表示のみFrameOriginYsで頭高を補正する。
8. 飛翔物は原画の可視中心をSprite支点にし、実攻撃判定内へ縦横比を保って表示する。銃弾は速度方向、ロケットは元が下向きなので速度角+90度、ドローンはFacingに合わせ左右反転。速度・寿命・命中・多重防止はR3の純C#を維持。
9. run_unity.ps1の全4工程、左右・予備／有効／硬直／解除・飛翔物・元の戦闘回帰を確認する。確認フォルダに動作名を付け、検証前は確認中、合格後にゲーム採用済みと明示。原画を完成と断定しない。
