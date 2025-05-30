shader_type canvas_item;

uniform sampler2D viewport_texture : hint_default_transparent;
uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius = 12.0;
uniform float glow_strength = 2.0;

void fragment() {
    vec2 tex_size = vec2(textureSize(viewport_texture, 0));
    vec2 px = 1.0 / tex_size;

    float sigma = glow_radius * 0.6;
    float twoSigmaSq = 2.0 * sigma * sigma;

    float total_weight = 0.0;
    float alpha_sum = 0.0;

    int kernel_radius = int(ceil(glow_radius));
    for (int x = -kernel_radius; x <= kernel_radius; x++) {
        for (int y = -kernel_radius; y <= kernel_radius; y++) {
            vec2 offset = vec2(float(x), float(y));
            float weight = exp(-(offset.x * offset.x + offset.y * offset.y) / twoSigmaSq);
            vec2 sample_uv = UV + offset * px;
            alpha_sum += texture(viewport_texture, sample_uv).a * weight;
            total_weight += weight;
        }
    }

    float final_alpha = (alpha_sum / total_weight) * glow_strength;
    COLOR = vec4(glow_color.rgb, final_alpha * glow_color.a);
}