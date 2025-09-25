shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 1.0;

void fragment() {
    COLOR = texture(TEXTURE, UV);
    float center = 0.5;
    float left_edge = center - (center * progress);
    float right_edge = center + (center * progress);
    if (UV.x < left_edge || UV.x > right_edge) {
        COLOR.a = 0.0;
    }
}