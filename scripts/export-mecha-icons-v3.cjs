// Toy-Mecha v3: pictogramas vetoriais originais com volume de adesivo cartoon.
// Diferenças da v2: contorno externo grosso (silhueta lê em tamanho pequeno), sombra dura de cel,
// brilhos curtos e pesos visuais equilibrados entre ícones de diagonal fina e ícones cheios.
// Gera os SVG editáveis em assets/ui/gameplay e PNG 256x256 transparentes via Edge headless.
// Uso (Windows):  node scripts/export-mecha-icons-v3.cjs ["C:/caminho/msedge.exe"]
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { execFileSync } = require('node:child_process');

const root = path.resolve(__dirname, '..');
const source = path.join(root, 'assets/ui/gameplay');
const output = path.join(root, 'UnityProject/Assets/_Game/Resources/Art/UI/Icons');
const edge = process.argv[2] || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';

const INK = '#0E2430';
// Paleta funcional do guia: aço, cobre/latão, petróleo, ciano de energia, coral, âmbar de valor.
const defs = `<defs>
<linearGradient id="steel" x1="0" y1="0" x2=".25" y2="1"><stop offset="0" stop-color="#F1F7F2"/><stop offset=".42" stop-color="#A9C9CC"/><stop offset=".46" stop-color="#5F8C97"/><stop offset="1" stop-color="#2F4E5B"/></linearGradient>
<linearGradient id="copper" x1="0" y1="0" x2=".2" y2="1"><stop offset="0" stop-color="#FFE7A8"/><stop offset=".38" stop-color="#F0AE45"/><stop offset=".42" stop-color="#C47A2E"/><stop offset="1" stop-color="#7C462A"/></linearGradient>
<linearGradient id="teal" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#2FA394"/><stop offset=".5" stop-color="#1E7466"/><stop offset=".53" stop-color="#175D53"/><stop offset="1" stop-color="#0F3F3A"/></linearGradient>
<linearGradient id="crystal" x1="0" y1="0" x2=".4" y2="1"><stop offset="0" stop-color="#D8FFF8"/><stop offset=".35" stop-color="#4DF7DD"/><stop offset=".4" stop-color="#14C9B5"/><stop offset="1" stop-color="#0B7F7A"/></linearGradient>
<linearGradient id="coral" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#FF9C8E"/><stop offset=".45" stop-color="#FF5E63"/><stop offset=".5" stop-color="#E0424F"/><stop offset="1" stop-color="#A72B41"/></linearGradient>
<linearGradient id="gold" x1="0" y1="0" x2=".2" y2="1"><stop offset="0" stop-color="#FFF2B8"/><stop offset=".4" stop-color="#FFC531"/><stop offset=".45" stop-color="#E89A0C"/><stop offset="1" stop-color="#A9620A"/></linearGradient>
<linearGradient id="screen" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#1D5560"/><stop offset="1" stop-color="#0F2E37"/></linearGradient>
<filter id="drop" x="-20%" y="-20%" width="140%" height="150%"><feDropShadow dx="0" dy="4" stdDeviation="0" flood-color="#04131A" flood-opacity=".55"/></filter>
</defs>`;

const shine = d => `<path d="${d}" fill="none" stroke="#FFFFFF" stroke-opacity=".85" stroke-width="4" stroke-linecap="round"/>`;
const gem = (cx, cy, s = 1) => `<g transform="translate(${cx} ${cy}) scale(${s})"><path d="M-12 -14H12L20 -2 0 22 -20 -2Z" fill="url(#crystal)"/><path d="M-12 -14L-7 -2H7L12 -14M-20 -2H20M-7 -2L0 22 7 -2" fill="none" stroke="${INK}" stroke-width="2.5" stroke-opacity=".55"/>${shine('M-8 -9L-4 -4')}</g>`;

const drill = `<rect x="12" y="38" width="28" height="54" rx="8" fill="url(#copper)"/><path d="M34 26H49L118 64 49 102H34Z" fill="url(#steel)"/>
<path d="M45 30Q64 64 45 98M65 39Q83 64 65 89M86 51Q99 64 86 77" fill="none" stroke="#25404C" stroke-width="7"/>${shine('M20 46V70')}${shine('M50 31L100 58')}`;
const heart = `<path d="M64 92L40 68C21 49 41 31 55 43L64 51 73 43C87 31 107 49 88 68Z" fill="url(#coral)"/>${shine('M44 52Q49 45 56 50')}`;
const shieldShape = `<path d="M64 10L110 29 105 80Q93 106 64 120Q35 106 23 80L18 29Z" fill="url(#steel)"/><path d="M64 25L96 39 92 75Q83 96 64 106Q45 96 36 75L32 39Z" fill="url(#teal)"/>`;
const wrench = `<path d="M78 14Q100 6 114 24L94 28 89 42 99 52 118 47Q116 72 90 74L52 112Q35 127 19 110Q8 96 24 82L62 46Q57 25 78 14Z" fill="url(#steel)"/>
<path d="M30 92L66 56 78 68 42 104Z" fill="url(#copper)"/><circle cx="30" cy="101" r="6" fill="${INK}"/>${shine('M74 24L74 36')}${shine('M36 88L58 66')}`;
const bolt = `<path d="M70 18L38 70H60L50 112 94 52H71L84 18Z" fill="url(#gold)"/>${shine('M66 26L50 54')}`;
const coin = `<circle cx="64" cy="64" r="44" fill="url(#gold)"/><circle cx="64" cy="64" r="30" fill="none" stroke="#A9620A" stroke-width="6"/><path d="M64 44V84M54 54H72Q78 54 78 60Q78 66 72 66H56Q50 66 50 72Q50 78 56 78H74" fill="none" stroke="#8A4F08" stroke-width="7"/>${shine('M36 46Q44 32 58 28')}`;

