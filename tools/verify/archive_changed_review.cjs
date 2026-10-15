// Preserve only managed copies that would otherwise be overwritten by a new adoption.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const root=path.resolve('イラスト確認/千鶴'),cat=read('art_source/paper_cutout/review-catalog.json');
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const planned=new Map(cat.entries.map(e=>['01_ゲームで採用済み/'+e.title+'_採用済み.png',hash(e.source)]));
function checked(p){const r=path.resolve(root,p),rel=path.relative(root,r);if(!rel||rel.startsWith('..')||path.isAbsolute(rel))throw Error('Unsafe archive path');return r;}
let moved=0;
for(const old of read(path.join(root,'.managed.json'))){
 if(planned.get(old.file)===old.sha256)continue;
 const from=checked(old.file),to=checked('予備_上下振りと旧サイズ/以前の採用表示/'+path.basename(old.file));
 if(!fs.existsSync(from))continue;
 if(hash(from)!==old.sha256)throw Error('Managed copy modified by user');
 if(fs.existsSync(to))throw Error('Archive destination exists');
 fs.mkdirSync(path.dirname(to),{recursive:true});fs.renameSync(from,to);moved++;
}
process.stdout.write(JSON.stringify({moved,sourcesDeleted:0})+'\n');
