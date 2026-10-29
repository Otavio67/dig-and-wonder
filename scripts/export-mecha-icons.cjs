// Toy-Mecha v2: original editable pictograms, shared by combat and workshop.
// Usage: node scripts/export-mecha-icons.cjs <absolute path to sharp>
const fs = require('node:fs');
const path = require('node:path');
const sharp = require(process.argv[2] || 'sharp');
const root = path.resolve(__dirname, '..');
const source = path.join(root, 'assets/ui/gameplay');
const output = path.join(root, 'UnityProject/Assets/_Game/Resources/Art/UI/Icons');
const bolt = '<path d="M66 28L42 68H60L53 101 88 56H69L80 28Z" fill="#00F5D4"/><path d="M65 42L55 61H70" fill="none" stroke="#E7FFFF" stroke-width="3"/>';
const gem = '<path d="M48 22H80L99 55 64 108 29 55Z" fill="#16BDB4"/><path d="M48 22L51 55H29M80 22L77 55H99M51 55L64 108 77 55M51 55H77" fill="none" stroke="#B0FFF3" stroke-width="3"/><path d="M48 22H80L77 55H51Z" fill="#75F6DD" stroke-width="3"/>';
const drill = '<rect x="15" y="39" width="25" height="51" rx="7" fill="url(#copper)"/><path d="M35 28L48 28 115 64 48 100H35Z" fill="url(#steel)"/><path d="M39 29Q58 63 39 99M59 36Q79 63 59 92M80 48Q95 63 80 81M99 58L106 70" fill="none" stroke="#253C48" stroke-width="8"/><path d="M21 48V78" stroke="#FFE29A" stroke-width="4"/><path d="M45 30L104 61" stroke="#ECF4EE" stroke-width="4"/>';
const shield = '<path d="M64 13L108 31 103 79Q91 103 64 116Q37 103 25 79L20 31Z" fill="url(#steel)"/><path d="M64 26L95 39 91 73Q82 93 64 102Q46 93 37 73L33 39Z" fill="#174D50"/><path d="M64 26V102" stroke="#48AEAA" stroke-width="3"/><circle cx="29" cy="37" r="3" fill="#FFCB62" stroke="none"/><circle cx="99" cy="37" r="3" fill="#FFCB62" stroke="none"/>';
const heart = '<path d="M64 88L42 66C25 48 43 33 55 43L64 52 73 43C85 33 103 48 86 66Z" fill="url(#coral)"/><path d="M46 49Q52 46 57 54" fill="none" stroke="#FFE5CC" stroke-width="4"/>';
const wrench = '<path d="M77 18Q96 10 109 25L91 28 86 41 96 51 113 46Q111 68 88 70L51 109Q36 123 22 107Q12 95 26 82L64 46Q60 28 77 18Z" fill="url(#steel)"/><path d="M33 91L68 56 79 67 45 102Z" fill="url(#copper)"/><circle cx="32" cy="100" r="6" fill="#13333C"/><path d="M72 27L72 39" stroke="#EDF6ED" stroke-width="3"/>';
const motor = '<path d="M44 18H83V32H44Z" fill="url(#copper)"/><path d="M28 35L41 27H87L100 39V94L85 105H41L28 94Z" fill="url(#steel)"/><rect x="18" y="47" width="19" height="35" rx="5" fill="url(#copper)"/><rect x="91" y="47" width="19" height="35" rx="5" fill="url(#copper)"/><rect x="44" y="41" width="40" height="47" rx="8" fill="#16464B"/><path d="M53 51H75M53 64H75M53 77H75" stroke="#00F5D4" stroke-width="5"/><path d="M43 96H84" stroke="#253C48" stroke-width="5"/>';
const chest = '<path d="M19 56Q19 25 43 25H85Q109 25 109 56V104H19Z" fill="url(#copper)"/><path d="M19 61H109V104H19Z" fill="#226761"/><path d="M33 29V105M95 29V105" stroke="#E2B66E" stroke-width="10"/><path d="M20 61H108" stroke="#142F39" stroke-width="6"/><rect x="51" y="51" width="26" height="27" rx="4" fill="url(#steel)"/><path d="M64 56L71 64 64 72 57 64Z" fill="#00F5D4" stroke-width="3"/>';
const shapes = {
 attack: drill, drill,
 health: shield + heart,
 repair: wrench,
 cadence: motor,
 motor,
 shield,
 core: '<path d="M40 34L29 47V91L44 110H84L99 91V47L88 34" fill="url(#copper)"/>' + gem,
 bolt: '<path d="M38 18H90L105 34V94L90 110H38L23 94V34Z" fill="url(#steel)"/><path d="M39 30H89V98H39Z" fill="#153D46"/>' + bolt,
 special: '<path d="M37 26L22 44V92L39 110H89L106 92V44L90 26Z" fill="url(#copper)"/><path d="M38 39H90V96H38Z" fill="#193D49"/>' + bolt,
 critical: '<circle cx="64" cy="64" r="43" fill="url(#steel)"/><circle cx="64" cy="64" r="29" fill="#175657"/><path d="M64 11V39M64 89V117M11 64H39M89 64H117" stroke="#E4B66C" stroke-width="9"/><path d="M64 46L79 64 64 82 49 64Z" fill="#00F5D4"/>',
 critical_damage: '<path d="M64 12L76 35 104 24 94 50 117 64 94 78 104 104 77 94 64 117 50 94 24 104 34 78 11 64 34 50 24 24 51 35Z" fill="url(#copper)"/><g transform="translate(25 25) scale(.6)">' + drill + '</g>',
 chassis: '<rect x="15" y="78" width="98" height="32" rx="16" fill="#263D49"/><path d="M27 79L34 36 49 26H83L100 39 103 80Z" fill="url(#copper)"/><rect x="38" y="42" width="53" height="31" rx="10" fill="#14363F"/><path d="M51 52V62M77 52V62" stroke="#00F5D4" stroke-width="7"/><circle cx="34" cy="94" r="8" fill="url(#steel)"/><circle cx="64" cy="94" r="8" fill="url(#steel)"/><circle cx="94" cy="94" r="8" fill="url(#steel)"/>',
 nav_workshop: wrench, nav_chests: chest,
 nav_mine: '<path d="M68 40L81 48 47 114 34 106Z" fill="url(#copper)"/><path d="M17 49Q62 3 113 45L85 38 65 36 42 42Z" fill="url(#steel)"/><path d="M62 34L75 40 68 54 55 48Z" fill="#E7B364"/>',
 nav_shop: '<path d="M43 39V27Q64 7 85 27V39" fill="none" stroke="#9CBABE" stroke-width="10"/><path d="M25 39H103L110 109H18Z" fill="url(#copper)"/><path d="M32 50H96V98H32Z" fill="#215354"/><path d="M64 56L81 75 64 95 47 75Z" fill="#FFCC68"/>',
 mission: '<path d="M29 21H99L108 31V112H20V31Z" fill="url(#steel)"/><path d="M33 37H95V99H33Z" fill="#143F49"/><rect x="47" y="14" width="34" height="20" rx="5" fill="url(#copper)"/><path d="M44 66L57 79 83 52" stroke="#00F5D4" stroke-width="10" fill="none"/>'
};
const defs = '<defs><linearGradient id="steel" x2=".3" y2="1"><stop stop-color="#E2EEE3"/><stop offset=".45" stop-color="#95BCC0"/><stop offset=".48" stop-color="#527D88"/><stop offset="1" stop-color="#33515E"/></linearGradient><linearGradient id="copper" x2=".2" y2="1"><stop stop-color="#FFE19B"/><stop offset=".4" stop-color="#E6A33B"/><stop offset=".43" stop-color="#B9712C"/><stop offset="1" stop-color="#78452B"/></linearGradient><linearGradient id="coral" x2="0" y2="1"><stop stop-color="#FF887C"/><stop offset="1" stop-color="#C83E52"/></linearGradient></defs>';
async function main() {
 fs.mkdirSync(output,{recursive:true});
 for (const [name,body] of Object.entries(shapes)) {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 128 128">${defs}<g transform="translate(5 5) scale(.92)" stroke="#122C38" stroke-width="5" stroke-linecap="round" stroke-linejoin="round">${body}</g></svg>\n`;
  fs.writeFileSync(path.join(source,name+'.svg'),svg);
  await sharp(Buffer.from(svg)).resize(256,256).png().toFile(path.join(output,name+'.png'));
 }
 const panel = '<svg xmlns="http://www.w3.org/2000/svg" width="128" height="128" viewBox="0 0 128 128"><path d="M14 3H114L125 14V114L114 125H14L3 114V14Z" fill="#182C37"/><path d="M16 7H112L121 16V112L112 121H16L7 112V16Z" fill="#FFFFFF"/><path d="M17 12H111L116 17V111L111 116H17L12 111V17Z" fill="none" stroke="#152E38" stroke-width="3"/><path d="M18 9H110L118 17" fill="none" stroke="#FFFFFF" opacity=".4" stroke-width="3"/></svg>';
 fs.writeFileSync(path.join(source,'hud_panel.svg'),panel);
 await sharp(Buffer.from(panel)).png().toFile(path.join(output,'hud_panel.png'));
 console.log(`Exported ${Object.keys(shapes).length} Toy-Mecha icons.`);
}
main().catch(e=>{console.error(e);process.exitCode=1;});
