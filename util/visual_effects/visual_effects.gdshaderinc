uniform vec4 base_tint : source_color = vec4(1.0);

group_uniforms Glow;
uniform bool glow_enabled = false;
uniform vec4 glow_color : source_color = vec4(0.937, 0.357, 0.357, 1.0);
uniform float glow_radius : hint_range(0.0, 200.0) = 12.0;
uniform float glow_strength : hint_range(0.0, 10.0) = 2.0;
uniform bool pulse_enabled = false;
uniform float pulse_radius_delta : hint_range(0.0, 200.0) = 0.0;
uniform float pulse_strength_delta : hint_range(0.0, 10.0) = 0.0;
uniform float pulses_per_second : hint_range(0.0, 10.0) = 1.0;

group_uniforms Mask;
uniform bool mask_enabled = false;
uniform float mask_progress : hint_range(0.0, 1.0) = 1.0;
uniform int mask_axis : hint_enum("Horizontal", "Vertical") = 0;
uniform int mask_origin : hint_enum("Center", "Left or top", "Right or bottom") = 0;

group_uniforms Dissolve;
uniform bool dissolve_enabled = false;
uniform float dissolve_value : hint_range(0.0, 1.0) = 1.0;
/** Optional NoiseTexture2D. Opaque texels supply the threshold in their red channel.
 * Transparent texels use the built-in noise. */
uniform sampler2D dissolve_texture : hint_default_transparent, repeat_disable;

group_uniforms Stretch;
uniform bool stretch_enabled = false;
uniform float stretch_amount : hint_range(0.0, 2.0) = 0.0;
uniform vec2 stretch_direction = vec2(1.0, 0.0);
group_uniforms;

/** Optional local rectangle (x, y, width, height). A positive size anchors group
 * effects independently of Fit Margin. On flipped sprites, use the sprite's local
 * drawing rectangle so the halo expands outward. */
uniform vec4 local_rect = vec4(0.0);

varying vec2 original_position;
#ifndef CANVAS_GROUP_SOURCE
varying vec2 padding;
#endif
varying vec4 item_color;
varying flat vec2 halo; // effective radius in local pixels and strength
varying flat mat2 stretch_basis;

vec2 glow_parameters(float time) {
	float pulse = 0.0;
	if (glow_enabled && pulse_enabled) {
		pulse = 0.5 + 0.5 * sin(time * TAU * pulses_per_second);
	}
	vec2 result = vec2(glow_radius, glow_strength) + pulse * vec2(pulse_radius_delta, pulse_strength_delta);
	return (glow_enabled && result.x >= 0.5 && result.y >= 0.01 && glow_color.a > 0.0) ? result : vec2(0.0);
}

mat2 stretch_transform() {
	vec2 axis = stretch_direction / max(length(stretch_direction), 0.00001);
	return mat2(1.0) + mat2(axis * axis.x, axis * axis.y) * stretch_amount;
}

// Standalone include validation has no canvas_item context for the entry points.
#ifdef EFFECT_SOURCE
void vertex() {
	item_color = COLOR;
	original_position = VERTEX;
	halo = glow_parameters(TIME);
	stretch_basis = stretch_enabled ? stretch_transform() : mat2(1.0);
#ifndef CANVAS_GROUP_SOURCE
	// Godot's indexed and six-vertex canvas rectangles use this corner order.
	const vec2 corners[6] = {vec2(-1,-1), vec2(-1,1), vec2(1,1), vec2(1,-1), vec2(-1,-1), vec2(1,1)};
	vec2 corner = corners[VERTEX_ID % 6];
	if (all(greaterThan(local_rect.zw, vec2(0.0)))) {
		corner = sign(VERTEX - local_rect.xy - local_rect.zw * 0.5);
	}
	padding = corner * halo.x;
	VERTEX += padding;
#endif
}
#endif

