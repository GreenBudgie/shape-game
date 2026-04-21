shader_type canvas_item;
render_mode unshaded;

uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;

float fast_exp(float x) {
    x = 1.0 + x * (1.0 + x * 0.5);
    return x * x;
}

void fragment() {
    vec4 original = texture(screen_texture, SCREEN_UV);

    bool skip_glow = glow_radius < 0.5 || glow_strength < 0.01 || original.a > 0.99;

    float alpha_sum = 0.0;
    float total_weight = 0.0;

    if (!skip_glow) {
        float glow_radius_sqrt = sqrt(glow_radius);
        int num_directions = int(clamp(glow_radius_sqrt * 3.0, 16.0, 48.0));
        float steps_scale = clamp(glow_radius_sqrt * 1.25, 2.0, 16.0);
        int num_steps = int(ceil(steps_scale));
        float angle_step = 6.283185 / float(num_directions);

        vec2 px = SCREEN_PIXEL_SIZE;
        float sigma_sq = glow_radius * glow_radius * 0.36;
        float inv_two_sigma_sq = 1.0 / (2.0 * sigma_sq);
        float step_size = glow_radius / float(num_steps);

        for (int d = 0; d < num_directions; d++) {
            float angle = float(d) * angle_step;
            vec2 dir = vec2(cos(angle), sin(angle));

            for (int s = 1; s <= num_steps; s++) {
                float dist = float(s) * step_size;
                vec2 offset = dir * dist * px;
                float dist_sq = dist * dist;
                float weight = fast_exp(-dist_sq * inv_two_sigma_sq);
                if (weight < 0.01) break;

                alpha_sum += texture(screen_texture, SCREEN_UV + offset).a * weight;
                total_weight += weight;
            }
        }
    }

    total_weight = max(total_weight, 0.0001);
    float glow_alpha = (alpha_sum / total_weight) * glow_strength * glow_color.a;

    float effective_glow_alpha = glow_alpha * (1.0 - original.a);
    float out_alpha = original.a + effective_glow_alpha;
    vec3 out_rgb = out_alpha > 0.0001
        ? (original.rgb * original.a + glow_color.rgb * effective_glow_alpha) / out_alpha
        : vec3(0.0);

    COLOR = vec4(out_rgb, out_alpha);
}
