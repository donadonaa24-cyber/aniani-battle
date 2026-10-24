# Windows・Android試作版の公開配布

- 案内：[ポータルのダウンロード欄](https://donadonaa24-cyber.github.io/aniani-asobiba/anibato/#downloads)
- Windows・Android最新配布版：[v0.1.3-trial.20261025](https://github.com/donadonaa24-cyber/aniani-battle/releases/tag/v0.1.3-trial.20261025)
- Windows x64 ZIP：205150711 bytes（約205MB）、展開後276886462 bytes（約277MB）。全展開し、AniBato/AniBato.exeを起動。Data・DLLも同じフォルダに必要。
- Android ARM64 APK：162329904 bytes（約162MB）。Android8以降、試作用署名。旧v0.1.0はユーザーの実機起動報告あり。今回の更新APKの実機操作・アイコン表示・発熱・メモリ使用量は未確認。旧版と同じ署名で削除せず更新する。
- 招待対戦はブラウザ版のみ。ダウンロード版はCPU対戦・練習・ローカル操作向け。

GitHub Releasesで配布し、バイナリをGit履歴・Supabase Storageへ追加しない。課金サービスなし。旧版は上書きせず保持する。

## 最新版の検証（2026-10-25）

戦闘とメニューのタッチ方式を独立設定・保存できる操作更新。既定はスティック/十字キー。実Unity BuildAll・Edit845/845・修理後のPlay全119/119・Windows成功。Windows15秒起動エラー0、WebGL/Androidも成功。全471PNGのRGBA保持、APK署名/13アイコン/旧版との署名一致を確認。ZIP156エントリと、公開ZIP/APKの匿名全バイト取得でサイズ/SHA一致。実機未確認は継続。

ポータルPR28・CI成功・Pages built、6つの配布リンクと新版記事/説明/識別子を照合。PC1366px/スマホ390pxで横はみ出しなし。証跡codex_logs/touch_pad_context.json、ゲームコード337674e。

Windows SHA-256：2eb53f275d96708858a980776101ac8b26d01dd8b24d62792bc7a9a33d3576d4

Android SHA-256：58d0aaa25f5ba5366ef10228250fe6b9f5284cd9c768fc6391faa5741441c67a

## 旧版の容量最適化（2026-10-23の記録）

旧WindowsのZIP509049136→204935031bytes（約60％減）、展開2520696705→276859563bytes（約89％減）。旧姿勢・旧背景等120枚をビルド時の参照から除外（432→312枚）。原PNG468枚とゲームData248件はSHA不変で、原画・カバン予備も開発側に保持。

305画像は原寸の最高品質BC7、誤差の大きい7画像は元形式。GPU画像は完全可逆ではない。原寸・支点・PPU・元フィルタ/ミップを維持。SHA付き品質許可表で新規/変更/未測定画像は元形式へ戻す。保存データは可逆LZ4HC、ZIPはSmallestSize。Web/Androidは既に参照整理/GPU圧縮済みなので現行ファイルを維持し、APKを重複アップロードしない。

## 再現と検証

1. tools/verify/run_unity.ps1 -Tag <作業名> をサンドボックス外で実行（既定BC7、WindowsTextures Losslessで可逆比較）。
2. UnityのWindowsTextureQualityAudit.Runで出荷画像/支点/原寸/元Samplerを検査。新しい原画は測定後に許可表へ追加するまで元形式。
3. tools/verify/compare_build_size.cjsで保存済み基準と比較。Baselineは旧版がある時だけ作り直す。
4. tools/verify/smoke_windows_build.ps1 -Build Build/<作業名>で実Windows起動。
5. tools/verify/package_native_release.ps1 -WindowsBuild Build/<作業名> -Version <新規版>。既存出力は上書きしない。AndroidVersionが既存版なら同じAPKを再利用。156エントリを元ファイルと全SHA照合済み。
6. tools/verify/verify_native_downloads.cjsで公開物を匿名ストリーム取得しサイズ/全SHA/名前/README/チェックサムを確認。別の巨大コピーは保存しない。

実Unity size_balanced_finalはBuildAll/Edit836/836/Play111/111/Windows成功。最後のEditorのみのSampler変更はsize_native_focus Edit2/2・size_native_final Windows再ビルド成功・size_native_quality 312画像/324スプライト検査成功（最小PSNR35.4401dB、平均誤差1.0822/255）、15秒バッチ起動エラー0で確認。起動確認はネイティブの全操作確認ではなく、戦闘検証はUnity PlayModeで行った。

証跡：codex_logs/build_size_optimization.json。元C#戦闘システム・戦闘Data・原PNGは変更なし。

Windows SHA-256：2274257752f263c0d5c10649f1c40c8001ca85cf0f6928106dac94b26c76b52d

Android SHA-256：138f9e9e1395d2056bbd0e054cfa82bf7dc6dd2e91fc43dc391c334bdd978571
