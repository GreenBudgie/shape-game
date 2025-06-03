shader_type canvas_item;

uniform sampler2D viewport_texture : hint_default_transparent;
uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;
uniform bool cull_occluded = true;

// Fast inverse square root approximation (famous Quake trick)
float fast_inv_sqrt(float x) {
    float xhalf = 0.5 * x;
    int i = floatBitsToInt(x);
    i = 0x5f3759df - (i >> 1);
    x = intBitsToFloat(i);
    return x * (1.5 - (xhalf * x * x));
}

// Fast exp approximation (2nd order Taylor)
float fast_exp(float x) {
    x = 1.0 + x * (1.0 + x * 0.5);
    return x * x;
}

void fragment() {
    // Early discard
    if (cull_occluded && texture(TEXTURE, UV).a > 0.99) discard;

    // Fast quality calculation using inverse sqrt
    float inv_sqrt_r = fast_inv_sqrt(glow_radius);
    int quality = clamp(int(1.8 / inv_sqrt_r), 4, 32);
    int kernel_radius = int(ceil(glow_radius));
    int step = max(1, kernel_radius / quality);

    // Pre-calculations
    vec2 px = 1.0 / vec2(textureSize(viewport_texture, 0));
    float sigma_sq = glow_radius * glow_radius * 0.36;
    float radius_sq = float(kernel_radius * kernel_radius);
    float inv_two_sigma_sq = 1.0 / (2.0 * sigma_sq);

    // Blur with approximations
    float alpha_sum = 0.0;
    float total_weight = 0.0;

    for (int x = -kernel_radius; x <= kernel_radius; x += step) {
        for (int y = -kernel_radius; y <= kernel_radius; y += step) {
            vec2 offset = vec2(float(x), float(y));
            float dist_sq = dot(offset, offset);
            if (dist_sq > radius_sq) continue;

            // Fast exp approximation
            float weight = fast_exp(-dist_sq * inv_two_sigma_sq);
            if (weight < 0.005) continue;

            alpha_sum += texture(viewport_texture, UV + offset * px).a * weight;
            total_weight += weight;
        }
    }

    COLOR = vec4(glow_color.rgb, (alpha_sum / total_weight) * glow_strength * glow_color.a);
}