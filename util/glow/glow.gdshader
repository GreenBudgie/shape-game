shader_type canvas_item;
render_mode unshaded;

uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;

varying vec4 vertex_color;

float fast_exp(float x) {
    x = 1.0 + x * (1.0 + x * 0.5);
    return x * x;
}

void vertex() {
    vertex_color = COLOR;
    VERTEX += (UV - 0.5) * 2.0 * glow_radius;
}

void fragment() {
    vec2 expanded_scale = vec2(1.0) + 2.0 * glow_radius * TEXTURE_PIXEL_SIZE;
    vec2 uv = (UV - 0.5) * expanded_scale + 0.5;

    bool inside = uv == clamp(uv, vec2(0.0), vec2(1.0));
    vec4 original = inside ? texture(TEXTURE, uv) : vec4(0.0);
    float src_a = original.a * vertex_color.a;
    bool isOpaque = inside && original.a >= 0.99 && vertex_color.a >= 0.99;

    if (isOpaque) {
        COLOR = original * vertex_color;
    } else {
        float glow_radius_sqrt = sqrt(glow_radius);
        int num_directions = int(clamp(glow_radius_sqrt * 3.0, 16.0, 48.0));
        float steps_scale = clamp(glow_radius_sqrt * 1.25, 2.0, 16.0);
        int num_steps = int(ceil(steps_scale));
        float angle_step = 6.283185 / float(num_directions);

        vec2 px = TEXTURE_PIXEL_SIZE;
        float sigma_sq = glow_radius * glow_radius * 0.36;
        float inv_two_sigma_sq = 1.0 / (2.0 * sigma_sq);
        float step_size = glow_radius / float(num_steps);

        float alpha_sum = 0.0;
        float total_weight = 0.0;

        for (int d = 0; d < num_directions; d++) {
            float angle = float(d) * angle_step;
            vec2 dir = vec2(cos(angle), sin(angle));

            for (int s = 1; s <= num_steps; s++) {
                float dist = float(s) * step_size;
                vec2 offset = dir * dist;
                float weight = fast_exp(-(dist * dist) * inv_two_sigma_sq);
                if (weight < 0.01) break;

                vec2 sample_uv = uv + offset * px;
                if (sample_uv == clamp(sample_uv, vec2(0.0), vec2(1.0))) {
                    alpha_sum += texture(TEXTURE, sample_uv).a * weight;
                    total_weight += weight;
                }
            }
        }

        total_weight = max(total_weight, 0.0001);
        float glow_alpha = (alpha_sum / total_weight) * glow_strength * glow_color.a * vertex_color.a;
        vec4 glow = vec4(glow_color.rgb * vertex_color.rgb, glow_alpha);

        COLOR = mix(glow, original * vertex_color, src_a);
    }
}