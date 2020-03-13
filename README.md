本内容を利用した場合の一切の責任を私は負いません。

# 機能
テトリスの動画を解析して、ミノの積み位置を抽出します。    
「 https://www.youtube.com/watch?v=kJohHun8ANI 」の左フィールドを抽出したものが https://github.com/github895439/tetris_stacking_position_picker/tree/master/log です。    
(ログの「表示無し」をチェック入りの状態で40分弱かかりました。)    
基本的に、画面のログとログファイル(実行ファイルの場所に出力)の内容は同じです。    
ログファイルは1ファイル/クレジットです。    
ログの内容は、

□:ミノ落ち前後で存在したままのブロック    
■:ミノが落ちる前と比較して、増えたブロック(=落ちたミノ)    
◆:ミノが落ちる前と比較して、減ったブロック    

ライン消しがあった場合とその後近辺はライン消しの影響があり、判別が困難なため、間違った結果になっています。    
対戦モードには対応していません。    
同ミノ連続した場合も判別できないため、2ミノが一度に落ちたような結果になっています。    
「f」単位は経過フレームで、タイムスタンプはそれを時間換算したものです。

# バージョン
- OS    
OS 名:                  Microsoft Windows 10 Home    
OS バージョン:          10.0.18362 N/A ビルド 18362    
システムの種類:         x64-based PC
- 開発    
Visual Studio Community 2015(以降、VS)    
OpenCVSharp4 v4.2.0.20200208(nuget)    
OpenCVSharp4.runtime.win v4.2.0.20200208(nuget)    
OpenCVSharp4.Windows v4.2.0.20200208(nuget)    

# ビルド
1. 本ツールをダウンロード    
https://github.com/github895439/tetris_stacking_position_picker

1. ダウンロードしたものを展開

1. 展開したソリューションをVSでオープン

1. OpenCVSharpをインストール    
VSのプロジェクトの右クリックメニューの「NuGetパッケージの管理」を選択する。    
「opencvsharp4」を検索し、結果一覧の「OpenCVSharp4.Windows」をインストールする。    
(依存関係により、バージョンに挙げた残り2つのOpenCVSharpも自動的にインストールされる。)    
「インストール済み」タブにバージョンに挙げた3つのOpenCVSharpが表示される。

1. ソリューションのビルド

# 使い方
1. 直接入力か、「動画ファイル選択」ボタンで動画ファイルを選択

1. 下記の設定を入力(座標の特定は「?」ボタンにガイド有り)
- NEXT判定座標
- プレイ開始判定座標
- プレイ終了判定座標
- フィールド判定座標

1. 「抽出実行」ボタン押下(※markdownのせいで番号がおかしく、手順の続き)

その他:
- 「動画ファイル概要」ボタン    
動画ファイルの情報を表示します。

- 「動画ファイル変更」ボタン    
抽出を行った場合はデータを破棄する必要があるため、このボタンを押します。    
このボタンを押すと、動画ファイルのパスや「動画ファイル概要」ボタンが復帰します。

- 「抽出中止」ボタン    
抽出中に途中で止めます。

- 「終了」ボタン    
ツールを終了します。

# 備考    
下記は設計メモです。    
(「追記」以降がコーディングを始めてから追記した内容です。    
https://github.com/github895439/tetris_stacking_position_picker/blob/master/design_memo.txt    
フレームの確認はaviutlを使用しています。