const shapes = {
  attack: drill,
  drill,
  health: shieldShape.replace('url(#teal)', '#163E47') + heart,
  repair: wrench,
  cadence: `<path d="M44 14H84V30H44Z" fill="url(#copper)"/><path d="M26 33L40 24H88L102 37V96L87 108H41L26 96Z" fill="url(#steel)"/>
<rect x="14" y="46" width="20" height="38" rx="6" fill="url(#copper)"/><rect x="94" y="46" width="20" height="38" rx="6" fill="url(#copper)"/>
<rect x="42" y="40" width="44" height="52" rx="9" fill="url(#screen)"/><path d="M52 52H76M52 66H76M52 80H76" stroke="#3DF5DA" stroke-width="6" stroke-linecap="round"/>${shine('M32 38L42 31')}`,
  motor: null,
  shield: shieldShape + `<path d="M64 25V106" stroke="#5FD0C4" stroke-width="4"/>${shine('M30 34L56 23')}`,
  core: `<path d="M38 30L24 46V92L42 114H86L104 92V46L90 30Z" fill="url(#copper)"/><path d="M42 40H86L96 52V88L84 102H44L32 88V52Z" fill="url(#screen)"/>${gem(64, 68, 1.45)}`,
  bolt: `<path d="M36 14H92L108 30V98L92 114H36L20 98V30Z" fill="url(#steel)"/><path d="M38 28H90V100H38Z" fill="url(#screen)"/>` + bolt,
  special: `<path d="M36 18L20 36V96L38 114H90L108 96V36L92 18Z" fill="url(#copper)"/><path d="M36 32H92V100H36Z" fill="url(#screen)"/>` + bolt,
  critical: `<circle cx="64" cy="64" r="46" fill="url(#steel)"/><circle cx="64" cy="64" r="30" fill="url(#teal)"/><path d="M64 8V36M64 92V120M8 64H36M92 64H120" stroke="${INK}" stroke-width="20" stroke-linecap="round"/><path d="M64 8V36M64 92V120M8 64H36M92 64H120" stroke="url(#copper)" stroke-width="11" stroke-linecap="round"/>${gem(64, 64, .95)}`,
  critical_damage: `<path d="M64 6L78 32 108 20 98 50 122 64 98 78 108 108 78 96 64 122 50 96 20 108 30 78 6 64 30 50 20 20 50 32Z" fill="url(#coral)"/><g transform="translate(26 26) scale(.6)">${drill}</g>`,
  chassis: `<rect x="12" y="80" width="104" height="34" rx="17" fill="#22394A"/><path d="M24 81L32 36 48 24H84L102 38 106 82Z" fill="url(#copper)"/>
<rect x="38" y="40" width="54" height="32" rx="11" fill="url(#screen)"/><path d="M52 50V62M78 50V62" stroke="#3DF5DA" stroke-width="8" stroke-linecap="round"/>
<circle cx="32" cy="97" r="9" fill="url(#steel)"/><circle cx="64" cy="97" r="9" fill="url(#steel)"/><circle cx="96" cy="97" r="9" fill="url(#steel)"/>${shine('M38 32L50 28')}`,
  lock: `<path d="M38 56V40Q38 14 64 14Q90 14 90 40V56" fill="none" stroke="${INK}" stroke-width="23"/><path d="M38 56V40Q38 14 64 14Q90 14 90 40V56" fill="none" stroke="url(#steel)" stroke-width="13"/>
<rect x="22" y="52" width="84" height="64" rx="12" fill="url(#copper)"/><circle cx="64" cy="78" r="10" fill="${INK}"/><path d="M64 82V100" stroke="${INK}" stroke-width="9" stroke-linecap="round"/><circle cx="64" cy="78" r="4" fill="#3DF5DA"/>${shine('M32 62H58')}`,
  mission: `<path d="M28 20H100L110 30V116H18V30Z" fill="url(#steel)"/><path d="M32 36H96V102H32Z" fill="url(#screen)"/><rect x="46" y="10" width="36" height="22" rx="6" fill="url(#copper)"/>
<path d="M44 68L58 82 86 52" stroke="#3DF5DA" stroke-width="11" fill="none" stroke-linecap="round" stroke-linejoin="round"/>${shine('M34 26H60')}`,
  nav_mine: `<path d="M62 42L80 52 46 118 28 108Z" fill="url(#copper)"/><path d="M12 52Q60 -2 118 44Q102 40 86 40L74 46 56 44 42 44Q28 46 12 52Z" fill="url(#steel)"/>
<path d="M58 34L78 42 72 56 52 48Z" fill="url(#gold)"/>${shine('M24 44Q56 14 100 32')}${shine('M58 58L44 86')}${gem(96, 92, .75)}`,
  nav_modes: `<path d="M12 86L32 62H96L116 86 106 114H22Z" fill="url(#copper)"/><path d="M22 52L42 22H86L106 52 92 76H36Z" fill="#9C5E34"/><path d="M28 52H100" stroke="${INK}" stroke-width="4"/>${gem(64, 72, 1.5)}${shine('M30 82L40 70')}`,
  nav_workshop: wrench,
  nav_chests: `<path d="M16 58Q16 22 42 22H86Q112 22 112 58V110H16Z" fill="url(#copper)"/><path d="M16 62H112V110H16Z" fill="url(#teal)"/>
<path d="M32 26V111M96 26V111" stroke="#E7BC74" stroke-width="11"/><path d="M17 62H111" stroke="${INK}" stroke-width="6"/>
<rect x="50" y="50" width="28" height="30" rx="5" fill="url(#steel)"/>${gem(64, 65, .55)}${shine('M24 50Q26 32 42 30')}`,
  nav_shop: `<path d="M42 40V28Q64 4 86 28V40" fill="none" stroke="${INK}" stroke-width="20"/><path d="M42 40V28Q64 4 86 28V40" fill="none" stroke="url(#steel)" stroke-width="11"/><path d="M22 40H106L114 114H14Z" fill="url(#copper)"/>
<path d="M30 52H98V102H30Z" fill="url(#teal)"/>${gem(64, 76, 1.05)}${shine('M28 46H56')}`,
  nav_album: `<path d="M18 22H62Q64 22 64 26V114Q64 108 56 108H18Z" fill="url(#teal)"/><path d="M110 22H66Q64 22 64 26V114Q64 108 72 108H110Z" fill="url(#teal)"/>
<path d="M24 28H58V100H24ZM70 28H104V100H70Z" fill="#EAF2E8"/><path d="M30 44H52M30 56H52M30 68H48" stroke="#9DB3B5" stroke-width="5" stroke-linecap="round"/>${gem(87, 64, .9)}`,
};
shapes.key = `<circle cx="40" cy="44" r="26" fill="url(#copper)"/><circle cx="40" cy="44" r="11" fill="${INK}"/><path d="M58 60L104 106" stroke="${INK}" stroke-width="22" stroke-linecap="round"/><path d="M58 60L104 106" stroke="url(#steel)" stroke-width="12" stroke-linecap="round"/><path d="M84 86L96 74M96 98L108 86" stroke="${INK}" stroke-width="16" stroke-linecap="round"/><path d="M84 86L96 74M96 98L108 86" stroke="url(#steel)" stroke-width="7" stroke-linecap="round"/>${shine("M24 34Q30 22 44 20")}`;
shapes.crystal = gem(64, 62, 3.2);
shapes.coin = coin;
shapes.motor = shapes.cadence;

