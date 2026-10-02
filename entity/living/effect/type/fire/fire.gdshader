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

// Sine-free hash, stable for large inputs (TIME keeps growing).
float hash(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

float value_noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	float a = hash(i);
	float b = hash(i + vec2(1.0, 0.0));
	float c = hash(i + vec2(0.0, 1.0));
	float d = hash(i + vec2(1.0, 1.0));
	return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
	float v = 0.0;
	float amp = 0.5;
	for (int i = 0; i < 4; i++) {
		v += amp * value_noise(p);
		p = p * 2.0 + vec2(37.0, 17.0);
		amp *= 0.5;
	}
	return v;
}

void fragment() {
	// Centered coordinates, -1..1. Y points down, so "up" is negative Y.
	vec2 p = (UV - 0.5) * 2.0;
	float t = TIME * speed;

	// Ignition scales the flame body around the center.
	float size = max(ignition, 0.001);
	vec2 c = p / size - vec2(0.0, BODY_OFFSET);

	// 0 at the body center, 1 at the top of the tongues.
	float up = clamp(-c.y, 0.0, 1.0);

	// Sideways sway, stronger higher up.
	float sway = fbm(vec2(p.y * detail * 0.5 + t * 0.9, t * 0.3)) - 0.5;
	vec2 np = p + vec2(sway * turbulence * 0.6 * up, 0.0);

	// Noise scrolls upward and is stretched vertically -> elongated rising tongues.
	float n1 = fbm(vec2(np.x * detail, np.y * detail * 0.5 + t * 1.6));
	float n2 = fbm(vec2(np.x * detail * 2.0 + 7.3, np.y * detail + t * 2.7));
	float noise_val = n1 * 0.65 + n2 * 0.35;

	// Heat field: a disc stretched upward, 1 in the center, 0 at the body edge.
	vec2 shape = c;
	shape.x += sway * turbulence * 0.4 * up;
	if (shape.y < 0.0) {
		shape.y /= 1.0 + flame_height;
	}
	float heat = 1.0 - length(shape) / BODY_RADIUS;

	// Noise erodes the heat. The core stays solid, the top breaks into tongues.
	float erosion = turbulence * mix(0.35, 1.0, up);
	heat += (noise_val - 0.5) * 1.6 * erosion;
	heat *= intensity;
	heat -= (1.0 - ignition) * 0.2;

	// Layers by heat thresholds: outer -> mid -> core.
	float alpha = smoothstep(0.0, edge_softness, heat);
	float mid_mask = smoothstep(0.25, 0.25 + edge_softness, heat);
	float core_mask = smoothstep(0.55, 0.55 + edge_softness, heat);
	vec3 color = outer_color.rgb;
	color = mix(color, mid_color.rgb, mid_mask);
	color = mix(color, core_color.rgb, core_mask);

	// Flickering halo.
	vec2 g = p / size - vec2(0.0, BODY_OFFSET * 0.5);
	float glow = exp(-dot(g, g) * 3.0);
	float flicker = 0.8 + 0.4 * value_noise(vec2(t * 4.0, 3.7));
	glow *= glow_strength * intensity * ignition * flicker;

	vec3 rgb = color * alpha + glow_color.rgb * glow;

	// Fade out near the quad border so nothing is ever cut off sharply.
	vec2 edge = min(UV, 1.0 - UV);
	rgb *= smoothstep(0.0, 0.1, min(edge.x, edge.y));

	// Color is premultiplied, so alpha 1.0 means "just add rgb".
	COLOR = vec4(rgb, 1.0);
}
