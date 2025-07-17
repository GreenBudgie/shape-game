shader_type canvas_item;

uniform float stretch_amount : hint_range(0.0, 1.0) = 0.0;
uniform vec2 stretch_direction = vec2(1.0, 0.0);

void fragment() {
	vec2 stretch_origin = vec2(0.5, 0.5);
	
    // Normalize direction
    vec2 dir = normalize(stretch_direction);
    vec2 perp_dir = vec2(-dir.y, dir.x); // Perpendicular direction
    
    // Get offset from origin
    vec2 offset = UV - stretch_origin;
    
    // Split offset into components along and perpendicular to stretch direction
    float along = dot(offset, dir);
    float across = dot(offset, perp_dir);
    
    // Apply stretch only in the perpendicular direction
    across *= stretch_amount + 1.0;
    
    // Reconstruct UV
    vec2 stretched_uv = stretch_origin + dir * along + perp_dir * across;
    
    // Prevent artifacts
    stretched_uv = clamp(stretched_uv, vec2(0.0), vec2(1.0));
    
    COLOR = texture(TEXTURE, stretched_uv);
}