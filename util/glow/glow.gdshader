shader_type canvas_item;
render_mode unshaded;

uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;

// Each sample stands for a cell of at most this many texels. Fewer samples are needed this way,
// and a mip level as wide as the cell blurs the sprite over it, so the samples blend into a smooth glow.
// Larger cells would need coarse, blocky mip levels.
const float SAMPLE_CELL = 20.0;
// Reads a slightly sharper mip level than the cell size, so the glow keeps its shape near the sprite.
const float LOD_BIAS = -0.5;
// Every ring is turned by this angle, so the samples of neighbouring rings never line up into spokes.
const float GOLDEN_ANGLE = 2.39996323;

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
	if (glow_radius < 0.5 || glow_strength < 0.01) {
		discard;
	}
	vec2 expanded_scale = vec2(1.0) + 2.0 * glow_radius * TEXTURE_PIXEL_SIZE;
	vec2 uv = (UV - 0.5) * expanded_scale + 0.5;

	float base_alpha = texture(TEXTURE, uv).a * vertex_color.a;
	if (base_alpha > 0.99) {
		discard;
	}

	// Rings and directions are spread so that a sample stands for a cell of about SAMPLE_CELL texels
	int num_directions = int(clamp(round(TAU * glow_radius / SAMPLE_CELL), 12.0, 48.0));
	int num_rings = int(clamp(round(glow_radius / SAMPLE_CELL), 3.0, 16.0));
	float angle_step = TAU / float(num_directions);
	// Turn by one angle step. Every next direction is the previous one turned by it,
	// so no cos() and sin() are needed per sample.
	vec2 turn = vec2(cos(angle_step), sin(angle_step));

	vec2 px = 1.0 / vec2(textureSize(TEXTURE, 0));
	float sigma_sq = glow_radius * glow_radius * 0.36;
	float inv_two_sigma_sq = 1.0 / (2.0 * sigma_sq);
	float ring_step = glow_radius / float(num_rings);

	float alpha_sum = 0.0;
	float total_weight = 0.0;

	// Samples lie on rings around the pixel. The weight depends only on the ring,
	// so it is computed once per ring and applied to the sum of its samples.
	for (int s = 1; s <= num_rings; s++) {
		float dist = float(s) * ring_step;
		float weight = fast_exp(-dist * dist * inv_two_sigma_sq);
		vec2 ring_offset = dist * px;

		// A sample stands for the cell between it and its neighbours on the ring and on the next ring
		float cell = max(TAU * dist / float(num_directions), ring_step);
		float lod = max(log2(cell) + LOD_BIAS, 0.0);

		float start_angle = float(s) * GOLDEN_ANGLE;
		vec2 dir = vec2(cos(start_angle), sin(start_angle));
		float ring_sum = 0.0;
		for (int d = 0; d < num_directions; d++) {
			vec2 sample_uv = uv + dir * ring_offset;
			// There is no sprite outside of its texture. Coarse mip levels must not be clamped to their edge,
			// which already holds the averaged sprite and would spread a haze over the whole glow.
			float inside = step(0.0, sample_uv.x) * step(sample_uv.x, 1.0) * step(0.0, sample_uv.y) * step(sample_uv.y, 1.0);
			ring_sum += textureLod(TEXTURE, sample_uv, lod).a * inside;
			dir = vec2(dir.x * turn.x - dir.y * turn.y, dir.x * turn.y + dir.y * turn.x);
		}

		alpha_sum += ring_sum * weight;
		total_weight += weight;
	}
	// Every ring has the same number of samples
	total_weight *= float(num_directions);

	total_weight = max(total_weight, 0.0001);
	float glow_alpha = (alpha_sum / total_weight) * glow_strength * glow_color.a * vertex_color.a;
	COLOR = vec4(glow_color.rgb * vertex_color.rgb, glow_alpha);
}