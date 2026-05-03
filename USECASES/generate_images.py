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
RELATIONSHIPS = {
    "include": {
        "UC-2.2": ["UC-2.1"],
        "UC-2.3": ["UC-2.1"],
        "UC-4.1": ["UC-3.2"],
        "UC-4.2": ["UC-4.1"],
    },
    "extend": {
        "UC-2.4": ["UC-2.1"],
        "UC-3.1": ["UC-2.4"],
        "UC-3.3": ["UC-3.2"],
        "UC-3.4": ["UC-3.2"],
        "UC-5.2": ["UC-5.1"],
        "UC-6.4": ["UC-6.3"],
    },
}


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


def parse_actor_names(actor_lines):
    combined = " ".join(actor_lines).strip()
    if not combined:
        return ["User"]

    actor_names = []
    lowered = combined.lower()
    if "guest" in lowered:
        actor_names.append("Guest User")
    if "customer" in lowered or "registered user" in lowered:
        actor_names.append("Customer")
    if "administrator" in lowered or "admin" in lowered:
        actor_names.append("Administrator")

    if not actor_names:
        actor_names.append(combined)

    deduped = []
    for actor in actor_names:
        if actor not in deduped:
            deduped.append(actor)
    return deduped


def draw_centered_text(draw, box, text, font, fill, max_width_chars):
    lines = wrap_lines(text, max_width_chars)
    line_height = font.size + 6
    total_height = len(lines) * line_height
    x0, y0, x1, y1 = box
    y = y0 + ((y1 - y0 - total_height) / 2)
    for line in lines:
        bbox = draw.textbbox((0, 0), line, font=font)
        line_width = bbox[2] - bbox[0]
        x = x0 + ((x1 - x0 - line_width) / 2)
        draw.text((x, y), line, fill=fill, font=font)
        y += line_height


def draw_actor(draw, center_x, top_y, name, line_color, text_color, font):
    head_radius = 22
    head_center_y = top_y + head_radius
    draw.ellipse(
        [(center_x - head_radius, top_y), (center_x + head_radius, top_y + head_radius * 2)],
        outline=line_color,
        width=3,
    )
    torso_top = top_y + head_radius * 2
    torso_bottom = torso_top + 70
    draw.line([(center_x, torso_top), (center_x, torso_bottom)], fill=line_color, width=3)
    draw.line([(center_x - 36, torso_top + 18), (center_x + 36, torso_top + 18)], fill=line_color, width=3)
    draw.line([(center_x, torso_bottom), (center_x - 28, torso_bottom + 42)], fill=line_color, width=3)
    draw.line([(center_x, torso_bottom), (center_x + 28, torso_bottom + 42)], fill=line_color, width=3)

    name_box = draw.textbbox((0, 0), name, font=font)
    name_width = name_box[2] - name_box[0]
    draw.text((center_x - name_width / 2, torso_bottom + 58), name, fill=text_color, font=font)
    return {
        "hand_x": center_x + 36,
        "hand_y": torso_top + 18,
    }


def draw_dashed_line(draw, start, end, fill, width=3, dash=12, gap=8):
    x1, y1 = start
    x2, y2 = end
    dx = x2 - x1
    dy = y2 - y1
    distance = max((dx * dx + dy * dy) ** 0.5, 1)
    step_x = dx / distance
    step_y = dy / distance
    painted = 0
    while painted < distance:
        segment_end = min(painted + dash, distance)
        sx = x1 + step_x * painted
        sy = y1 + step_y * painted
        ex = x1 + step_x * segment_end
        ey = y1 + step_y * segment_end
        draw.line([(sx, sy), (ex, ey)], fill=fill, width=width)
        painted += dash + gap


