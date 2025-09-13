#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

// シンプルなSVG to PNG converter using HTML Canvas simulation
function generateIcon(size) {
    // SVGベースのアイコンデザイン
    const svg = `<svg width="${size}" height="${size}" xmlns="http://www.w3.org/2000/svg">
  <!-- 背景（角丸矩形） -->
  <rect x="0" y="0" width="${size}" height="${size}" rx="${size/8}" ry="${size/8}" fill="#0d6efd"/>
  
  <!-- メモ用紙の背景 -->
  <rect x="${size/8}" y="${size*0.15625}" width="${size*0.75}" height="${size*0.78125}" rx="${size/32}" ry="${size/32}" fill="#ffffff"/>
  
  <!-- メモの罫線 -->
  <line x1="${size*0.1875}" y1="${size*0.3125}" x2="${size*0.8125}" y2="${size*0.3125}" stroke="#e0e0e0" stroke-width="${Math.max(1, size/171)}"/>
  <line x1="${size*0.1875}" y1="${size*0.4296875}" x2="${size*0.8125}" y2="${size*0.4296875}" stroke="#e0e0e0" stroke-width="${Math.max(1, size/171)}"/>
  <line x1="${size*0.1875}" y1="${size*0.546875}" x2="${size*0.8125}" y2="${size*0.546875}" stroke="#e0e0e0" stroke-width="${Math.max(1, size/171)}"/>
  <line x1="${size*0.1875}" y1="${size*0.6640625}" x2="${size*0.8125}" y2="${size*0.6640625}" stroke="#e0e0e0" stroke-width="${Math.max(1, size/171)}"/>
  <line x1="${size*0.1875}" y1="${size*0.78125}" x2="${size*0.8125}" y2="${size*0.78125}" stroke="#e0e0e0" stroke-width="${Math.max(1, size/171)}"/>
  
  <!-- ペンアイコン -->
  <path d="M${size*0.7421875} ${size*0.234375} L${size*0.765625} ${size*0.2109375} L${size*0.7890625} ${size*0.234375} L${size*0.765625} ${size*0.2578125} Z" fill="#0d6efd"/>
  <path d="M${size*0.71875} ${size*0.2578125} L${size*0.7421875} ${size*0.234375} L${size*0.765625} ${size*0.2578125} L${size*0.7421875} ${size*0.28125} Z" fill="#6c757d"/>
</svg>`;

    return svg;
}

// 各サイズのアイコンを生成
const sizes = [72, 96, 128, 144, 152, 192, 384, 512];

console.log('Generating PWA icons...');

sizes.forEach(size => {
    const svg = generateIcon(size);
    const filename = `icon-${size}x${size}.svg`;
    fs.writeFileSync(filename, svg);
    console.log(`Generated: ${filename}`);
});

console.log('\nSVG icons generated successfully.');
console.log('Note: For production use, convert these SVG files to PNG format.');
console.log('You can use the generate-icons.html file in a browser to create PNG files.');