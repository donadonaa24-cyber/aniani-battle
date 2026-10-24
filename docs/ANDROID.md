2026-10-24所有者報告：旧v0.1.0 APKの実機起動をユーザーが確認。端末名・OS版・操作/発熱/メモリの詳細は未提供。今回のv0.1.2配布APKの実機確認とは区別する。新APKは162327300bytesで、スティック/丸ボタン/操作・HUD配置編集/残像/先行カメラ/専用アイコンを追加済み。署名・起動Activity・ARM64・API26と、全13ランチャー画像を検査した。旧APKと同じ署名のため、削除せず上書き更新する。実機の更新動作と表示は未確認。

# Android試作版

公開配布（2026-10-23所有者指定）：[あにあにポータルのダウンロード欄](https://donadonaa24-cyber.github.io/aniani-asobiba/anibato/#downloads)からAPKを取得できる。Windows版と配布・検証手順は[NATIVE_DOWNLOADS.md](NATIVE_DOWNLOADS.md)を参照。

## 成果物と操作

- ビルド先：`Build/Android/AniBato-arm64.apk`。Android 8以降、64bit ARM端末向け。
- 横持ち。左下はスティック、右下は丸ボタンで通常・特殊・ジャンプ、防御・回避・反撃・超必殺。通常・溜め技は長押しして離す。メニュー中は決定・戻るへ切り替える。設定の「操作・HUDの配置」で位置・倍率・不透明度を保存し、変更の破棄・初期化も可能。詳細は[UI_CUSTOMIZATION.md](UI_CUSTOMIZATION.md)。
- CPU対戦、練習、端末に接続したパッドとタッチのローカル操作を使用する。別の指を離しても長押しを保持し、バックグラウンドへ移ると入力を解除する。
- ネイティブAndroidにはWebRTC接続窓口を追加していない。招待コードによる対戦は公開ブラウザ版を使う。

## 再現

Unity Hubの6000.6.0f1にAndroid Build Support・SDK/NDK・OpenJDKが必要。プロジェクトを開いたエディタを閉じて、ルートで次を実行する。

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify/build_android.ps1 -Tag android -RestoreWindows
```

ARM64・IL2CPP・ETC2 RGBAでビルド。原画は縮小せず透明余白を追加する。元のPNG・ゲームデータを上書きしない。ストア登録・有料サービス・個人用リリース署名鍵は使用しない。Unityの試作用署名のAPKであり、ストア配布版ではない。

専用アイコンは[APP_ICON.md](APP_ICON.md)。Unityビルド後、同じGradleで互換アイコンを再構築する工程をbuild_android.ps1が自動実行する。ゲーム本体1506エントリの内容SHAが一致しない場合は最終APKを置き換えない。証跡はcodex_logs/ui_motion_controls.json。

## 確認状況

タッチの実Unity重点4件と最終全4工程（Edit834/Play111）、Web再構築・468原画保持・実ブラウザのボタン操作を確認済み。APK生成（161961932bytes）、署名v2・ARM64・最低API26/対象API36・起動Activity・Unity/IL2CPPライブラリの検査も成功。Windowsを元のフォルダへ復帰し、一時ドライブ解除と修理済みスクリプトの自動解除検査も成功。Android実機での起動・操作・発熱・メモリ使用量は未確認。

日本語パスはAndroidツールの制約で拒否されたため、スクリプトは同じ作業フォルダを一時的な英字ドライブとして参照する。[Microsoftのsubst仕様](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/subst)に基づき、ファイルを移動せず、固有マーカーを照合して割当を解除する。既存のドライブ割当は上書きしない。単独の割当/自動解除検査は `-AliasCheckOnly`。

実行証跡：`docs/codex_logs/mobile_android_verification.json`。試作APKのSHA-256は `138f9e9e1395d2056bbd0e054cfa82bf7dc6dd2e91fc43dc391c334bdd978571`。