def draw_arrow_head(draw, tip, tail, fill):
    tx, ty = tip
    bx, by = tail
    dx = tx - bx
    dy = ty - by
    distance = max((dx * dx + dy * dy) ** 0.5, 1)
    ux = dx / distance
    uy = dy / distance
    left_x = tx - ux * 18 - uy * 8
    left_y = ty - uy * 18 + ux * 8
    right_x = tx - ux * 18 + uy * 8
    right_y = ty - uy * 18 - ux * 8
    draw.polygon([(tx, ty), (left_x, left_y), (right_x, right_y)], fill=fill)


def draw_relation_ellipse(draw, box, title, fill, outline, text_color, font):
    draw.ellipse([(box[0], box[1]), (box[2], box[3])], fill=fill, outline=outline, width=3)
    draw_centered_text(draw, box, title, font, text_color, 18)


def build_inverse_relationships():
    inverse = {"include": {}, "extend": {}}
    for relation_type, mapping in RELATIONSHIPS.items():
        for source, targets in mapping.items():
            for target in targets:
                inverse[relation_type].setdefault(target, []).append(source)
    return inverse


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
    relation_fill = (240, 252, 247)
    relation_outline = (47, 133, 90)

    use_cases = parse_use_cases(MARKDOWN_PATH)
    use_case_lookup = {use_case["code"]: use_case for use_case in use_cases}
    inverse_relationships = build_inverse_relationships()
    generated_files = []

    for use_case in use_cases:
        width = 1600
        height = 980
        margin = 60

        temp_img = Image.new("RGB", (width, height), color=background)
        draw = ImageDraw.Draw(temp_img)

        draw.rounded_rectangle([(24, 24), (width - 24, height - 24)], radius=28, outline=border_color, width=2, fill=background)
        draw.rounded_rectangle([(40, 34), (width - 40, 170)], radius=24, fill=(243, 246, 255), outline=(226, 232, 247), width=2)
        draw.text((margin, 58), use_case["code"], fill=primary_color, font=subtitle_font)
        draw.text((margin, 96), use_case["title"], fill=text_color, font=title_font)
        draw.text((margin, 138), f"Category: {use_case['category']}", fill=secondary_color, font=subtitle_font)

        actor_names = parse_actor_names(use_case["actors"])
        actor_positions = []
        actor_start_x = 160
        actor_gap = 170
        for index, actor_name in enumerate(actor_names[:3]):
            actor_positions.append(
                draw_actor(draw, actor_start_x + index * actor_gap, 310, actor_name, secondary_color, text_color, body_font)
            )

        system_box = (470, 240, 1180, 760)
        draw.rounded_rectangle([(system_box[0], system_box[1]), (system_box[2], system_box[3])], radius=28, outline=primary_color, width=4)
        draw.rounded_rectangle([(system_box[0] + 24, system_box[1] + 18), (system_box[0] + 270, system_box[1] + 66)], radius=14, fill=panel_bg, outline=border_color)
        draw.text((system_box[0] + 42, system_box[1] + 28), "NHEO E-Commerce System", fill=primary_color, font=section_font)

        ellipse_box = (640, 410, 1035, 580)
        draw.ellipse([(ellipse_box[0], ellipse_box[1]), (ellipse_box[2], ellipse_box[3])], outline=secondary_color, width=4, fill=(252, 245, 255))
        ellipse_text = f"{use_case['code']}\n{use_case['title']}"
        draw_centered_text(draw, ellipse_box, ellipse_text, body_font, text_color, 24)

        relation_specs = []
        for relation_type in ("include", "extend"):
            for target_code in RELATIONSHIPS[relation_type].get(use_case["code"], []):
                if target_code in use_case_lookup:
                    relation_specs.append((relation_type, "outgoing", target_code, use_case_lookup[target_code]["title"]))
            for source_code in inverse_relationships[relation_type].get(use_case["code"], []):
                if source_code in use_case_lookup:
                    relation_specs.append((relation_type, "incoming", source_code, use_case_lookup[source_code]["title"]))

        relation_boxes = [
            (700, 270, 1010, 365),
            (700, 625, 1010, 720),
            (500, 615, 760, 710),
            (930, 615, 1190, 710),
        ]

        for index, (relation_type, direction, related_code, related_title) in enumerate(relation_specs[:4]):
            box = relation_boxes[index]
            relation_label = f"{related_code}\n{related_title}"
            draw_relation_ellipse(draw, box, relation_label, relation_fill, relation_outline, text_color, small_font)

            main_center = ((ellipse_box[0] + ellipse_box[2]) / 2, (ellipse_box[1] + ellipse_box[3]) / 2)
            related_center = ((box[0] + box[2]) / 2, (box[1] + box[3]) / 2)
            if direction == "outgoing":
                start = related_center
                end = main_center
            else:
                start = main_center
                end = related_center
            draw_dashed_line(draw, start, end, relation_outline, width=3)
            draw_arrow_head(draw, end, start, relation_outline)

            mid_x = (start[0] + end[0]) / 2
            mid_y = (start[1] + end[1]) / 2
            label = f"<<{relation_type}>>"
            label_box = draw.textbbox((0, 0), label, font=small_font)
            label_width = label_box[2] - label_box[0]
            draw.rounded_rectangle(
                [(mid_x - label_width / 2 - 10, mid_y - 18), (mid_x + label_width / 2 + 10, mid_y + 12)],
                radius=10,
                fill=background,
                outline=border_color,
            )
            draw.text((mid_x - label_width / 2, mid_y - 14), label, fill=relation_outline, font=small_font)

        for position in actor_positions:
            draw.line([(position["hand_x"], position["hand_y"]), (ellipse_box[0], (ellipse_box[1] + ellipse_box[3]) / 2)], fill=secondary_color, width=3)

        note_x = 1220
        note_width = 300
        sections = [
            ("Preconditions", use_case["preconditions"], "- "),
            ("Main Flow", use_case["main_flow"][:4], None),
            ("Alternative", use_case["alternative_flows"], "- "),
            ("Postconditions", use_case["postconditions"], "- "),
        ]
        note_y = 250
        for title, items, bullet in sections:
            if not items:
                continue
            box_height = 120 if title == "Main Flow" else 110
            draw.rounded_rectangle(
                [(note_x, note_y), (note_x + note_width, note_y + box_height)],
                radius=18,
                fill=panel_bg,
                outline=border_color,
                width=2,
            )
            draw.text((note_x + 18, note_y + 14), title, fill=secondary_color, font=section_font)
            content_y = note_y + 48
            content_items = items
            if title == "Main Flow" and len(use_case["main_flow"]) > 4:
                content_items = items + [f"... {len(use_case['main_flow']) - 4} more step(s)"]
            content_y = render_list_block(draw, note_x + 18, content_y, content_items, small_font, text_color, 32, bullet=bullet, spacing=6)
            note_y += box_height + 20

        legend_box = (530, 795, 1135, 900)
        draw.rounded_rectangle([(legend_box[0], legend_box[1]), (legend_box[2], legend_box[3])], radius=18, fill=(249, 250, 252), outline=border_color, width=2)
        draw.text((legend_box[0] + 22, legend_box[1] + 16), "Diagram Meaning", fill=secondary_color, font=section_font)
        legend_text = "Actor interacts with the highlighted use case. Dashed arrows show related use cases through <<include>> or <<extend>> relationships when applicable."
        legend_lines = wrap_lines(legend_text, 72)
        y = legend_box[1] + 52
        for line in legend_lines:
            draw.text((legend_box[0] + 22, y), line, fill=text_color, font=small_font)
            y += small_font.size + 6

        footer_y = height - 58
        draw.line([(margin, footer_y), (width - margin, footer_y)], fill=border_color, width=2)
        draw.text((margin, footer_y + 14), "Generated from USE_CASES.md", fill=muted_color, font=small_font)

        final_img = temp_img

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
