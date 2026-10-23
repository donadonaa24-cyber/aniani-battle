# 容量最適化の完了（2026-10-23）

Windows配布ZIPは509049136→204935031bytes、展開後は2520696705→276859563bytes。未採用参照120枚を除外し、原寸305枚は品質検査済みBC7、7枚は元形式。原PNG468枚/ゲームData248件はSHA不変。

実Unity全4（Edit836/Play111）＋最終Sampler変更のEdit2/Windows再ビルド/画像312・支点324検査/15秒起動が成功。梱包156ファイルと公開ZIP/APKを全SHA照合。Release v0.1.1、ポータルPR23（c5ea60b9）、Linux CIとPages成功、PC/スマホ表示を確認。詳しい限界と再現はNATIVE_DOWNLOADS.md、数値証跡はbuild_size_optimization.json。
