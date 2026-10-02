shader_type canvas_item;
render_mode blend_add, unshaded;

// How hot the fire burns. Lower = dim orange flames without a white core,
// higher = bigger white core and brighter glow.
uniform float intensity : hint_range(0.0, 2.0) = 1.0;

// Lifecycle of the fire. 0 = no fire, 1 = fully burning.
// Tween 0 -> 1 for ignition, 1 -> 0 for fade out.
uniform float ignition : hint_range(0.0, 1.0) = 1.0;

// Flame layers, from the hottest to the coolest.
uniform vec4 core_color : source_color = vec4(1.0, 0.98, 0.9, 1.0);
uniform vec4 mid_color : source_color = vec4(1.0, 0.7, 0.2, 1.0);
uniform vec4 outer_color : source_color = vec4(1.0, 0.25, 0.04, 1.0);

// Soft halo around the flames.
uniform vec4 glow_color : source_color = vec4(1.0, 0.35, 0.08, 1.0);
uniform float glow_strength : hint_range(0.0, 2.0) = 0.5;

// Animation speed of the flames.
uniform float speed : hint_range(0.0, 5.0) = 1.5;
// How far the tongues stretch upward.
uniform float flame_height : hint_range(0.0, 1.5) = 0.7;
// How strongly the noise tears the flame body into tongues and sways them.
uniform float turbulence : hint_range(0.0, 1.0) = 0.7;
// Noise scale. Higher = more, thinner tongues.
uniform float detail : hint_range(1.0, 8.0) = 3.0;
// Softness of the flame edges and of the transitions between color layers.
uniform float edge_softness : hint_range(0.01, 0.5) = 0.15;

// Radius of the flame body in centered UV space (quad half-size = 1).
const float BODY_RADIUS = 0.5;
// The body sits slightly below the quad center to leave room for the tongues.
const float BODY_OFFSET = 0.15;
// How far the sway shifts the flame body sideways.
const float SWAY_SHIFT = 0.4;
// How strongly the noise changes the heat field.
const float NOISE_GAIN = 1.6;
// Upper bound of fbm(): the sum of 4 octave amplitudes.
const float FBM_MAX = 0.9375;
// Scales gradient noise so its spread matches value noise in 0..1 (std ~0.21 vs ~0.17).
const float NOISE_CONTRAST = 1.24;
// Lattice period of gradient_noise(). A power of two, so mod() is exact for any input.
const float NOISE_PERIOD = 1024.0;

// Glow multiplier with flicker. Depends only on time and uniforms, so it is computed per vertex.
varying float glow_gain;

// Sine-free hash, returns 0..1 per component. Expects small inputs, see gradient_noise().
vec2 hash22(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.xx + p3.yz) * p3.zy);
}

