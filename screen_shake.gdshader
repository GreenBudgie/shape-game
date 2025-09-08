shader_type canvas_item;

uniform float strength = 0;
uniform vec2 magnitude = vec2(0.0);
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
    vec2 uv = SCREEN_UV;
	uv -= 0.5;
    uv *= 1.0 - 2.0 * magnitude.x;
    uv += 0.5;
	vec2 dt = vec2(0.0, 0.0);
	dt.x = sin(TIME * 100.0) * magnitude.x;
	dt.y = cos(TIME * 100.0) * magnitude.y;
	COLOR = texture(SCREEN_TEXTURE, uv + (dt * strength));
}


