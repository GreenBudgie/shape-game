shader_type canvas_item;
render_mode unshaded;

uniform sampler2D group_image : hint_screen_texture, repeat_disable, filter_linear;
#define CANVAS_GROUP_SOURCE
#define EFFECT_SOURCE group_image
#include "visual_effects.gdshaderinc"