// Gradient noise centered at 0.5. Unlike value noise, its contours do not snap to the
// lattice axes, so thresholded flame edges stay organic.
float gradient_noise(vec2 p) {
	vec2 cell = floor(p);
	vec2 f = p - cell;
	vec2 u = f * f * (3.0 - 2.0 * f);
	// Wrap lattice coordinates before hashing. Y grows with TIME, and the hash of a large
	// number is so sensitive to rounding that a shared corner could hash differently in two
	// neighbouring cells, leaving horizontal seams that scroll up with the flames.
	vec2 i0 = mod(cell, NOISE_PERIOD);
	vec2 i1 = mod(cell + 1.0, NOISE_PERIOD);
	float a = dot(hash22(i0) * 2.0 - 1.0, f);
	float b = dot(hash22(vec2(i1.x, i0.y)) * 2.0 - 1.0, f - vec2(1.0, 0.0));
	float c = dot(hash22(vec2(i0.x, i1.y)) * 2.0 - 1.0, f - vec2(0.0, 1.0));
	float d = dot(hash22(i1) * 2.0 - 1.0, f - vec2(1.0, 1.0));
	return 0.5 + NOISE_CONTRAST * mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

// Fractal noise in 0..FBM_MAX with the average at FBM_MAX / 2. Fewer octaves are cheaper
// and lose only the finest detail. Clamped so the early-out bound in fragment() always holds.
float fbm(vec2 p, int octaves) {
	float v = 0.0;
	float amp = 0.5;
	float total = 0.0;
	for (int i = 0; i < octaves; i++) {
		v += amp * gradient_noise(p);
		total += amp;
		p = p * 2.0 + vec2(37.0, 17.0);
		amp *= 0.5;
	}
	return clamp(v * (FBM_MAX / total), 0.0, FBM_MAX);
}

void vertex() {
	float flicker = 0.8 + 0.4 * gradient_noise(vec2(TIME * speed * 4.0, 3.7));
	glow_gain = glow_strength * intensity * ignition * flicker;
}

void fragment() {
	// Centered coordinates, -1..1. Y points down, so "up" is negative Y.
	vec2 p = (UV - 0.5) * 2.0;
	float t = TIME * speed;

	// Ignition scales the flame body around the center.
	float inv_size = 1.0 / max(ignition, 0.001);
	vec2 c = p * inv_size - vec2(0.0, BODY_OFFSET);

	// 0 at the body center, 1 at the top of the tongues.
	float up = clamp(-c.y, 0.0, 1.0);

	// Heat field: a disc stretched upward, 1 in the center, 0 at the body edge.
	vec2 shape = c;
	if (shape.y < 0.0) {
		shape.y /= 1.0 + flame_height;
	}
	float heat = 1.0 - length(shape) / BODY_RADIUS;

	// Noise erodes the heat. The core stays solid, the top breaks into tongues.
	float erosion = turbulence * mix(0.35, 1.0, up);

	// The most heat that sway (|sway| <= 0.5) and noise (<= FBM_MAX) can add here.
	// If even that is not enough, the flame is fully transparent, so all noise is skipped.
	float max_sway_heat = 0.5 * SWAY_SHIFT * turbulence * up / BODY_RADIUS;
	float max_noise_heat = (FBM_MAX - 0.5) * NOISE_GAIN * erosion;

	vec3 rgb = vec3(0.0);
	if (heat + max_sway_heat + max_noise_heat > 0.0) {
		// Sideways sway, stronger higher up.
		float sway = fbm(vec2(p.y * detail * 0.5 + t * 0.9, t * 0.3), 3) - 0.5;
		vec2 np = p + vec2(sway * turbulence * 0.6 * up, 0.0);

		// Noise scrolls upward and is stretched vertically -> elongated rising tongues.
		float n1 = fbm(vec2(np.x * detail, np.y * detail * 0.5 + t * 1.6), 4);
		float n2 = fbm(vec2(np.x * detail * 2.0 + 7.3, np.y * detail + t * 2.7), 3);
		float noise_val = n1 * 0.65 + n2 * 0.35;

		shape.x += sway * SWAY_SHIFT * turbulence * up;
		heat = 1.0 - length(shape) / BODY_RADIUS;
		heat += (noise_val - 0.5) * NOISE_GAIN * erosion;
		heat *= intensity;
		heat -= (1.0 - ignition) * 0.2;

		// Layers by heat thresholds: outer -> mid -> core.
		float alpha = smoothstep(0.0, edge_softness, heat);
		float mid_mask = smoothstep(0.25, 0.25 + edge_softness, heat);
		float core_mask = smoothstep(0.55, 0.55 + edge_softness, heat);
		vec3 color = outer_color.rgb;
		color = mix(color, mid_color.rgb, mid_mask);
		color = mix(color, core_color.rgb, core_mask);
		rgb = color * alpha;
	}

	// Flickering halo.
	vec2 g = p * inv_size - vec2(0.0, BODY_OFFSET * 0.5);
	rgb += glow_color.rgb * (exp(-dot(g, g) * 3.0) * glow_gain);

	// Fade out near the quad border so nothing is ever cut off sharply.
	vec2 edge = min(UV, 1.0 - UV);
	rgb *= smoothstep(0.0, 0.1, min(edge.x, edge.y));

	// Color is premultiplied, so alpha 1.0 means "just add rgb".
	COLOR = vec4(rgb, 1.0);
}
