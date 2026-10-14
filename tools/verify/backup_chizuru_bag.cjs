// User explicitly requested retaining the bag version until they request its deletion.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=process.cwd(),read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const manifestPath='art_source/paper_cutout/variants/bag-backup.json';
if(fs.existsSync(manifestPath))throw Error('Bag backup already exists; do not overwrite');
const cat=read('art_source/paper_cutout/review-catalog.json');
const keys=new Set(['attack1','attack2','attack3','crouch_attack','back_attack','air_neutral','air_forward','air_back','air_up','air_down','air_forward_up','air_forward_down','effect_bag_throw']);
const output='イラスト確認/千鶴/予備_カバン版_削除禁止',entries=[];
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
fs.mkdirSync(output,{recursive:true});fs.mkdirSync('art_source/paper_cutout/variants',{recursive:true});
for(const r of cat.entries.filter(r=>keys.has(r.key))){
 const copy=output+'/'+r.title+'_カバン版_予備.png',sha256=hash(r.source);
 if(fs.existsSync(copy))throw Error('Backup copy already exists');
 fs.copyFileSync(r.source,copy);if(hash(copy)!==sha256)throw Error('Backup copy mismatch');
 entries.push({key:r.key,title:r.title,source:r.source,copy,sha256,prompt:r.source.replace('.png','.prompt.json')});
}
if(entries.length!==13)throw Error('Bag backup incomplete');
fs.copyFileSync('art_source/paper_cutout/runtime-manifest.json','art_source/paper_cutout/variants/bag-runtime-manifest.json');
fs.writeFileSync(manifestPath,JSON.stringify({schemaVersion:1,protected:true,releaseOnlyOnExplicitUserDelete:true,reason:'カバン版は消してと指示があるまで予備として保持する（2026-10-15）',sourceCommit:'50cc90a12f55bbb5120f2e89963da01641e77e2d',outputFolder:output,entries},null,2)+'\n');
fs.writeFileSync(output+'/このフォルダについて.txt','ユーザーの明示的な削除指示があるまで保持するカバン版の予備です。13枚は現在の原画と同一バイト。対応表はart_source/paper_cutout/variants/bag-backup.json。原画・全文条件とGitコミットも保持しています。\n');
process.stdout.write(JSON.stringify({backupImages:entries.length,protected:true})+'\n');
