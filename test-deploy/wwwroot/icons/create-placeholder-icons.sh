#!/bin/bash

# プレースホルダーアイコン作成スクリプト
# 各サイズのプレースホルダーPNGファイルを作成

sizes=(72 96 128 144 152 192 384 512)

# Base64エンコードされた1x1透明PNG
base64_png="iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="

for size in "${sizes[@]}"; do
    filename="icon-${size}x${size}.png"
    echo "$base64_png" | base64 -d > "$filename"
    echo "Created placeholder: $filename"
done

echo "プレースホルダーアイコンの作成が完了しました。"
echo "実際のアイコンは generate-icons.html を使用して作成してください。"