float noise_hash(vec2 cell) {
	vec3 p = fract(cell.xyx * vec3(0.1031, 0.1030, 0.0973));
	p += dot(p, p.yzx + 33.33);
	return fract((p.x + p.y) * p.z);
}

// A material can be previewed before assigning its noise texture.
float dissolve_noise(vec2 uv) {
	vec4 noise_sample = textureLod(dissolve_texture, uv, 0.0);
	if (noise_sample.a > 0.0) {
		return noise_sample.r;
	}
	vec2 grid = uv * 32.0;
	vec2 cell = floor(grid);
	vec2 t = smoothstep(vec2(0.0), vec2(1.0), fract(grid));
	return mix(mix(noise_hash(cell), noise_hash(cell + vec2(1, 0)), t.x),
			mix(noise_hash(cell + vec2(0, 1)), noise_hash(cell + vec2(1, 1)), t.x), t.y);
}

// This same silhouette is used for the center and for every glow sample.
bool visible_at(vec2 uv) {
	if (any(lessThan(uv, vec2(0.0))) || any(greaterThan(uv, vec2(1.0)))) {
		return false;
	}
	if (mask_enabled && mask_progress < 1.0) {
		float coord = mask_axis == 0 ? uv.x : uv.y;
		float distance = mask_origin == 0 ? abs(2.0 * coord - 1.0)
				: (mask_origin == 1 ? coord : 1.0 - coord);
		if (distance > mask_progress) {
			return false;
		}
	}
	if (dissolve_enabled && dissolve_value < 1.0) {
		return dissolve_noise(uv) >= 1.0 - dissolve_value;
	}
	return true;
}

struct SourceMap {
	vec2 origin;
	mat2 axes;
	vec2 half_texel; // inset for atlas regions
};

vec2 source_uv(SourceMap source, vec2 uv) {
	return source.origin + source.axes * clamp(uv, source.half_texel, 1.0 - source.half_texel);
}

// Map the normalized effect rectangle into the source. Local pixels give a
// consistent blur radius when the node is resized, mirrored or rotated.
#ifdef CANVAS_GROUP_SOURCE
SourceMap map_group(vec2 quad_uv, vec2 screen_uv, out vec2 uv, out vec2 pixel_size) {
	uv = quad_uv;
	if (all(greaterThan(local_rect.zw, vec2(0.0)))) {
		uv = (original_position - local_rect.xy) / local_rect.zw;
	}
	pixel_size = fwidth(uv) / max(fwidth(original_position), vec2(0.000001));
	mat2 axes = mat2(dFdx(screen_uv), dFdy(screen_uv)) * inverse(mat2(dFdx(uv), dFdy(uv)));
	return SourceMap(screen_uv - axes * uv, axes, vec2(0.0));
}
#else
SourceMap map_texture(vec2 tex_uv, vec4 region, vec2 texel, out vec2 uv, out vec2 pixel_size) {
	vec2 size = max(abs(region.zw), texel);
	uv = (tex_uv - region.xy) / size;
	pixel_size = fwidth(uv) / max(fwidth(original_position), vec2(0.000001));
	uv += padding * sign(region.zw) * pixel_size;
	bool atlas = any(notEqual(size, vec2(1.0)));
	return SourceMap(region.xy, mat2(vec2(size.x, 0), vec2(0, size.y)),
			atlas ? 0.5 * texel / size : vec2(0.0));
}
#endif

