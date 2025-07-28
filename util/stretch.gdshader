shader_type canvas_item;

uniform float stretch_amount : hint_range(0.0, 2.0) = 0.0;
uniform vec2 stretch_direction = vec2(1.0, 0.0);

void fragment() {
    // Normalize the stretch direction
    vec2 normalized_direction = normalize(stretch_direction);

    // Calculate the center of the texture
    vec2 center = vec2(0.5, 0.5);

    // Get the current UV coordinate relative to center
    vec2 uv_centered = UV - center;

    // Create a transformation matrix for stretching
    // We stretch along the normalized direction
    vec2 perpendicular = vec2(-normalized_direction.y, normalized_direction.x);

    // Project the UV onto the stretch direction and perpendicular
    float projection_stretch = dot(uv_centered, normalized_direction);
    float projection_perp = dot(uv_centered, perpendicular);

    // Apply stretching only along the stretch direction
    projection_stretch *= stretch_amount + 1.0;

    // Reconstruct the UV coordinates
    vec2 stretched_uv = center + projection_stretch * normalized_direction + projection_perp * perpendicular;

    // Sample the texture at the stretched coordinates
    // Check if we're outside the original texture bounds
    if (stretched_uv.x < 0.0 || stretched_uv.x > 1.0 || stretched_uv.y < 0.0 || stretched_uv.y > 1.0) {
        COLOR = vec4(0.0, 0.0, 0.0, 0.0); // Transparent outside bounds
    } else {
        COLOR = texture(TEXTURE, stretched_uv);
    }
}