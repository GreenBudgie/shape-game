shader_type canvas_item;
render_mode unshaded;

uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0, 200) = 12.0;
uniform float glow_strength : hint_range(0, 10) = 2.0;

// Size of the ColorRect in local pixels. Set from RectGlow, replaces texture size.
uniform vec2 rect_size = vec2(1.0);

varying vec4 vertex_color;

// Standard normal CDF, logistic approximation. Smooth everywhere, cheap, ~1% error.
float ndf_cdf(float z) {
	return 1.0 / (1.0 + exp(-1.702 * z));
}

// How far past the rectangle edge the halo needs to reach before it fades to ~0.
// Grows with the blur sigma and, logarithmically, with strength (which lifts the tail).
float glow_extent(float sigma) {
	return sigma * (3.24 + 0.588 * log(max(glow_strength, 1.0)));
}

void vertex() {
	vertex_color = COLOR;
	float sigma = glow_radius * 0.35;
	VERTEX += (UV - 0.5) * 2.0 * glow_extent(sigma);
}

void fragment() {
	if (glow_radius < 0.5 || glow_strength < 0.01) {
		discard;
	}
	float sigma = glow_radius * 0.35;
	float extent = glow_extent(sigma);

	vec2 pixel_size = 1.0 / rect_size;
	vec2 expanded_scale = vec2(1.0) + 2.0 * extent * pixel_size;
	vec2 uv = (UV - 0.5) * expanded_scale + 0.5;

	// Gaussian blur of a solid rectangle is separable: the coverage at a point is
	// the product of two 1D edge integrals. Smooth fill inside, soft falloff outside,
	// no banding, no texture samples.
	vec2 pos = uv * rect_size;
	float inv_sigma = 1.0 / sigma;
	float coverage_x = ndf_cdf(pos.x * inv_sigma) - ndf_cdf((pos.x - rect_size.x) * inv_sigma);
	float coverage_y = ndf_cdf(pos.y * inv_sigma) - ndf_cdf((pos.y - rect_size.y) * inv_sigma);
	float coverage = coverage_x * coverage_y;

	// strength shapes the outer halo (1 = plain gaussian blur, >1 = brighter/wider glow),
	// while the inside stays a solid fill.
	float alpha = (1.0 - pow(1.0 - coverage, glow_strength)) * glow_color.a * vertex_color.a;

	// Feather the very edge of the expanded quad so the halo can never cut off sharply.
	vec2 edge = min(UV, 1.0 - UV);
	alpha *= smoothstep(0.0, 0.02, min(edge.x, edge.y));

	COLOR = vec4(glow_color.rgb * vertex_color.rgb, alpha);
}