#ifdef EFFECT_SOURCE
void fragment() {
	bool hidden = item_color.a <= 0.0 || base_tint.a <= 0.0 || (mask_enabled && mask_progress <= 0.0)
			|| (dissolve_enabled && dissolve_value <= 0.0);
	bool active = halo.x > 0.0 || (mask_enabled && mask_progress < 1.0)
			|| (dissolve_enabled && dissolve_value < 1.0) || (stretch_enabled && stretch_amount != 0.0);
	if (hidden) {
		COLOR = vec4(0.0);
	} else if (!active) {
#ifdef CANVAS_GROUP_SOURCE
		vec4 base = textureLod(EFFECT_SOURCE, SCREEN_UV, 0.0);
		base.rgb /= max(base.a, 0.00001);
		COLOR = base * item_color * base_tint;
#else
		COLOR *= base_tint;
#endif
	} else {
		vec2 uv;
		vec2 pixel_size;
#ifdef CANVAS_GROUP_SOURCE
		SourceMap source = map_group(UV, SCREEN_UV, uv, pixel_size);
#else
		SourceMap source = map_texture(UV, REGION_RECT, TEXTURE_PIXEL_SIZE, uv, pixel_size);
#endif
		vec2 p = vec2(0.5) + stretch_basis * (uv - vec2(0.5));
#ifndef CANVAS_GROUP_SOURCE
		vec2 sample_dx = source.axes * dFdx(p);
		vec2 sample_dy = source.axes * dFdy(p);
#endif
		vec4 base = vec4(0.0);
		if (visible_at(p)) {
#ifdef CANVAS_GROUP_SOURCE
			base = textureLod(EFFECT_SOURCE, source_uv(source, p), 0.0);
			base.rgb /= max(base.a, 0.00001);
#else
			base = textureGrad(EFFECT_SOURCE, source_uv(source, p), sample_dx, sample_dy);
#endif
			base *= base_tint;
		}
		if (halo.x > 0.0 && base.a < 1.0) {
			mat2 step_uv = stretch_basis * mat2(vec2(pixel_size.x, 0), vec2(0, pixel_size.y));
			// Rotate successive rings so a sparse kernel doesn't leave radial spokes.
			const float GOLDEN_ANGLE = 2.39996323;
			int rings = int(clamp(ceil(halo.x / 14.0), 3.0, 12.0));
			int directions = int(clamp(ceil(TAU * halo.x / 18.0), 12.0, 40.0));
			float angle = TAU / float(directions);
			mat2 turn = mat2(vec2(cos(angle), sin(angle)), vec2(-sin(angle), cos(angle)));
			float alpha_sum = 0.0;
			float weights = 0.0;
			for (int i = 1; i <= rings; i++) {
				float r = float(i) / float(rings);
				float weight = exp(-r * r / 0.72);
				float lod = 0.0;
#ifndef CANVAS_GROUP_SOURCE
				// Atlas mips can bleed from adjacent frames. The group variant
				// works with the default CanvasGroup setting (no backbuffer mips).
				if (all(equal(source.half_texel, vec2(0.0)))) {
					float cell = max(TAU * r * halo.x / float(directions), halo.x / float(rings));
					float texels = max(pixel_size.x / TEXTURE_PIXEL_SIZE.x, pixel_size.y / TEXTURE_PIXEL_SIZE.y);
					lod = max(log2(max(cell * texels, 1.0)) - 0.5, 0.0);
				}
#endif
				vec2 dir = vec2(cos(float(i) * GOLDEN_ANGLE), sin(float(i) * GOLDEN_ANGLE));
				float sum = 0.0;
				for (int j = 0; j < directions; j++) {
					vec2 neighbor = p + step_uv * (dir * r * halo.x);
					if (visible_at(neighbor)) {
						sum += textureLod(EFFECT_SOURCE, source_uv(source, neighbor), lod).a;
					}
					dir = turn * dir;
				}
				alpha_sum += sum * weight;
				weights += weight;
			}
			float blur = alpha_sum / (weights * float(directions));
			float behind = clamp(blur * base_tint.a * glow_color.a * halo.y, 0.0, 1.0) * (1.0 - base.a);
			float alpha = base.a + behind;
			base = vec4((base.rgb * base.a + glow_color.rgb * behind) / max(alpha, 0.00001), alpha);
		}
		COLOR = base * item_color;
	}
}
#endif
