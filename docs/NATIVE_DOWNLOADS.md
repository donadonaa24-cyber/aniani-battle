# Windows・Android試作版の公開配布

2026-10-23の所有者指示により、あにあにポータルにもダウンロード導線を追加する。

- 案内：[ポータルのダウンロード欄](https://donadonaa24-cyber.github.io/aniani-asobiba/anibato/#downloads)
- 配布：[GitHub Releases v0.1.0-trial.20261023](https://github.com/donadonaa24-cyber/aniani-battle/releases/tag/v0.1.0-trial.20261023)
- Windows x64 ZIP：509049136 bytes（約509MB）、展開後約2.5GB。全展開し、`AniBato/AniBato.exe`を起動する。Data・DLLも同じフォルダに必要。
- Android ARM64 APK：161961932 bytes（約162MB）。Android8以降・横持ちタッチ対応。試作用署名、Google Play外。実機の動作・発熱等は未確認。
- 招待コードによるオンライン対戦はブラウザ版のみ。ダウンロード版はCPU対戦・練習・ローカル操作向け。

公開配布先はゲーム本体のGitHub Releases。バイナリをGit履歴やSupabase Storageへ追加しない。Git LFSや課金サービスを使用しない。[GitHubの配布仕様](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases)の1ファイル2GiB未満に収まる。

## 梱包と確認

`tools/verify/package_native_release.ps1`で既存検証済みの`Build/mobile_final_2/`一式をZIP化し、`Build/Android/AniBato-arm64.apk`を版名付きで複製する。出力は`Build/Distribution/v0.1.0-trial.20261023/`。既存出力は上書きしない。Windowsランタイム・フォントライセンス・導入案内を含め、165エントリを照合済み。README/SHA256SUMSもReleaseへ添付する。

`tools/verify/verify_native_downloads.cjs`は公開配布物を認証なしでストリーム読み取りし、サイズ・全バイトSHA-256・ダウンロード名・案内2ファイルのHTTP200を照合する。別の巨大コピーは保存しない。ポータルの配布マニフェストは`anibato/downloads.json`、検査は`native-downloads.test.cjs`。

ゲームの検証済み基準はe959392。今回C#・戦闘Data・原画・WebGLを変更せず、前工程の実BuildAll/Edit834/Play111/Windows成功とAPK署名v2/ABI検査を引き継ぐ。今回の実行は梱包・公開HTTP・ポータルの回帰/表示検証で、Unityを再実行した記録ではない。

Windows SHA-256：`6625b8d052fddf263c3fc9795724b80af02a26dadbed02e8a5a43475dc592a12`

Android SHA-256：`138f9e9e1395d2056bbd0e054cfa82bf7dc6dd2e91fc43dc391c334bdd978571`
