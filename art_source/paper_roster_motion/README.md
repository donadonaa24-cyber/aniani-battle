# 専用7動作の原画

対象は千鶴以外の8人、走り・ジャンプ上昇・落下・回避・カウンター成功・通常被弾・強被弾の各1枚、計56枚。作業指示は docs/codex_tasks/PAPER_ROSTER_SEVEN_MOTION_KEYS.md。

`progress.json` が全体の進行表。各人物の `manifest.json` は候補も含む生成履歴、`prompts/` は全文条件と参照の役割、`generated/` は元の透明PNG、`prepared/` は整数移動と透明余白だけの共通枠。拡大縮小はしない。

`packed-manifest.json` はUnity接続用データで、全7枚を揃えてから作る。ゲームで使用中の版はUnity検証後の `current-adoption.json` で確認できる。`artApproved:false` は最終的な美術承認と実装・テスト合格を分けるための表示。テスト合格だけで人体の正しさを保証しない。

現在採用の日本語画像は `イラスト確認/現在ゲームで採用中/<人物>/`。生成中の候補は `イラスト確認/<人物>/追加7動作_確認中/`。この2つを混同しない。

2026-10-19、龍太の通常被弾はユーザーが「これでいい」と添付した握り拳版v5で確定。承認参照は `ryuta/user_approved_hit_light.png`、選択理由とSHAはmanifestに記録。開いた掌のv6は不採用。特定画像の承認はそのentryの `artApproved:true` と `part-review.json` の `userApprovedActions` で示し、全画像の美術最終承認と混同しない。

透真counter_success_v1は、ユーザーから腕の折れ方が異常と指摘があったため不採用。v1を接続したmotion_toma_1/2の技術検証が合格しても、この絵を採用へ戻さない。v2以降の自然に腕を伸ばした修正版でゲーム素材と検証を更新する。

全身1枚を動かすペープサート方式を維持し、攻撃値・物理・被弾条件・ヒットストップは変更しない。通常攻撃の4原画、下と空中N各1枚、既存ガード・しゃがみ・構え・勝敗は維持する。

全8人56動作は生成・接続・Unity全4工程・Web確認まで完了。進行表はcomplete。最終機械可読証跡はdocs/codex_logs/motion_roster_final_verification.json、Web実画面はdocs/screenshots/motion_roster_web_3/。
