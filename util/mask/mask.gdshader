shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 1.0;

/*
axis:
0 = horizontal (X)
1 = vertical (Y)
*/
uniform int axis = 0;

/*
origin:
0 = center
1 = left (X) / top (Y)
2 = right (X) / bottom (Y)
*/
uniform int origin = 0;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    float coord = (axis == 0) ? UV.x : UV.y;

    float alpha = 1.0;

    if (origin == 0) {
        // FROM CENTER
        float center = 0.5;
        float half_size = 0.5 * progress;
        if (coord < center - half_size || coord > center + half_size) {
            alpha = 0.0;
        }
    } else if (origin == 1) {
        // FROM LEFT or TOP
        if (coord > progress) {
            alpha = 0.0;
        }
    } else if (origin == 2) {
        // FROM RIGHT or BOTTOM
        if (coord < 1.0 - progress) {
            alpha = 0.0;
        }
    }

    COLOR = vec4(tex.rgb, tex.a * alpha);
}
