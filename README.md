# H面導波管シミュレーター(HPlaneWGSimulator)  
  
**Latest Release**  
version1.6.0.4  
　[installer](https://github.com/ryujimiya/HPlaneWGSimulator/tree/master/publish)  

![HPlaneWGSimulator](http://cdn-ak.f.st-hatena.com/images/fotolife/r/ryujimiya/20120925/20120925000643.jpg)  

**News**

***2012-11-22 version1.6.0.0 Release***  
●平行平板導波路(TE,TM)、E面導波管(TE)の解析機能を追加しました。  
　E面導波管伝達問題の計算例   
　　[E面コーナーベンド](http://ryujimiya.hatenablog.com/entry/2012/11/22/002733)  
　　[E面T分岐](http://ryujimiya.hatenablog.com/entry/2012/11/22/010458)   
　　[E面スタブ](http://ryujimiya.hatenablog.com/entry/2012/11/22/015323)   
　平行平板導波路(TMモード)の計算例   
　　[誘電体スラブ導波路終端](http://ryujimiya.hatenablog.com/entry/2012/11/25/204048)   
●散乱係数周波数特性グラフを対数で表示できるようにしました。(右クリックでメニューが表示されます。)  

***2012-10-28 version1.5.0.0 Release***  
●界分布図の種類を増やしました。下記４つを表示できます。  
　|Ez|分布図 (従来のもの)  
　Ez実数部の分布図 (新規)  
　(Hx, Hy)ベクトルのベクトル表示  
　複素ポインティングベクトルのベクトル表示  
●線形方程式の解法にバンドマトリクスを考慮した方法(zgbsv)を追加しました。  
●自動計算を追加しました。図面作成時に指定した周波数の計算を自動実行します。  
●図面作成機能を強化しました。  
　位置移動  
　マス目選択（従来の長方形描画に加えて、自由描画、直線描画、楕円描画を追加しました）  
　消しゴム（従来の長方形領域消去に加えて、自由消去、直線消去を追加しました）  
  

**Summary**  
2次元有限要素法を使用してH面導波管回路の反射、透過特性を計算するアプリケーションです。  
30×30のマス目からなる方眼紙上でマス目を選択して回路形状を指定できます。  
電界分布及び散乱係数の周波数特性を図示できます。  
誘電体を充填した導波管を入出力に指定できます。一部充填もできます。  

なお、本アプリケーションでは行列の固有値計算及び線形方程式計算にKrdLab氏のLisysを用いています。  
また、有限要素法行列の計算に関して一部梅谷信行氏のDelFEMを参考にしています。ここに記し深謝致します。  
  
**About License**  
HPlaneWGSimulatorのアセンブリ、ソースコード（下記注釈を除く）の著作権は、りゅうじみやにありますが、それらの利用になんら制限はありません。ただし、動作の保証はできませんので予め御了承願います。  
※DelFEMソースコードの著作権は、梅谷信行氏にあります。  
※同梱されているLisysの著作権は、KrdLab氏にあります。  
　　DelFEM　[有限要素法(FEM)のページ](http://ums.futene.net/)  
　　Lisys　 [KrdLabの不定期日記 2009-05-07](http://d.hatena.ne.jp/KrdLab/20090507)  
  
**Contact To Human**  
何かございましたら下記までご連絡ください。  
りゅうじみや ryujimiya(あっと)mail.goo.ne.jp  
  
