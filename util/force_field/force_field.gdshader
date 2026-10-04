shader_type canvas_item;
render_mode blend_add, unshaded;

// A circular force field with a water-like surface. The ripples are bent by noise and move in one of the modes:
// - Waves: several sets of waves cross the field from different sides, like water stirred from all sides.
// - Outward: rings spread from the center, like the field pushes things away.
// - Inward: rings run into the center, like the field pulls things in.
// A brighter rim marks the border of the field.
// The field fills the inscribed circle of its rect, so the rect should be square.

// Color of the field. The brightest parts shift towards white on top of it.
uniform vec4 field_color : source_color = vec4(0.22, 0.63, 0.93, 1.0);
// Overall brightness.
uniform float intensity : hint_range(0.0, 2.0) = 1.0;
// Radius of the field, as a part of the full radius. 0 = no field, 1 = full size.
// Tween 0 -> 1 to open the field, 1 -> 0 to close it.
uniform float activation : hint_range(0.0, 1.0) = 1.0;

// Brightness of the area inside the field.
uniform float fill_strength : hint_range(0.0, 1.0) = 0.22;
// Brightness of the border of the field.
uniform float rim_strength : hint_range(0.0, 3.0) = 1.1;
// Thickness of the border, as a part of the radius.
uniform float rim_width : hint_range(0.01, 0.5) = 0.13;
// How much the border is whitened. 0 keeps the field color everywhere.
uniform float rim_whiteness : hint_range(0.0, 1.0) = 0.45;
// Softness of the outer edge, as a part of the radius.
uniform float edge_softness : hint_range(0.01, 0.5) = 0.13;

// How the ripples move, see the description at the top.
uniform int ripple_mode : hint_enum("Waves", "Outward", "Inward") = 0;
// Number of waves fitting into the radius.
uniform float ripple_count : hint_range(1.0, 20.0) = 5.0;
// How fast the waves run and the surface changes.
uniform float ripple_speed : hint_range(0.0, 5.0) = 0.5;
// Contrast of the waves.
uniform float ripple_strength : hint_range(0.0, 1.0) = 0.5;
// How strongly the noise bends the waves. 0 gives straight overlapping waves, higher looks like water.
uniform float wobble : hint_range(0.0, 1.0) = 0.8;
// Scale of the surface noise. Higher = finer bends.
uniform float detail : hint_range(1.0, 8.0) = 2.2;

// Shift of the noise pattern. A random value per field makes fields look different from each other.
uniform vec2 noise_offset = vec2(0.0);

// Lattice period of gradient_noise(). A power of two, so mod() is exact for any input.
const float NOISE_PERIOD = 1024.0;
// Scales gradient noise so its spread matches value noise in 0..1.
const float NOISE_CONTRAST = 1.24;
// Upper bound of fbm(): the sum of octave amplitudes.
const float FBM_MAX = 0.9375;

// Values of ripple_mode that the code checks. Outward is 1, it is the default of the ring branch.
const int RIPPLE_MODE_WAVES = 0;
const int RIPPLE_MODE_INWARD = 2;

// How far the surface noise moves the waves at full wobble, in field units (radius = 1).
// Larger than a wave length, so the waves meander instead of only wiggling.
const float WARP_SCALE = 1.0;

// Directions of the 4 wave sets. Each next one is turned by the golden angle (137.5 degrees),
// which spreads directions as unevenly as possible, so the overlap never forms a symmetric pattern.
const vec2 WAVE_DIRECTION_0 = vec2(1.0, 0.0);
const vec2 WAVE_DIRECTION_1 = vec2(-0.73737, 0.67549);
const vec2 WAVE_DIRECTION_2 = vec2(0.08743, -0.99617);
const vec2 WAVE_DIRECTION_3 = vec2(0.60844, 0.79360);
// Wave length, speed and strength of each set relative to the uniforms. The ratios are not simple,
// so the sets never line up into a repeating grid.
const vec4 WAVE_SCALES = vec4(1.0, 1.31, 0.83, 1.17);
const vec4 WAVE_SPEEDS = vec4(1.0, 0.8, 1.15, 0.9);
const vec4 WAVE_WEIGHTS = vec4(1.0, 0.85, 0.7, 0.55);

// Sine-free hash, returns 0..1 per component. Expects small inputs, see gradient_noise().
vec2 hash22(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.xx + p3.yz) * p3.zy);
}

