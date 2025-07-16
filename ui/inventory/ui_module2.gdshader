shader_type canvas_item;

// Uniforms to control the stretching
uniform float stretch_amount : hint_range(0.0, 5.0) = 0.0; // How much to stretch (0 = no stretch)
uniform vec2 stretch_direction = vec2(1.0, 0.0); // Direction to stretch in (normalized)

void fragment() {
    // Normalize the direction vector just in case it wasn't normalized
    vec2 dir = normalize(stretch_direction);
    vec2 stretch_origin = vec2(0.5, 0.5);
	
    // Calculate the offset from the origin
    vec2 offset = UV - stretch_origin;
    
    // Project the offset onto the stretch direction to get how much we should stretch
    float projection = dot(offset, dir);
    
    // Apply the stretch
    vec2 stretched_uv = stretch_origin + offset + dir * projection * stretch_amount;
    
    // Sample the texture with the modified UV coordinates
    COLOR = texture(TEXTURE, stretched_uv);
}