// Diagnostic crops only. Never edit, normalize, or overwrite game artwork.
const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp');
const catalog=JSON.parse(fs.readFileSync('art_source/paper_cutout/review-catalog.json','utf8').replace(/^\uFEFF/,''));
const cases=[
 ['idle_v1','待機',[575,590],[855,590]],['run_v3','走り',[330,595],[917,482]],
 ['crouch_v1','しゃがみ',[650,720],[828,731]],['guard_v1','ガード',[757,424],[869,416]],
 ['jump_v1','ジャンプ',[222,600],[1020,560]],['fall_v1','落下',[300,222],[1035,437]],
 ['dodge_v2','回避',[300,770],[880,688]],['counter_v1','カウンター',[600,458],[1000,420]],
 ['counter_success_v1','反撃成功',[529,450],[1026,410]],['hit_light_v1','軽い被弾',[665,530],[970,521]],
 ['hit_heavy_v1','強い被弾',[738,546],[874,434]],['ko_v2','敗北',[718,990],[850,977]],
 ['win_v1','勝利',[550,623],[910,126]],['attack1_v3','カバン一段目',[989,484],[720,572]],
 ['attack2_v2','カバン二段目',[1000,673],[315,600]],['attack3_v2','カバン投げ',[1160,443],[255,602]],
 ['crouch_attack_v2','しゃがみ攻撃',[850,943],[662,810]],['back_attack_v2','後方攻撃',[329,365],[1023,583]],
 ['air_neutral_v1','空中中立',[962,341],[340,560]],['air_forward_v2','空中前',[1000,473],[205,584]],
 ['air_back_v1','空中後',[346,400],[1083,570]],['air_up_v1','空中上',[460,125],[1084,661]],
 ['air_down_v1','空中下',[750,883],[1033,267]],['air_forward_up_v1','空中斜め上',[950,175],[253,623]],
 ['air_forward_down_v1','空中斜め下',[958,762],[336,214]],['special_neutral_v1','おにぎり',[281,302],[900,650]],
 ['special_side_v4','フライパン',[995,625],[163,626]],['special_up_v2','おたま・修正前',[497,160],[1083,635]],
 ['special_down_v1','食事',[663,443],[864,548]],['ultimate_v1','満腹カレー',[330,231],[1002,480]]
];
async function main(){
 const dir='docs/art_wip/paper_hands';fs.mkdirSync(dir,{recursive:true});
 for(let page=0;page<5;page++){
  const rows=cases.slice(page*6,page*6+6),composites=[];
  for(let row=0;row<rows.length;row++){
   const [legacyKey,,...points]=rows[row],entry=catalog.entries.find(r=>r.key===legacyKey.replace(/_v\d+$/,''));
   if(!entry)throw Error('Missing current hand reference: '+legacyKey);
   const file=entry.source,key=path.basename(file,'.png'),label=entry.title;
   const m=await sharp(file).metadata();
   for(let column=0;column<points.length;column++){
    const [cx,cy]=points[column],left=Math.max(0,Math.min(m.width-180,cx-90)),top=Math.max(0,Math.min(m.height-180,cy-90));
    const crop=await sharp(file).extract({left,top,width:180,height:180}).resize(300,300,{kernel:'nearest'}).flatten({background:'#eeeeee'}).png().toBuffer();
    composites.push({input:crop,left:20+column*320,top:row*350+40});
   }
   const title=Buffer.from(`<svg width="660" height="35"><text x="12" y="25" font-size="21" font-family="Yu Gothic,Meiryo" fill="#222">${label} / ${key}</text></svg>`);
   composites.push({input:title,left:0,top:row*350});
  }
  await sharp({create:{width:660,height:rows.length*350,channels:4,background:'#ffffff'}}).composite(composites).png().toFile(`${dir}/hands_${page+1}.png`);
 }
 fs.writeFileSync(`${dir}/README.md`,'# 千鶴の手元の確認資料\n\n原画の手元を確認するための拡大切り出し。左右の欄は本人の右手／左手を自動判定したものではない。肩と腕の所属は元の全身画像も見て判断する。ゲーム用原画は変更していない。持ち手／親指／甲と掌／指／手首を確認する。\n');
 process.stdout.write('30姿勢の手元確認用5ページを保存。原画は未変更。\n');
}
main().catch(e=>{process.stderr.write(e.stack+'\n');process.exitCode=1;});
