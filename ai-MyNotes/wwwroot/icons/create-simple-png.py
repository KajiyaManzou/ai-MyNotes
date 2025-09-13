#!/usr/bin/env python3
"""
Simple PNG icon creator without external dependencies
Creates solid color PNG files with correct dimensions
"""
import struct
import zlib
import os

def create_png(width, height, color=(13, 110, 253)):
    """Create a simple PNG image with solid color"""
    
    def write_png_chunk(chunk_type, data):
        """Write a PNG chunk"""
        chunk_data = chunk_type + data
        crc = zlib.crc32(chunk_data) & 0xffffffff
        return struct.pack('>I', len(data)) + chunk_data + struct.pack('>I', crc)
    
    # PNG signature
    png_signature = b'\x89PNG\r\n\x1a\n'
    
    # IHDR chunk
    ihdr_data = struct.pack('>2I5B', width, height, 8, 2, 0, 0, 0)
    ihdr_chunk = write_png_chunk(b'IHDR', ihdr_data)
    
    # Create image data (RGB)
    r, g, b = color
    row = bytes([r, g, b] * width)
    
    # Add filter byte (0) to each row
    image_data = b''
    for _ in range(height):
        image_data += b'\x00' + row
    
    # Compress image data
    compressed_data = zlib.compress(image_data)
    idat_chunk = write_png_chunk(b'IDAT', compressed_data)
    
    # IEND chunk
    iend_chunk = write_png_chunk(b'IEND', b'')
    
    # Combine all chunks
    png_data = png_signature + ihdr_chunk + idat_chunk + iend_chunk
    
    return png_data

def main():
    """Generate PNG icons for PWA"""
    sizes = [72, 96, 128, 144, 152, 192, 384, 512]
    
    # Bootstrap primary color #0d6efd
    bg_color = (13, 110, 253)
    
    print("Creating simple PNG icons...")
    
    for size in sizes:
        filename = f"icon-{size}x{size}.png"
        png_data = create_png(size, size, bg_color)
        
        with open(filename, 'wb') as f:
            f.write(png_data)
        
        print(f"Created: {filename} ({len(png_data)} bytes)")
    
    print("\nPNG icons created successfully!")
    print("These are simple solid color icons. For production,")
    print("consider using proper design tools to create detailed icons.")

if __name__ == "__main__":
    main()