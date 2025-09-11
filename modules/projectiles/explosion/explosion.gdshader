shader_type canvas_item;

uniform vec2 center = vec2(0.5, 0.5);
uniform float radius: hint_range(0.0, 1.0, 0.001) = 0.0;

uniform float width: hint_range(0.0, 0.1, 0.0001) = 0.06;
uniform float feather: hint_range(0.0, 1.0, 0.001) = 0.2;

uniform float intensity: hint_range(0.0, 1.0, 0.01) = 1.0;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    vec2 uv = SCREEN_UV;

    // aspect = width / height
    float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
    vec2 scale = vec2(aspect, 1.0);

    // work in aspect-corrected space for a circular mask
    vec2 uv_scaled = (uv - 0.5) * scale + 0.5;
    vec2 center_scaled = (center - 0.5) * scale + 0.5;

    vec2 to_center = uv_scaled - center_scaled;
    float dist = length(to_center);

    // ring mask with soft inner+outer edges
    float ring = (1.0 - smoothstep(radius - feather, radius, dist))
	 * smoothstep(max(0.0, radius - width - feather), radius - width, dist);

    // safe normalize
    vec2 dir = dist > 0.0 ? (to_center / dist) : vec2(0.0);

    // build displacement in SCALED space, then convert to SCREEN_UV space
    // include ring * intensity so displacement also fades out
    // bumped for more noticeable base effect
    float strength = 0.3;
    vec2 offset_scaled = dir * strength * (ring * intensity);
    vec2 offset_uv = offset_scaled / scale; // convert back to unscaled UVs

    // displaced sampling coordinates
    vec2 displaced_uv = uv - offset_uv;

    // hardcoded aberration values
    float aberration = 0.3; // increased from 0.2 for more intensity; scales with radius below

    // chromatic aberration offsets (same UV space!)
    // scale aberration with radius for more intense effects on big explosions
    // (radius grows during tween, so effect ramps up naturally)
    float scaled_aberration = aberration * (1.0 + radius * 2.0); // for radius=0: x1; radius=0.5: x2; radius=1: x3
    vec2 ab = offset_uv * scaled_aberration;

    // make asymmetric for more colorful/noticeable fringing (red shifts more outward)
    vec2 ab_r = ab * 1.5;
    vec2 ab_b = ab * 0.75;

    vec4 col_r = texture(SCREEN_TEXTURE, displaced_uv + ab_r);
    vec4 col_b = texture(SCREEN_TEXTURE, displaced_uv - ab_b);
    vec4 col_o = texture(SCREEN_TEXTURE, displaced_uv);

    vec4 effect_col = vec4(col_r.r, col_o.g, col_b.b, 1.0);

    // blend only where ring is present; fade by intensity
    float mix_a = ring * intensity;
    COLOR = mix(col_o, effect_col, mix_a);
}