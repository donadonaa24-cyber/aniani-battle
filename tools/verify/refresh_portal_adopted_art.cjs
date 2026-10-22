// Update website derivatives from the exact Unity-adopted ensemble and normal3 keys.
// Requires the bundled sharp package via NODE_PATH; never modifies original artwork.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),sharp=require('sharp');
const root=path.resolve(__dirname,'../..'),portal=path.resolve(process.argv[2]||'TestResults/portal_release');
if(!portal.startsWith(root+path.sep))throw Error('portal checkout must stay inside workspace');
const guidMap=new Map(),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
function scan(dir){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const p=path.join(dir,e.name);if(e.isDirectory())scan(p);else if(p.endsWith('.meta')){const m=fs.readFileSync(p,'utf8').match(/^guid: ([a-f0-9]+)/m);if(m)guidMap.set(m[1],p.slice(0,-5));}}}
scan(path.join(root,'AniBato/Assets/_Project'));
const read=p=>fs.readFileSync(path.join(portal,p),'utf8'),write=(p,s)=>fs.writeFileSync(path.join(portal,p),s);
function source(asset,field){const s=fs.readFileSync(path.join(root,asset),'utf8'),g=s.match(new RegExp('^  '+field+': \\{[^}]*guid: ([a-f0-9]+)','m'))?.[1];if(!guidMap.has(g))throw Error('Missing '+asset+' '+field);return guidMap.get(g);}
async function main(){
const entries=[];
async function image(src,dest,options){await sharp(src).resize(options).webp({quality:88}).toFile(path.join(portal,dest));entries.push({source:path.relative(root,src).replaceAll('\\','/'),image:dest,sourceSha256:sha(fs.readFileSync(src)),webSha256:sha(fs.readFileSync(path.join(portal,dest)))});}
const ensemble=source('AniBato/Assets/_Project/Data/PaperEnsemble.asset','Artwork');
await image(ensemble,'images/anibato/ensemble-20261022.webp',{width:1600,height:900,fit:'contain',background:{r:0,g:0,b:0,alpha:0}});
for(const file of ['index.html','anibato/index.html'])write(file,read(file).replaceAll('ensemble-20261018.webp','ensemble-20261022.webp'));
for(const [key,asset] of [['toma','Touma'],['ren','Ren'],['chizuru','Chizuru'],['minato','Minato'],['shiro','Shiro'],['yui','Yui'],['king','King'],['akatsuki','Akatsuki'],['ryuta','Ryuta']]){
const motion=fs.readFileSync(source('AniBato/Assets/_Project/Data/'+asset+'.asset','PaperMotion'),'utf8');
const pose=motion.match(/  - Key: (?:normal3|attack3)\r?\n[\s\S]*?(?=  - Key:|$)/)?.[0];
if(!pose)throw Error('Missing finishing pose '+key);
const frames=Array.from(pose.matchAll(/    - \{fileID: 21300000, guid: ([a-f0-9]+)/g));
if(frames.length!==2||!guidMap.has(frames[1][1]))throw Error('Missing normal3 strike '+key);
await image(guidMap.get(frames[1][1]),'images/anibato/chars/'+key+'-action.webp',{width:768,height:1024,fit:'inside',withoutEnlargement:true});
}
const provenance=JSON.parse(read('docs/paper-cards-assets.json'));provenance.siteImagery=entries;write('docs/paper-cards-assets.json',JSON.stringify(provenance,null,2)+'\n');
let page=read('anibato/index.html');page=page.replaceAll(' あにバト独自のクロスオーバー演出です。ガチャの所持と試合内の出現・効果は連動しません。','');
page=page.replace('押している間、スタミナを消費して攻撃を防ぐ。ただしスタミナを使い、足りないと','押している間、スタミナを使って攻撃を防ぐ。足りないと');
write('anibato/index.html',page);console.log('Current adopted ensemble and 9 finishing attack keys updated; originals unchanged.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
