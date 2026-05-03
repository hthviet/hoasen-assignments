#!/usr/bin/env python3
"""
Generate Use Case diagram and summary images for NHEO project
"""

from PIL import Image, ImageDraw, ImageFont
import textwrap
import os

def create_usecase_summary_image():
    """Create a summary image of all use cases"""
    
    # Image dimensions
    width = 1200
    height = 1400
    
    # Create image with white background
    img = Image.new('RGB', (width, height), color='white')
    draw = ImageDraw.Draw(img)
    
    # Try to use a system font, fallback to default
    try:
        title_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 48)
        heading_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 32)
        section_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 24)
        normal_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 18)
    except:
        title_font = heading_font = section_font = normal_font = ImageFont.load_default()
    
    # Colors
    primary_color = (102, 126, 234)  # #667eea
    secondary_color = (118, 75, 162)  # #764ba2
    text_color = (51, 51, 51)  # #333
    light_bg = (245, 245, 245)  # #f5f5f5
    
    y_pos = 40
    line_height = 30
    
    # Title
    draw.text((width//2 - 200, y_pos), "NHEO E-Commerce Platform", fill=primary_color, font=title_font)
    y_pos += 60
    
    draw.text((width//2 - 150, y_pos), "Use Cases Overview", fill=secondary_color, font=heading_font)
    y_pos += 60
    
    # Stats
    stats_text = "21 Use Cases  •  3 Actor Types  •  6 Functional Areas"
    draw.text((width//2 - 250, y_pos), stats_text, fill=text_color, font=section_font)
    y_pos += 80
    
    # Draw horizontal line
    draw.line([(40, y_pos), (width-40, y_pos)], fill=primary_color, width=2)
    y_pos += 40
    
    # Use Case Categories
    categories = [
        ("🔐 Authentication & Account (4 UC)", [
            "UC-1.1: Register New Account",
            "UC-1.2: Login to Account",
            "UC-1.3: View Profile",
            "UC-1.4: Logout from Account"
        ]),
        ("📦 Product Catalog (4 UC)", [
            "UC-2.1: Browse Product Catalog",
            "UC-2.2: Search Products",
            "UC-2.3: Filter & Sort Products",
            "UC-2.4: View Product Details"
        ]),
        ("🛒 Shopping Cart (4 UC)", [
            "UC-3.1: Add Product to Cart",
            "UC-3.2: View Shopping Cart",
            "UC-3.3: Modify Cart Quantity",
            "UC-3.4: Remove from Cart"
        ]),
        ("💳 Checkout & Orders (4 UC)", [
            "UC-4.1: Initiate Checkout",
            "UC-4.2: Place Order",
            "UC-5.1: View Order History",
            "UC-5.2: View Order Details"
        ]),
        ("⚙️ Admin Management (5 UC)", [
            "UC-6.1: Access Admin Dashboard",
            "UC-6.2: Manage Products (CRUD)",
            "UC-6.3: Track Orders",
            "UC-6.4: Update Order Status",
            "UC-6.5: View Revenue Reports"
        ])
    ]
    
    for category, use_cases in categories:
        # Category header
        draw.text((50, y_pos), category, fill=secondary_color, font=section_font)
        y_pos += 35
        
        # Use cases
        for uc in use_cases:
            draw.text((70, y_pos), "• " + uc, fill=text_color, font=normal_font)
            y_pos += 28
        
        y_pos += 15
    
    # Footer
    footer_y = height - 60
    draw.line([(40, footer_y), (width-40, footer_y)], fill=primary_color, width=1)
    footer_text = "NHEO Laptop E-Commerce Platform | Use Cases Document | May 3, 2026"
    draw.text((width//2 - 350, footer_y + 15), footer_text, fill=text_color, font=normal_font)
    
    # Save image
    output_path = '/Users/viethuynh/Library/Mobile Documents/com~apple~CloudDocs/NHEO/USECASES/UseCase_Summary.png'
    img.save(output_path)
    print(f"✅ Use case summary image created: {output_path}")
    return output_path

def create_actors_image():
    """Create an image showing the three system actors"""
    
    width = 1200
    height = 600
    
    img = Image.new('RGB', (width, height), color='white')
    draw = ImageDraw.Draw(img)
    
    try:
        title_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 40)
        heading_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 28)
        normal_font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", 18)
    except:
        title_font = heading_font = normal_font = ImageFont.load_default()
    
    primary_color = (102, 126, 234)
    secondary_color = (118, 75, 162)
    text_color = (51, 51, 51)
    
    # Title
    draw.text((width//2 - 150, 30), "System Actors", fill=primary_color, font=title_font)
    
    # Actor boxes
    actors = [
        {
            "title": "👤 Guest User",
            "desc": "Browse products\nAdd to cart\nNo registration",
            "x": 80
        },
        {
            "title": "👤 Registered Customer",
            "desc": "Place orders\nTrack history\nManage account",
            "x": 440
        },
        {
            "title": "👨‍💼 Administrator",
            "desc": "Manage catalog\nTrack orders\nView reports",
            "x": 880
        }
    ]
    
    for actor in actors:
        x = actor["x"]
        # Draw box
        draw.rectangle([(x, 120), (x+280, 520)], outline=primary_color, width=3)
        
        # Title
        draw.text((x+20, 140), actor["title"], fill=secondary_color, font=heading_font)
        
        # Description
        y_text = 220
        for line in actor["desc"].split("\n"):
            draw.text((x+20, y_text), "• " + line, fill=text_color, font=normal_font)
            y_text += 45
    
    output_path = '/Users/viethuynh/Library/Mobile Documents/com~apple~CloudDocs/NHEO/USECASES/System_Actors.png'
    img.save(output_path)
    print(f"✅ System actors image created: {output_path}")
    return output_path

if __name__ == "__main__":
    print("Generating use case images...")
    create_usecase_summary_image()
    create_actors_image()
    print("\n✅ All images generated successfully in USECASES folder!")