// Gradient noise centered at 0.5. Unlike value noise, its contours do not snap to the lattice axes,
// so the surface stays organic.
float gradient_noise(vec2 p) {
	vec2 cell = floor(p);
	vec2 f = p - cell;
	vec2 u = f * f * (3.0 - 2.0 * f);
	// Wrap lattice coordinates before hashing. They grow with TIME, and the hash of a large number
	// is so sensitive to rounding that a shared corner could hash differently in two neighbouring
	// cells, leaving seams that drift across the field.
	vec2 i0 = mod(cell, NOISE_PERIOD);
	vec2 i1 = mod(cell + 1.0, NOISE_PERIOD);
	float a = dot(hash22(i0) * 2.0 - 1.0, f);
	float b = dot(hash22(vec2(i1.x, i0.y)) * 2.0 - 1.0, f - vec2(1.0, 0.0));
	float c = dot(hash22(vec2(i0.x, i1.y)) * 2.0 - 1.0, f - vec2(0.0, 1.0));
	float d = dot(hash22(i1) * 2.0 - 1.0, f - vec2(1.0, 1.0));
	return 0.5 + NOISE_CONTRAST * mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

// Fractal noise in 0..FBM_MAX with the average at FBM_MAX / 2.
float fbm(vec2 p, int octaves) {
	float v = 0.0;
	float amp = 0.5;
	float total = 0.0;
	for (int i = 0; i < octaves; i++) {
		v += amp * gradient_noise(p);
		total += amp;
		// Every octave is rotated by about 37 degrees. Without it the lattices of all octaves line up,
		// the noise varies less along their shared cell borders, and the waves bent by it show
		// straight darker lines drifting across the field.
		p = vec2(0.8 * p.x - 0.6 * p.y, 0.6 * p.x + 0.8 * p.y) * 2.0 + vec2(37.0, 17.0);
		amp *= 0.5;
	}
	return clamp(v * (FBM_MAX / total), 0.0, FBM_MAX);
}

void fragment() {
	// Centered coordinates, -1..1. The field fills the inscribed circle.
	vec2 p = (UV - 0.5) * 2.0;

	// Field coordinates grow with activation, so the whole pattern opens together with the field.
	vec2 f = p / max(activation, 0.001);
	// 0 at the center, 1 at the border of the active field.
	float r = length(f);
	if (r > 1.0) {
		discard;
	}

	float t = TIME * ripple_speed;
	vec2 q = p * detail + noise_offset;

	// Two noise layers drifting apart make the surface move without repeating.
	float n1 = fbm(q + vec2(t * 0.15, -t * 0.1), 3);
	float n2 = fbm(q * 1.7 + vec2(-t * 0.12, t * 0.17) + 19.3, 2);

	// The mode is a uniform, so every pixel takes the same branch and the other one is skipped.
	float interference = 0.0;
	if (ripple_mode == RIPPLE_MODE_WAVES) {
		// The noise moves the point where the waves are measured, which bends their fronts.
		vec2 warp = (vec2(n1, n2) - FBM_MAX * 0.5) * WARP_SCALE;
		vec2 wf = f + warp * wobble;

		// Every set of waves runs along its own direction: its fronts are perpendicular to it
		// and move forward with time.
		vec4 positions = vec4(
			dot(wf, WAVE_DIRECTION_0),
			dot(wf, WAVE_DIRECTION_1),
			dot(wf, WAVE_DIRECTION_2),
			dot(wf, WAVE_DIRECTION_3)
		);
		vec4 waves = sin((positions * ripple_count * WAVE_SCALES - t * WAVE_SPEEDS) * TAU);
		// The sets overlap: crests meet and brighten, troughs cancel. Normalized by the combined weight,
		// so the contrast is the same as of a single set. Where several crests meet the sum goes above 1,
		// which is clamped into a bright spot.
		interference = dot(waves, WAVE_WEIGHTS) / length(WAVE_WEIGHTS);
	} else {
		// Rings around the center. The noise shifts the radius they are measured at,
		// which bends them like waves on water.
		float warp = (n1 - FBM_MAX * 0.5) * 0.7 + (n2 - FBM_MAX * 0.5) * 0.3;
		float ripple_radius = r + warp * wobble;
		// Outward rings move to bigger radii with time, inward ones to smaller.
		float direction = ripple_mode == RIPPLE_MODE_INWARD ? -1.0 : 1.0;
		interference = sin((ripple_radius * ripple_count - t * direction) * TAU);
	}

	float rings = clamp(interference, -1.0, 1.0) * 0.5 + 0.5;
	// Squaring sharpens the crests and widens the troughs, like waves on water.
	rings *= rings;

	// Soft outer edge of the inner area.
	float fade = 1.0 - smoothstep(1.0 - edge_softness, 1.0, r);
	float inside = (fill_strength * mix(0.7, 1.3, n1) + rings * ripple_strength) * fade;

	// A band ending exactly at the border, so it fades out on its own and is not dimmed by the fade above.
	float rim_center = 1.0 - rim_width * 0.5;
	float rim = 1.0 - smoothstep(0.0, rim_width * 0.5, abs(r - rim_center));

	float brightness = (inside + rim * rim_strength) * intensity;

	vec3 color = mix(field_color.rgb, vec3(1.0), rim * rim_whiteness);

	// Color is premultiplied, so alpha 1.0 means "just add rgb".
	COLOR = vec4(color * brightness, 1.0);
}
