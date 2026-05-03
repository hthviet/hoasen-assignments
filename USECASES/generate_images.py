#!/usr/bin/env python3
"""Generate use case images for the NHEO project."""

from PIL import Image, ImageDraw, ImageFont
import os
import re
import textwrap

ROOT_DIR = "/Users/viethuynh/Library/Mobile Documents/com~apple~CloudDocs/NHEO/USECASES"
MARKDOWN_PATH = os.path.join(ROOT_DIR, "USE_CASES.md")
INDIVIDUAL_DIR = os.path.join(ROOT_DIR, "individual_usecases")
FONT_PATH = "/System/Library/Fonts/Helvetica.ttc"


def load_font(size):
    try:
        return ImageFont.truetype(FONT_PATH, size)
    except Exception:
        return ImageFont.load_default()


def wrap_lines(text, width):
    if not text:
        return []
    pieces = []
    for raw_line in text.split("\n"):
        stripped = raw_line.strip()
        if not stripped:
            pieces.append("")
            continue
        pieces.extend(textwrap.wrap(stripped, width=width, break_long_words=False, break_on_hyphens=False))
    return pieces


def sanitize_filename(value):
    safe = re.sub(r"[^A-Za-z0-9._-]+", "_", value.strip())
    return safe.strip("_")


def parse_use_cases(markdown_path):
    with open(markdown_path, "r", encoding="utf-8") as handle:
        lines = handle.readlines()

    category = "General"
    use_cases = []
    current = None
    current_section = None

    section_map = {
        "**Actors:**": "actors",
        "**Preconditions:**": "preconditions",
        "**Main Flow:**": "main_flow",
        "**Alternative Flows:**": "alternative_flows",
        "**Postconditions:**": "postconditions",
    }

    for raw_line in lines:
        line = raw_line.rstrip("\n")
        stripped = line.strip()

        if stripped.startswith("### ") and not stripped.startswith("#### "):
            category = stripped[4:].strip()
            continue

        if stripped.startswith("#### UC-"):
            if current:
                use_cases.append(current)
            code, title = stripped[5:].split(":", 1)
            current = {
                "code": code.strip(),
                "title": title.strip(),
                "category": category,
                "actors": [],
                "preconditions": [],
                "main_flow": [],
                "alternative_flows": [],
                "postconditions": [],
            }
            current_section = None
            continue

        if not current:
            continue

        if stripped == "---":
            continue

        matched_section = False
        for prefix, key in section_map.items():
            if stripped.startswith(prefix):
                current_section = key
                inline = stripped[len(prefix):].strip()
                if inline:
                    current[key].append(inline)
                matched_section = True
                break
        if matched_section:
            continue

        if not stripped:
            continue

        if current_section:
            current[current_section].append(stripped)

    if current:
        use_cases.append(current)

    return use_cases


def render_list_block(draw, x, y, items, font, fill, wrap_width, bullet=None, spacing=10):
    for item in items:
        content = item
        prefix = ""
        if bullet and item.startswith(("- ", "* ")):
            content = item[2:].strip()
            prefix = bullet
        elif re.match(r"^\d+\.\s+", item):
            number, content = item.split(".", 1)
            prefix = f"{number.strip()}. "
            content = content.strip()

        wrapped = wrap_lines(content, wrap_width)
        if not wrapped:
            continue

        first_prefix = prefix
        continuation_indent = " " * len(first_prefix)
        for index, line in enumerate(wrapped):
            text = f"{first_prefix if index == 0 else continuation_indent}{line}"
            draw.text((x, y), text, fill=fill, font=font)
            y += font.size + spacing
        y += 4
    return y


def create_individual_use_case_images():
    os.makedirs(INDIVIDUAL_DIR, exist_ok=True)

    title_font = load_font(36)
    subtitle_font = load_font(22)
    section_font = load_font(24)
    body_font = load_font(18)
    small_font = load_font(16)

    primary_color = (102, 126, 234)
    secondary_color = (118, 75, 162)
    text_color = (51, 51, 51)
    muted_color = (99, 99, 99)
    border_color = (220, 224, 235)
    background = (255, 255, 255)
    panel_bg = (247, 249, 252)

    use_cases = parse_use_cases(MARKDOWN_PATH)
    generated_files = []

    for use_case in use_cases:
        width = 1400
        margin = 60
        y = margin

        temp_img = Image.new("RGB", (width, 4000), color=background)
        draw = ImageDraw.Draw(temp_img)

        draw.rounded_rectangle([(30, 30), (width - 30, 220)], radius=24, fill=(243, 246, 255))
        draw.text((margin, y), use_case["code"], fill=primary_color, font=subtitle_font)
        y += 40
        draw.text((margin, y), use_case["title"], fill=text_color, font=title_font)
        y += 56
        draw.text((margin, y), f"Category: {use_case['category']}", fill=secondary_color, font=subtitle_font)
        y += 56

        sections = [
            ("Actors", use_case["actors"], None),
            ("Preconditions", use_case["preconditions"], "- "),
            ("Main Flow", use_case["main_flow"], None),
            ("Alternative Flows", use_case["alternative_flows"], "- "),
            ("Postconditions", use_case["postconditions"], "- "),
        ]

        for title, items, bullet in sections:
            if not items:
                continue
            y += 10
            draw.rounded_rectangle([(margin - 16, y - 8), (width - margin, y + 38)], radius=12, fill=panel_bg, outline=border_color)
            draw.text((margin, y), title, fill=secondary_color, font=section_font)
            y += 52
            wrap_width = 90 if title != "Actors" else 96
            y = render_list_block(draw, margin + 8, y, items, body_font, text_color, wrap_width, bullet=bullet)

        footer_y = y + 10
        draw.line([(margin, footer_y), (width - margin, footer_y)], fill=border_color, width=2)
        footer_text = "NHEO Laptop E-Commerce Platform"
        draw.text((margin, footer_y + 14), footer_text, fill=muted_color, font=small_font)
        y = footer_y + 44

        final_height = max(y + margin, 500)
        final_img = temp_img.crop((0, 0, width, final_height))

        file_name = f"{use_case['code']}_{sanitize_filename(use_case['title'])}.png"
        output_path = os.path.join(INDIVIDUAL_DIR, file_name)
        final_img.save(output_path)
        generated_files.append(output_path)
        print(f"✅ Generated {output_path}")

    return generated_files

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
        title_font = load_font(48)
        heading_font = load_font(32)
        section_font = load_font(24)
        normal_font = load_font(18)
    except Exception:
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
        title_font = load_font(40)
        heading_font = load_font(28)
        normal_font = load_font(18)
    except Exception:
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
    create_individual_use_case_images()
    print("\n✅ All images generated successfully in USECASES folder!")
