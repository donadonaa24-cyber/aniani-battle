# Android試作版

## 成果物と操作

- ビルド先：`Build/Android/AniBato-arm64.apk`。Android 8以降、64bit ARM端末向け。
- 横持ち。左下は方向とダッシュ、右下は通常・特殊・ジャンプ、防御・回避・反撃・超必殺。通常・溜め技は長押しして離す。メニュー中は決定・戻るへ切り替える。
- CPU対戦、練習、端末に接続したパッドとタッチのローカル操作を使用する。別の指を離しても長押しを保持し、バックグラウンドへ移ると入力を解除する。
- ネイティブAndroidにはWebRTC接続窓口を追加していない。招待コードによる対戦は公開ブラウザ版を使う。

## 再現

Unity Hubの6000.6.0f1にAndroid Build Support・SDK/NDK・OpenJDKが必要。プロジェクトを開いたエディタを閉じて、ルートで次を実行する。

```powershell
powershell -ExecutionPolicy Bypass -File tools/verify/build_android.ps1 -Tag android -RestoreWindows
```

ARM64・IL2CPP・ETC2 RGBAでビルド。原画は縮小せず透明余白を追加する。元のPNG・ゲームデータを上書きしない。ストア登録・有料サービス・個人用リリース署名鍵は使用しない。Unityの試作用署名のAPKであり、ストア配布版ではない。

## 確認状況

タッチの実Unity重点4件と最終全4工程（Edit834/Play111）、Web再構築・468原画保持・実ブラウザのボタン操作を確認済み。APKの生成/署名/ABI検査は進行中。Android実機での起動・操作・発熱・メモリ使用量は未確認。
