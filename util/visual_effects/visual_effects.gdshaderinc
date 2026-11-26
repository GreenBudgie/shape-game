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
varying flat vec4 effect_bounds; // min/max UV after applying the mask

vec2 glow_parameters(float time) {
	if (!glow_enabled || glow_color.a <= 0.0) {
		return vec2(0.0);
	}
	float pulse = 0.0;
	if (pulse_enabled) {
		pulse = 0.5 + 0.5 * sin(time * TAU * pulses_per_second);
	}
	vec2 result = vec2(glow_radius, glow_strength) + pulse * vec2(pulse_radius_delta, pulse_strength_delta);
	return (result.x >= 0.5 && result.y >= 0.01) ? result : vec2(0.0);
}

mat2 stretch_transform() {
	vec2 axis = stretch_direction / max(length(stretch_direction), 0.00001);
	return mat2(1.0) + mat2(axis * axis.x, axis * axis.y) * stretch_amount;
}

vec4 mask_bounds() {
	vec4 bounds = vec4(0.0, 0.0, 1.0, 1.0);
	if (mask_enabled && mask_progress < 1.0) {
		float start = mask_origin == 0 ? (1.0 - mask_progress) * 0.5
				: (mask_origin == 1 ? 0.0 : 1.0 - mask_progress);
		if (mask_axis == 0) {
			bounds.xz = vec2(start, start + mask_progress);
		} else {
			bounds.yw = vec2(start, start + mask_progress);
		}
	}
	return bounds;
}

// Standalone include validation has no canvas_item context for the entry points.
#ifdef EFFECT_SOURCE
void vertex() {
	item_color = COLOR;
	original_position = VERTEX;
	halo = glow_parameters(TIME);
	effect_bounds = mask_bounds();
	stretch_basis = stretch_enabled && stretch_amount != 0.0 ? stretch_transform() : mat2(1.0);
#ifndef CANVAS_GROUP_SOURCE
	padding = vec2(0.0);
	if (halo.x > 0.0) {
		// Godot's indexed and six-vertex canvas rectangles use this corner order.
		const vec2 corners[6] = {vec2(-1,-1), vec2(-1,1), vec2(1,1), vec2(1,-1), vec2(-1,-1), vec2(1,1)};
		vec2 corner = corners[VERTEX_ID % 6];
		if (all(greaterThan(local_rect.zw, vec2(0.0)))) {
			corner = sign(VERTEX - local_rect.xy - local_rect.zw * 0.5);
		}
		padding = corner * halo.x;
		VERTEX += padding;
	}
#endif
}
#endif

float noise_hash(vec2 cell) {
	vec3 p = fract(cell.xyx * vec3(0.1031, 0.1030, 0.0973));
	p += dot(p, p.yzx + 33.33);
	return fract((p.x + p.y) * p.z);
}

// The sprite keeps a sharp dissolve edge. For glow, estimate how much of a
// sampling cell survives the threshold so a whole cell does not blink on/off.
float dissolve_coverage(vec2 uv, vec2 footprint, bool filter_cell) {
	vec4 sample_value = textureLod(dissolve_texture, uv, 0.0);
	float value = sample_value.r;
	float width = 0.0;
	if (sample_value.a > 0.0) {
		if (filter_cell) {
			vec4 sample_x = textureLod(dissolve_texture, uv + vec2(footprint.x * 0.5, 0.0), 0.0);
			vec4 sample_y = textureLod(dissolve_texture, uv + vec2(0.0, footprint.y * 0.5), 0.0);
			width = 2.0 * (abs(sample_x.r - value) * step(0.00001, sample_x.a) + abs(sample_y.r - value) * step(0.00001, sample_y.a));
		}
	} else {
		vec2 grid = uv * 32.0;
		vec2 cell = floor(grid);
		vec2 f = fract(grid);
		vec2 t = f * f * (3.0 - 2.0 * f);
		float a = noise_hash(cell);
		float b = noise_hash(cell + vec2(1, 0));
		float c = noise_hash(cell + vec2(0, 1));
		float d = noise_hash(cell + vec2(1, 1));
		value = mix(mix(a, b, t.x), mix(c, d, t.x), t.y);
		if (filter_cell) {
			vec2 slope = vec2(mix(b - a, d - c, t.y), mix(c - a, d - b, t.x));
			// Chain rule for smoothstep interpolation on the 32-cell noise grid.
			width = dot(abs(slope) * (6.0 * f * (1.0 - f)), footprint * 32.0);
		}
	}
	float threshold = 1.0 - dissolve_value;
	if (!filter_cell || width <= 0.00001) {
		return step(threshold, value);
	}
	vec2 range = clamp(value + vec2(-0.5, 0.5) * width, vec2(0.0), vec2(1.0));
	return range.y > range.x ? clamp((range.y - threshold) / (range.y - range.x), 0.0, 1.0) : step(threshold, value);
}

