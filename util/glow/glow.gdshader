shader_type canvas_item;

uniform sampler2D viewport_texture : hint_default_transparent;
uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;
uniform bool cull_occluded = true;

// Fast exp approximation (2nd order Taylor)
float fast_exp(float x) {
    x = 1.0 + x * (1.0 + x * 0.5);
    return x * x;
}

void fragment() {
    // Early discard with tighter threshold
    if (cull_occluded && texture(viewport_texture, UV).a > 0.95) discard;

    // Dynamic quality: number of directions and steps
	float glow_radius_sqrt = sqrt(glow_radius);
    int num_directions = int(clamp(glow_radius_sqrt * 3.0, 16.0, 48.0));
    float steps_scale = clamp(glow_radius_sqrt * 1.25, 2.0, 16.0); // Steps per direction, clamped
    int num_steps = int(ceil(steps_scale));
    float angle_step = 6.283185 / float(num_directions); // 2*PI / directions

    // Pre-calculations
    vec2 px = 1.0 / vec2(textureSize(viewport_texture, 0));
    float sigma_sq = glow_radius * glow_radius * 0.36;
    float inv_two_sigma_sq = 1.0 / (2.0 * sigma_sq);
    float step_size = glow_radius / float(num_steps); // Radial step size

    // Radial sampling for blur
    float alpha_sum = 0.0;
    float total_weight = 0.0;

    for (int d = 0; d < num_directions; d++) {
        float angle = float(d) * angle_step;
        vec2 dir = vec2(cos(angle), sin(angle));

        for (int s = 1; s <= num_steps; s++) { // Start from 1 to avoid center (already culled)
            float dist = float(s) * step_size;
            vec2 offset = dir * dist;
            float dist_sq = dist * dist;

            // Gaussian weight
            float weight = fast_exp(-dist_sq * inv_two_sigma_sq);
            if (weight < 0.01) break; // Early break for low weights

            alpha_sum += texture(viewport_texture, UV + offset * px).a * weight;
            total_weight += weight;
        }
    }

    // Avoid division by zero
    total_weight = max(total_weight, 0.0001);
    COLOR = vec4(glow_color.rgb, (alpha_sum / total_weight) * glow_strength * glow_color.a);
}