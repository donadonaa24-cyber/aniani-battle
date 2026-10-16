const fs=require('fs'),path=require('path'),crypto=require('crypto'),sharp=require('sharp');
const root='art_source/paper_stages',file=root+'/manifest.json';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,v)=>{fs.mkdirSync(path.dirname(p),{recursive:true});fs.writeFileSync(p,JSON.stringify(v,null,2)+'\n');};
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const stages=[['future','星の終わり','青・紺と控えめなシアン。管理AIの未来都市、電子の窓、打ち上げタワー'],['board','推し駒Battle','蜂蜜色の木目と朱色・金。将棋風の6×6盤面、無地の五角形の駒、塔'],['festival','Battle à la carte','暖かなオレンジとクリーム。秋の文化祭、校舎、調理場、屋台、黒板、料理と花飾り']];
const layers={
 far:['遠景',false,'横長の舞台背景1枚。幅:高さ2.4:1、空と遠い建物の書き割り、複数の紙を重ねた穏やかな遠景。全キャンバスを背景で埋める。中央は細部を減らす。地面や戦闘足場を描かず、遠い都市／塔／校舎を下側に配置する。不透明。'],
 mid:['中景',true,'横長2.4:1の独立した舞台の書き割り1レイヤー。下の端に並ぶ大きな建物や小道具。上半分は大きく透明に空け、手前の人物を邪魔しない。未来都市なら窓と回路のビル群、将棋なら6×6の盤を背中に大きく立てた工作セットと両端の塔／無地の駒、文化祭なら屋台と無地黒板、鍋・カレー・おにぎりの絵看板。外は完全透明。'],
 frame:['舞台枠',true,'横長16:9の舞台額縁。左右端の細い袖幕と、上端の小さな無地の看板、少量のマスキングテープ。中央の横幅85%×高さ85%は完全透明で何も置かない。未来は紺色、将棋は朱と金、文化祭は紅白。画面下側も広く透明。枠だけ1枚。'],
 main:['メイン足場',true,'1個の極端に横長の帯状の足場だけ。透明な横長キャンバス中央へ、物体自体の幅:高さは約10:1、余白を上下に残す。上端の輪郭は全幅にわたり完全にまっすぐ水平。その下へ厚紙の断面・短い硬い影と貼り重ねた紙を描く。未来は青い細線入り金属パネル風の厚紙、将棋は等間隔に6列の盤の木目、文化祭は調理台の木の天板と紙工作の断面。遠近で台形にしない。上面に出っ張り・小物を置かない。外は透明、台だけ1個。'],
 upper:['小足場',true,'1個の横長の薄い小足場だけ。物体自体の幅:高さは約7:1、透明キャンバス中央、上下に余白。上端は全幅にわたり完全にまっすぐ水平。その下に厚紙の断面と短い硬い影。未来は青く光る金属パネル風厚紙、将棋は木の駒台、文化祭はまな板。遠近で台形にしない。上面に突起・道具・駒を置かない。外は透明。'],
 warning:['縮小予告',true,'小さな紙の舞台小道具1個だけ、正方形の透明キャンバス。未来は赤い警告ビーコンの紙工作、将棋はめくれかけた木目の盤の一マス、文化祭はたたまれかけた紅白の小旗。文字・記号・数字を入れない。シンプルで小さく表示しても読める輪郭。'],
 removal:['縮小小物',true,'小さな紙の舞台小道具1個だけ、正方形の透明キャンバス。未来は切り離されたシアンの正方形の紙回路の一片、将棋は裏返りかけた正方形の木目の盤一マス、文化祭は片付け中の紙皿と折れた小さな旗をひとまとまりにした形。小さく表示しても読める輪郭。文字・数字なし。']
};
async function main(){const [command,id,input]=process.argv.slice(2);
 if(command==='init'){
  if(fs.existsSync(file)){console.log(file);return;}const jobs=[];
  for(const [key,name,theme]of stages)for(const [layer,[label,transparent,content]]of Object.entries(layers)){
   const id=key+'_'+layer,prompt=`オリジナル2D対戦ゲーム「あにあにバトル！！」の生成背景素材。厚紙と切り絵を何枚も貼り重ねたペープサートの舞台セット。${name}用、${theme}。${content}\n普通の手作り工作を丁寧な2Dアニメ背景イラストとして描く。濃淡のある厚紙、切った縁、薄く硬いクリーム白の外周、数ミリ浮かせた短く硬い影、少しずれた重なり。落ち着いた彩度と中間調で、手前のアニメ人物が読みやすい色。写真・写実・3DCG・強い被写界深度・ピクセルアート・文字・ロゴ・透かし・数字・人物・キャラクター・人体・操作UIを描かない。1枚1素材、分割シートや複数案にしない。${transparent?'輪郭の外は本当の透明。グローやぼかした境界なし。':'本レイヤーは不透明の背景。'}`;
   const job={id,key,name,layer,label,transparent,prompt,source:`${root}/${key}/generated/${layer}_v1.png`,version:1,tool:'built-in image_gen',references:[],status:'planned'};
   const promptFile=`${root}/${key}/prompts/${layer}_v1.json`;write(promptFile,job);jobs.push({id,promptFile});
  }write(file,{schemaVersion:1,jobs,entries:[],integrated:false,verifiedTag:null});console.log(JSON.stringify({jobs:jobs.length}));return;
 }
 const m=read(file),job=read(m.jobs.find(j=>j.id===id).promptFile);
 if(command==='job'){if(fs.existsSync(job.source))throw Error('Saved source; do not regenerate');console.log(JSON.stringify(job));return;}
 if(command!=='save')throw Error('init/job/save');
 const allowed=path.resolve(process.env.USERPROFILE,'.codex/generated_images')+path.sep;
 if(!path.resolve(input).toLowerCase().startsWith(allowed.toLowerCase()))throw Error('Use built-in output');
 fs.mkdirSync(path.dirname(job.source),{recursive:true});if(fs.existsSync(job.source)){if(sha(job.source)!==sha(input))throw Error('Do not overwrite source');}else fs.copyFileSync(input,job.source);
 const {data,info}=await sharp(job.source).ensureAlpha().raw().toBuffer({resolveWithObject:true});let left=info.width,top=info.height,right=-1,bottom=-1,transparentPixels=0;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){const a=data[(y*info.width+x)*4+3];if(a===0)transparentPixels++;if(a>=128){left=Math.min(left,x);top=Math.min(top,y);right=Math.max(right,x);bottom=Math.max(bottom,y);}}
 const old=m.entries.find(e=>e.id===id&&e.version===job.version);if(old)throw Error('Recorded source already exists');m.entries.push({...job,sha256:sha(job.source),native:{width:info.width,height:info.height},visible:{left,top,right,bottom},transparentPixels,status:'candidate'});write(file,m);console.log(JSON.stringify({saved:job.source,size:[info.width,info.height],visible:{left,top,right,bottom},transparentPixels}));
}main().catch(e=>{console.error(e.stack);process.exitCode=1;});
