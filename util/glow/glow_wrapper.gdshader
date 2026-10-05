shader_type canvas_item;
render_mode unshaded;

uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear;

// Screen pixels per local pixel, computed from the canvas matrices.
// Makes the glow radius consistent regardless of window size, stretch, or zoom.
varying vec2 local_to_screen;

float fast_exp(float x) {
    x = 1.0 + x * (1.0 + x * 0.5);
    return x * x;
}

void vertex() {
    mat4 wv = CANVAS_MATRIX * MODEL_MATRIX;
    local_to_screen = vec2(length(wv[0].xy), length(wv[1].xy));
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
        // Turn by one angle step. Every next direction is the previous one turned by it,
        // so no cos() and sin() are needed per sample.
        vec2 turn = vec2(cos(angle_step), sin(angle_step));

        vec2 px = SCREEN_PIXEL_SIZE * local_to_screen;
        float sigma_sq = glow_radius * glow_radius * 0.36;
        float inv_two_sigma_sq = 1.0 / (2.0 * sigma_sq);
        float step_size = glow_radius / float(num_steps);

        // Samples lie on rings around the pixel, one ring per step. The weight depends only on the ring,
        // so it is computed once per ring and applied to the sum of its samples.
        // fast_exp() never goes below 0.25, so no ring is ever too weak to be sampled.
        for (int s = 1; s <= num_steps; s++) {
            float dist = float(s) * step_size;
            float weight = fast_exp(-dist * dist * inv_two_sigma_sq);
            vec2 ring_offset = dist * px;

            vec2 dir = vec2(1.0, 0.0);
            float ring_sum = 0.0;
            for (int d = 0; d < num_directions; d++) {
                ring_sum += texture(screen_texture, SCREEN_UV + dir * ring_offset).a;
                dir = vec2(dir.x * turn.x - dir.y * turn.y, dir.x * turn.y + dir.y * turn.x);
            }

            alpha_sum += ring_sum * weight;
            total_weight += weight;
        }
        // Every ring has the same number of samples
        total_weight *= float(num_directions);
    }

    total_weight = max(total_weight, 0.0001);
    float glow_alpha = (alpha_sum / total_weight) * glow_strength * glow_color.a;

    // screen_texture in CanvasGroup is premultiplied, so original.rgb already == sprite.rgb * sprite.a.
    // Don't multiply by original.a again — that would darken edges.
    float effective_glow_alpha = glow_alpha * (1.0 - original.a);
    float out_alpha = original.a + effective_glow_alpha;
    vec3 out_rgb = out_alpha > 0.0001
        ? (original.rgb + glow_color.rgb * effective_glow_alpha) / out_alpha
        : vec3(0.0);

    COLOR = vec4(out_rgb, out_alpha);
}
