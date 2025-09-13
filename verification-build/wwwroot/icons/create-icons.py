#!/usr/bin/env python3
"""
PWAアイコン生成スクリプト
シンプルなメモアプリアイコンを各種サイズで生成
"""
from PIL import Image, ImageDraw, ImageFont
import os

def create_icon(size, filename):
    """指定サイズのアイコンを作成"""
    # 背景色（Bootstrap primary色）
    bg_color = (13, 110, 253)  # #0d6efd
    # テキスト色
    text_color = (255, 255, 255)
    
    # 画像作成
    img = Image.new('RGB', (size, size), bg_color)
    draw = ImageDraw.Draw(img)
    
    # 角丸効果（近似）
    corner_radius = size // 8
    
    # 背景を角丸にする（四隅を切り取り）
    mask = Image.new('L', (size, size), 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.rounded_rectangle(
        [(0, 0), (size-1, size-1)], 
        corner_radius, 
        fill=255
    )
    
    # 角丸背景を適用
    background = Image.new('RGB', (size, size), bg_color)
    img = Image.composite(img, Image.new('RGB', (size, size), (255, 255, 255, 0)), mask)
    
    # メモアイコンのシンプルなデザイン
    margin = size // 8
    note_width = size - (margin * 2)
    note_height = int(note_width * 1.2)
    
    # メモの位置を中央に調整
    note_x = margin
    note_y = (size - note_height) // 2
    
    # メモ背景（白）
    note_bg = (255, 255, 255)
    draw.rounded_rectangle(
        [(note_x, note_y), (note_x + note_width, note_y + note_height)],
        corner_radius // 2,
        fill=note_bg
    )
    
    # メモの線（薄いグレー）
    line_color = (200, 200, 200)
    line_spacing = note_height // 6
    line_margin = note_width // 8
    
    for i in range(1, 5):  # 4本の線
        y = note_y + (line_spacing * i)
        if y < note_y + note_height - line_spacing // 2:
            draw.line(
                [(note_x + line_margin, y), (note_x + note_width - line_margin, y)],
                fill=line_color,
                width=max(1, size // 128)
            )
    
    # ファイルを保存
    img.save(filename, 'PNG', quality=100)
    print(f"Created: {filename} ({size}x{size})")

def main():
    """各種サイズのアイコンを生成"""
    sizes = [72, 96, 128, 144, 152, 192, 384, 512]
    
    for size in sizes:
        filename = f"icon-{size}x{size}.png"
        create_icon(size, filename)
    
    print("\nPWAアイコンの生成が完了しました。")
    print("生成されたファイル:")
    for size in sizes:
        print(f"  - icon-{size}x{size}.png")

if __name__ == "__main__":
    main()