bool visible_at(vec2 uv, bool dissolved) {
	return all(greaterThanEqual(uv, effect_bounds.xy)) && all(lessThanEqual(uv, effect_bounds.zw))
			&& (!dissolved || dissolve_coverage(uv, vec2(0.0), false) > 0.0);
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
vec2 group_uv(vec2 quad_uv) {
	return all(greaterThan(local_rect.zw, vec2(0.0))) ? (original_position - local_rect.xy) / local_rect.zw : quad_uv;
}

SourceMap map_group(vec2 uv, vec2 screen_uv, out vec2 pixel_size) {
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

// Neighboring output pixels share samples anchored to the source image.
// The kernel weight falls smoothly to zero at the edge of the glow.
float integrate_glow(sampler2D image, SourceMap source, vec2 p, vec2 texel, bool dissolved,
		vec2 cell, vec2 uv_min, vec2 uv_max, mat2 to_kernel) {
	float lod = 0.0;
#ifndef CANVAS_GROUP_SOURCE
	// Atlas mip levels can mix adjacent frames. CanvasGroup uses its base level.
	if (all(equal(source.half_texel, vec2(0.0)))) {
		vec2 footprint = abs(source.axes[0] * cell.x) + abs(source.axes[1] * cell.y);
		vec2 texels = footprint / texel;
		lod = max(log2(max(texels.x, texels.y)) - 0.5, 0.0);
	}
#endif
	vec2 lower = effect_bounds.xy / cell;
	vec2 upper = effect_bounds.zw / cell;
	vec2 first = uv_min / cell;
	vec2 last = uv_max / cell;
	vec2 center = p / cell;
	mat2 step_in_kernel = mat2(to_kernel[0] * cell.x, to_kernel[1] * cell.y);
	float sum = 0.0;
	// Clip the boundary cells to the visible rectangle, including a partial mask.
	for (float y = floor(first.y); y < last.y; y += 1.0) {
		vec2 span_y = vec2(max(y, lower.y), min(y + 1.0, upper.y));
		float mid_y = (span_y.x + span_y.y) * 0.5;
		vec2 dy = step_in_kernel[1] * (mid_y - center.y);
		float stagger = fract(y * 0.5); // offset alternate rows by half a cell
		float row = 0.0;
		for (float x = floor(first.x - stagger) + stagger; x < last.x; x += 1.0) {
			vec2 span_x = vec2(max(x, lower.x), min(x + 1.0, upper.x));
			float mid_x = (span_x.x + span_x.y) * 0.5;
			vec2 d = dy + step_in_kernel[0] * (mid_x - center.x);
			float weight = 1.0 - dot(d, d);
			if (weight > 0.0) {
				vec2 uv = vec2(mid_x, mid_y) * cell;
				float alpha = textureLod(image, source_uv(source, uv), lod).a;
				if (dissolved && alpha > 0.0) {
					alpha *= dissolve_coverage(uv, cell * vec2(span_x.y - span_x.x, span_y.y - span_y.x), true);
				}
				row += alpha * weight * weight * (span_x.y - span_x.x);
			}
		}
		sum += row * (span_y.y - span_y.x);
	}
	// Integral of (1 - r*r)^2 over the unit disk is PI / 3.
	return sum * (3.0 / PI) * abs(determinant(step_in_kernel));
}

float glow_blur(sampler2D image, SourceMap source, vec2 p, vec2 pixel_size, vec2 texel, bool stretched, bool dissolved) {
	vec2 radius = max(halo.x * pixel_size, vec2(0.00001));
	mat2 kernel = mat2(vec2(radius.x, 0.0), vec2(0.0, radius.y));
	vec2 reach = radius;
	if (stretched) {
		kernel = stretch_basis * kernel;
		reach = sqrt(kernel[0] * kernel[0] + kernel[1] * kernel[1]);
	}
	vec2 uv_min = max(p - reach, effect_bounds.xy);
	vec2 uv_max = min(p + reach, effect_bounds.zw);
	if (any(lessThanEqual(uv_max, uv_min))) {
		return 0.0;
	}
	mat2 to_kernel = stretched ? inverse(kernel)
			: mat2(vec2(1.0 / radius.x, 0.0), vec2(0.0, 1.0 / radius.y));
	// Fixed sample pitches keep the halo stable while its radius is tweened.
	// Crossfade adjacent grids only during the transition between pitches.
	float desired = halo.x / clamp(halo.x / 12.0, 2.0, 9.0);
	float fine = 3.0 * exp2(floor(log2(desired / 3.0)));
	float coarse = fine * 2.0;
	float blend = smoothstep(0.15, 0.50, (desired - fine) / (coarse - fine));
	vec2 uv_per_pixel = reach / halo.x;
	// Resolve at least twelve cells across small sprites, even at large radii.
	vec2 fine_cell = min(uv_per_pixel * fine, vec2(1.0 / 12.0));
	vec2 coarse_cell = min(uv_per_pixel * coarse, vec2(1.0 / 12.0));
	bool transition = blend > 0.0 && blend < 1.0 && any(notEqual(fine_cell, coarse_cell));
	float result = 0.0;
	for (int i = 0; i < (transition ? 2 : 1); i++) {
		bool use_coarse = blend >= 1.0 || i == 1;
		float sample_alpha = integrate_glow(image, source, p, texel, dissolved,
				use_coarse ? coarse_cell : fine_cell, uv_min, uv_max, to_kernel);
		result += transition ? sample_alpha * (use_coarse ? blend : 1.0 - blend) : sample_alpha;
	}
	return result;
}

#ifdef EFFECT_SOURCE
void fragment() {
	bool hidden = item_color.a <= 0.0 || base_tint.a <= 0.0 || (mask_enabled && mask_progress <= 0.0)
			|| (dissolve_enabled && dissolve_value <= 0.0);
	bool dissolved = dissolve_enabled && dissolve_value < 1.0;
	bool stretched = stretch_enabled && stretch_amount != 0.0;
	bool masked = mask_enabled && mask_progress < 1.0;
	bool remap = halo.x > 0.0 || stretched;
#ifndef CANVAS_GROUP_SOURCE
	// Keep masked/dissolved atlas frames inset from adjacent texels.
	remap = remap || ((masked || dissolved) && any(notEqual(abs(REGION_RECT.zw), vec2(1.0))));
#endif
	if (hidden) {
		COLOR = vec4(0.0);
	} else if (!remap) {
		bool visible = true;
		if (masked || dissolved) {
#ifdef CANVAS_GROUP_SOURCE
			visible = visible_at(group_uv(UV), dissolved);
#else
			visible = visible_at((UV - REGION_RECT.xy) / max(abs(REGION_RECT.zw), TEXTURE_PIXEL_SIZE), dissolved);
#endif
		}
		if (visible) {
#ifdef CANVAS_GROUP_SOURCE
			vec4 base = textureLod(EFFECT_SOURCE, SCREEN_UV, 0.0);
			base.rgb /= max(base.a, 0.00001);
			COLOR = base * item_color * base_tint;
#else
			COLOR *= base_tint;
#endif
		} else {
			COLOR = vec4(0.0);
		}
	} else {
		vec2 uv;
		vec2 pixel_size;
#ifdef CANVAS_GROUP_SOURCE
		uv = group_uv(UV);
		SourceMap source = map_group(uv, SCREEN_UV, pixel_size);
#else
		SourceMap source = map_texture(UV, REGION_RECT, TEXTURE_PIXEL_SIZE, uv, pixel_size);
#endif
		vec2 p = stretched ? vec2(0.5) + stretch_basis * (uv - vec2(0.5)) : uv;
#ifndef CANVAS_GROUP_SOURCE
		vec2 sample_dx = source.axes * dFdx(p);
		vec2 sample_dy = source.axes * dFdy(p);
#endif
		vec4 base = vec4(0.0);
		if (visible_at(p, dissolved)) {
#ifdef CANVAS_GROUP_SOURCE
			base = textureLod(EFFECT_SOURCE, source_uv(source, p), 0.0);
			base.rgb /= max(base.a, 0.00001);
#else
			base = textureGrad(EFFECT_SOURCE, source_uv(source, p), sample_dx, sample_dy);
#endif
			base *= base_tint;
		}
		if (halo.x > 0.0 && base.a < 1.0) {
			float blur = glow_blur(EFFECT_SOURCE, source, p, pixel_size, TEXTURE_PIXEL_SIZE, stretched, dissolved);
			float behind = clamp(blur * base_tint.a * glow_color.a * halo.y, 0.0, 1.0) * (1.0 - base.a);
			float alpha = base.a + behind;
			base = vec4((base.rgb * base.a + glow_color.rgb * behind) / max(alpha, 0.00001), alpha);
		}
		COLOR = base * item_color;
	}
}
#endif
