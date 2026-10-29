// Fontes vetoriais editáveis e rasterização dos pictogramas do HUD.
// node scripts/export-gameplay-icons.cjs [caminho absoluto do módulo sharp]
const fs = require('node:fs');
const path = require('node:path');
const sharp = require(process.argv[2] || 'sharp');
const repo = path.resolve(__dirname, '..');
const source = path.join(repo, 'assets/ui/gameplay');
const output = path.join(repo, 'UnityProject/Assets/_Game/Resources/Art/UI/Icons');
const cone = '<rect x="13" y="34" width="23" height="60" rx="10" fill="#B5651D"/><ellipse cx="37" cy="64" rx="18" ry="35" fill="#FFB703"/><path d="M39 29L115 64 39 99Q59 64 39 29Z" fill="#B5C6CA"/><path d="M59 39Q78 64 59 89M80 49Q93 64 80 79M99 58Q104 64 99 70" fill="none"/><path d="M43 34L110 64 43 61Z" fill="#FFF8E7" stroke="none" opacity=".35"/>';
const heart = '<path d="M64 104L21 65C-1 43 13 17 36 22Q54 23 64 39Q74 23 92 22C115 17 129 43 107 65Z" fill="#FF595E"/><path d="M19 43L14 62 35 81M109 43L114 62 93 81" fill="none" stroke="#B5C6CA" stroke-width="12"/><path d="M30 38Q40 32 49 43" fill="none" stroke="#FFF8E7" stroke-width="5"/><circle cx="22" cy="61" r="3" fill="#1B263B" stroke="none"/><circle cx="106" cy="61" r="3" fill="#1B263B" stroke="none"/>';
const wrench = '<path d="M83 15Q104 8 114 28L97 26 87 39 94 53 112 54Q101 75 81 65L43 105Q29 119 17 105Q8 95 21 82L61 44Q53 25 70 15L69 33 81 38 91 28Z" fill="#B5C6CA"/><path d="M30 91L77 45" fill="none" stroke="#FFF8E7" stroke-width="4"/><circle cx="28" cy="98" r="6" fill="#1E6F5C"/>';
const shapes = {
  mission: '<path d="M30 19H98V114H20V29Z" fill="#FFF8E7"/><path d="M46 9H82V29H46Z" fill="#B5C6CA"/><path d="M33 48L39 54 49 42M33 73L39 79 49 67" fill="none" stroke="#1E6F5C"/><path d="M61 49H85M61 74H85M33 97H85" fill="none" stroke="#4B485C"/>',
  attack: cone,
  drill: cone,
  health: heart,
  repair: '<g transform="translate(-4 -2) scale(.86)">' + heart + '</g><g transform="translate(54 54) scale(.57)">' + wrench + '</g>',
  cadence: '<rect x="53" y="8" width="22" height="13" rx="3" fill="#B5651D"/><path d="M93 27L104 17 112 26 103 36" fill="#B5C6CA"/><circle cx="64" cy="72" r="43" fill="#B5C6CA"/><circle cx="64" cy="72" r="33" fill="#FFF8E7"/><path d="M64 43V49M35 72H41M87 72H93M64 95V101"/><path d="M64 72L81 49" stroke="#FF595E" stroke-width="8"/><circle cx="64" cy="72" r="5" fill="#1B263B"/>',
  critical: '<circle cx="64" cy="64" r="42" fill="#1E6F5C"/><circle cx="64" cy="64" r="29" fill="#00F5D4"/><path d="M64 10V38M64 90V118M10 64H38M90 64H118" stroke="#B5C6CA" stroke-width="12"/><path d="M64 46L78 64 64 82 50 64Z" fill="#FFF8E7"/>',
  critical_damage: '<path d="M64 9L76 35 104 23 95 52 120 65 93 77 104 106 77 94 63 119 51 93 22 106 33 77 8 64 35 51 23 23 51 35Z" fill="#FFB703"/><g transform="translate(24 28) scale(.65)">' + cone + '</g>',
  special: '<path d="M65 7L26 67H56L42 121 105 48H73L91 7Z" fill="#00F5D4"/><path d="M63 29L43 58H67L59 86 88 56H63Z" fill="#FFF8E7" stroke="none"/>',
  nav_modes: '<path d="M16 84L34 61H94L112 84 104 108H24Z" fill="#9A6642"/><path d="M24 49L43 22H86L104 49 92 73H36Z" fill="#B5651D"/><path d="M62 51L78 69 69 101 50 80Z" fill="#00F5D4"/><path d="M30 48H98M19 84H49M82 84H108" fill="none"/>',
  nav_workshop: wrench,
  nav_pets: '<path d="M64 15V31"/><circle cx="64" cy="13" r="7" fill="#FFB703"/><rect x="10" y="50" width="19" height="37" rx="8" fill="#B5651D"/><rect x="99" y="50" width="19" height="37" rx="8" fill="#B5651D"/><rect x="24" y="32" width="80" height="70" rx="22" fill="#1E6F5C"/><rect x="34" y="47" width="60" height="35" rx="13" fill="#1B263B"/><rect x="45" y="55" width="12" height="17" rx="5" fill="#00F5D4" stroke="none"/><rect x="71" y="55" width="12" height="17" rx="5" fill="#00F5D4" stroke="none"/><path d="M53 91H75" stroke="#FFF8E7" stroke-width="4"/>',
  nav_shop: '<path d="M44 39V24Q64 5 84 24V39" fill="none" stroke="#B5C6CA" stroke-width="9"/><path d="M28 37H100L110 111H18Z" fill="#B5651D"/><path d="M66 59L84 79 67 98 50 79Z" fill="#FFB703"/><circle cx="39" cy="47" r="4" fill="#FFF8E7" stroke="none"/><circle cx="89" cy="47" r="4" fill="#FFF8E7" stroke="none"/>'
};
async function main() {
  fs.mkdirSync(source, {recursive:true}); fs.mkdirSync(output, {recursive:true});
  for (const [name, body] of Object.entries(shapes)) {
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 128 128"><g stroke="#1B263B" stroke-width="6" stroke-linecap="round" stroke-linejoin="round">${body}</g></svg>\n`;
    fs.writeFileSync(path.join(source, name+'.svg'), svg);
  }
  // nav_mine.svg e nav_chests.svg já são fontes canônicas nesta pasta.
  for (const name of fs.readdirSync(source).filter(n=>n.endsWith('.svg'))) {
    // Equal optical bounds: transparent padding belongs to the export, never a
    // rectangle copied from the concept screen. Keep vector sources editable.
    const drawing = await sharp(path.join(source,name)).resize(512,512)
      .trim({background:'#00000000', threshold:0}).toBuffer();
    const fitted = await sharp(drawing).resize(216,216,{fit:'inside'}).png().toBuffer();
    await sharp({create:{width:256,height:256,channels:4,background:'#00000000'}})
      .composite([{input:fitted,gravity:'centre'}]).png().toFile(path.join(output,path.basename(name,'.svg')+'.png'));
  }
  console.log('Exportados '+fs.readdirSync(source).filter(n=>n.endsWith('.svg')).length+' ícones SVG/PNG.');
  // The latest approved direction replaces the older flat pictograms.
  require('node:child_process').execFileSync(process.execPath,
    [path.join(__dirname, 'export-mecha-icons.cjs'), process.argv[2] || 'sharp'], {stdio:'inherit'});
}
main().catch(e=>{console.error(e);process.exitCode=1;});
