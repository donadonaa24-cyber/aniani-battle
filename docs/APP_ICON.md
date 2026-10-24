# あにバト！！のAndroidアイコン

2026-10-24の所有者指定で作成。紙の劇場、赤い幕、切り抜いた星と「あにバト！！」を組み合わせた本作専用デザイン。

- 原画：`art_source/app_icon/anibato-icon-original-v1.png`
- 全文プロンプト：`art_source/app_icon/prompt.txt`。組み込みのimage_genで生成。既存キャラや他作品のロゴは使用しない。
- Unity素材：`Assets/_Project/Art/Branding/app-icon-legacy.png`、`app-icon-background.png`、`app-icon-foreground.png`
- 原画からの梱包は `tools/art/build_app_icon.cjs`（Sharp）。リサイズと透明余白の追加だけを行う。原画を保持する。
- `AppIconBuilder.ConfigureAndroid`をBuildAllとAndroidBuilderから呼び、対応する全アイコン枠へ設定する。使用するUnity 6000.6はAndroidのAdaptiveを6密度で公開するため、全枠の背景/前景と、平面画像用の共通Defaultアイコンを指定する。丸型表示もAdaptiveをOSがマスクする。
- AdaptiveはOSの72/108ビューポートとランチャーの丸/角丸マスクを考慮し、主題を中央へ収める。Unityロゴを使わない。

APKの署名・ARM64・起動Activityに加え、実リソースのAdaptive XMLと前景/背景、Legacy PNGを検査する。最終結果はRESUMEと検証記録へ記載。実Androidホーム画面での確認は所有者の実機確認と区別する。

Unity 6.6の対応形式は [Android Player settings公式資料](https://docs.unity3d.com/6000.6/Documentation/Manual/class-PlayerSettingsAndroid.html#Icon) と実APIで確認した。以前のUnityのLegacy/Round専用枠が必ず存在すると仮定しない。
