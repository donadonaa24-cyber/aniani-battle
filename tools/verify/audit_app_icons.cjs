const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..'),read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8').replace(/^\uFEFF/,''));
async function main(){
 const inventory=read('TestResults/android_app_icons/resources.json'), results=[];
 const sources={app_icon:'legacy',app_icon_round:'legacy',ic_launcher_background:'background',ic_launcher_foreground:'foreground'};
 for(const e of inventory.entries){
  const actual=path.join(root,e.file), source=path.join(root,'AniBato/Assets/_Project/Art/Branding/app-icon-'+sources[e.kind]+'.png');
  const meta=await sharp(actual).metadata();
  const a=await sharp(actual).resize(64,64).ensureAlpha().raw().toBuffer(),b=await sharp(source).resize(64,64).ensureAlpha().raw().toBuffer();
  const error=a.reduce((n,v,i)=>n+Math.abs(v-b[i]),0)/a.length;
  assert(error<8,`${e.kind}: actual APK image does not match brand artwork (${error})`);
  results.push({kind:e.kind,width:meta.width,height:meta.height,meanByteError:error});
 }
 const foreground=inventory.entries.filter(e=>e.kind==='ic_launcher_foreground');
 let chosen=foreground[0],width=0;
 for(const e of foreground){const m=await sharp(path.join(root,e.file)).metadata();if(m.width>width){width=m.width;chosen=e;}}
 // Approximate Android's 72/108 viewport, then apply common launcher masks for visual inspection.
 const fg=await sharp(path.join(root,chosen.file)).resize(384,384).png().toBuffer();
 const image=await sharp({create:{width:384,height:384,channels:4,background:'#092342'}}).composite([{input:fg}]).extract({left:64,top:64,width:256,height:256}).png().toBuffer();
 const circle=Buffer.from('<svg width="256" height="256"><circle cx="128" cy="128" r="128" fill="white"/></svg>');
 const round=Buffer.from('<svg width="256" height="256"><rect width="256" height="256" rx="56" fill="white"/></svg>');
 const images=await Promise.all([circle,round].map(mask=>sharp(image).composite([{input:mask,blend:'dest-in'}]).png().toBuffer()));
 fs.mkdirSync(path.join(root,'docs/screenshots/ui_motion'),{recursive:true});
 await sharp({create:{width:600,height:320,channels:4,background:'#e9e5da'}}).composite(images.map((input,i)=>({input,left:24+i*296,top:32}))).png().toFile(path.join(root,'docs/screenshots/ui_motion/android_icon_masks.png'));
 const report={apk:inventory.apk,adaptiveLauncher:true,bitmaps:results,deviceVerified:false,preview:'docs/screenshots/ui_motion/android_icon_masks.png'};
 fs.writeFileSync(path.join(root,'TestResults/android_app_icons/audit.json'),JSON.stringify(report,null,2)+'\n');
 console.log(JSON.stringify({verifiedBitmaps:results.length,maxMeanByteError:Math.max(...results.map(e=>e.meanByteError))}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});