function wrap(body) {
  // 1ª passada: silhueta externa grossa (efeito adesivo); 2ª: desenho com contorno interno.
  return `<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 128 128">${defs}
<g filter="url(#drop)"><g transform="translate(8 6) scale(.875)" stroke-linejoin="round" stroke-linecap="round">
<g stroke="${INK}" stroke-width="13">${body}</g>
<g stroke="${INK}" stroke-width="5">${body}</g>
</g></g></svg>\n`;
}

function main() {
  fs.mkdirSync(source, { recursive: true });
  fs.mkdirSync(output, { recursive: true });
  const work = fs.mkdtempSync(path.join(os.tmpdir(), 'dw-icons-'));
  const names = Object.keys(shapes);
  for (const name of names) {
    const svg = wrap(shapes[name]);
    fs.writeFileSync(path.join(source, name + '.svg'), svg);
    // Página mínima para o Edge capturar com fundo transparente, 1:1.
    const html = path.join(work, name + '.html');
    fs.writeFileSync(html, `<!doctype html><html><body style="margin:0;background:transparent">${svg}</body></html>`);
    const png = path.join(output, name + '.png');
    execFileSync(edge, ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
      '--default-background-color=00000000', '--window-size=256,256', `--screenshot=${png}`, 'file:///' + html.replace(/\\/g, '/')],
      { stdio: 'ignore' });
  }
  fs.rmSync(work, { recursive: true, force: true });
  console.log(`Exported ${names.length} Toy-Mecha v3 icons.`);
}
main();
