shader_type canvas_item;
render_mode blend_add, unshaded;

uniform int beams = 2; // How many beams the energy field should have
uniform float energy = 3.0; // How much the beams will travel up and down
uniform int roughness : hint_range(1, 10) = 3; // How compact the noise texture will be
uniform int frequency = 10; // Amount of "ripples" in the beams
uniform float speed = 1.0; // Animation speed
// Beam appearance
uniform float beam_core_width : hint_range(0.01, 0.5) = 0.05; // Main beam thickness (0-1, relative to perpendicular screen size)
uniform float beam_outline_width : hint_range(0.0, 0.5) = 0.15; // Outline thickness (0-1, relative to perpendicular screen size)
uniform float beam_difference : hint_range(0.0, 1.0) = 0.0; // Thickness difference between multiple beams
uniform float glow : hint_range(0.0, 3.0) = 0.0; // Use together with WorldEnvironment's Glow feature
uniform float outline_glow : hint_range(0.0, 3.0) = 0.0;
uniform vec4 color : source_color = vec4(0.91, 1.0, 1.0, 1.0);
uniform vec4 outline_color : source_color = vec4(0.5, 1.0, 0.96, 1.0);

// Beam animation and fading
uniform float progress : hint_range(0.0, 1.0) = 1.0;
uniform float edge_fade_size : hint_range(0.0, 0.5) = 0.05; // Fade distance from beam edges
uniform float end_fade_size : hint_range(0.0, 0.5) = 0.1; // Rounded fade at start/end of beam

// Beam positioning
uniform vec2 start_point = vec2(0.1, 0.5); // Start position in UV coordinates (0-1)
uniform vec2 end_point = vec2(0.9, 0.5);   // End position in UV coordinates (0-1)

float random(vec2 uv) {
    return fract(sin(dot(uv.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

vec2 randomVec2(vec2 uv){
    uv = vec2( dot(uv, vec2(127.1,311.7) ), dot(uv, vec2(269.5,183.3) ) );
    return -1.0 + 2.0 * fract(sin(uv) * 43758.5453123);
}

float noise(vec2 uv) {
    vec2 uv_index = floor(uv);
    vec2 uv_fract = fract(uv);
    vec2 blur = smoothstep(0.0, 1.0, uv_fract);
    return mix(
        mix( dot( randomVec2(uv_index + vec2(0.0,0.0) ), uv_fract - vec2(0.0,0.0) ),
             dot( randomVec2(uv_index + vec2(1.0,0.0) ), uv_fract - vec2(1.0,0.0) ), blur.x),
        mix( dot( randomVec2(uv_index + vec2(0.0,1.0) ), uv_fract - vec2(0.0,1.0) ),
             dot( randomVec2(uv_index + vec2(1.0,1.0) ), uv_fract - vec2(1.0,1.0) ), blur.x),
        blur.y) + 0.5;
}

float fbm(vec2 uv, float time) {
    int octaves = roughness;
    float amp = 0.01 * energy * progress;
    float freq = float(frequency);
    float value = 0.0;
    
    for(int i = 0; i < octaves; i++) {
        value += amp * noise(freq * vec2(uv.x, uv.y + time));
        amp *= 0.5;
        freq *= 2.0;
    }
    return value;
}

vec4 difference(vec4 base, vec4 blend){
    return abs(base - blend);
}

// Transform UV coordinates from screen space to beam-aligned space
vec2 transformToBeamSpace(vec2 uv) {
    // Calculate beam direction and length
    vec2 beam_dir = end_point - start_point;
    float beam_length = length(beam_dir);
    vec2 beam_normal = normalize(vec2(-beam_dir.y, beam_dir.x));
    vec2 beam_tangent = normalize(beam_dir);
    
    // Translate UV to start point
    vec2 translated_uv = uv - start_point;
    
    // Project onto beam coordinate system
    float along_beam = dot(translated_uv, beam_tangent) / beam_length;
    float across_beam = dot(translated_uv, beam_normal);
    
    // Scale the perpendicular distance to maintain proper thickness
    // This ensures the beam thickness is relative to the shorter screen dimension
    across_beam = across_beam / (beam_length * 0.5);
    
    return vec2(along_beam, across_beam + 0.5);
}

vec4 bolt(vec2 uv, float time, float i) {
    // Transform UV to beam-aligned coordinate system
    vec2 beam_uv = transformToBeamSpace(uv);
    
    // Check if we're within the beam bounds
    if (beam_uv.x < 0.0 || beam_uv.x > 1.0) {
        return vec4(0.0);
    }
    
    // Create smooth falloff at the start and end of the beam (rounded caps)
    float end_falloff = smoothstep(0.0, end_fade_size, beam_uv.x) * 
                        smoothstep(0.0, end_fade_size, 1.0 - beam_uv.x);
    
    // Setup the beam edge falloff (keeps the beam stable near the edges)
    float edge_falloff = smoothstep(0.0, edge_fade_size, beam_uv.x) * 
                         smoothstep(0.0, edge_fade_size, 1.0 - beam_uv.x);
    
    // Use the beam-aligned UV coordinates for noise
    vec4 clouds = vec4(fbm((beam_uv + vec2(i, 0.0)) * vec2(1.0, 2.0), time * speed)) * edge_falloff;
    vec4 diff_clouds = difference(clouds, vec4(beam_uv.y - 0.5 + (beam_uv.y * edge_falloff * 0.02 * energy * progress)));
    
    // Create a new noise to mask the beams on low "progress" values
    vec4 clouds2 = vec4(fbm(beam_uv * 2.0, time * 1.0)) * 5.0;
    diff_clouds += smoothstep(0.0, 0.8, clouds2) * 0.1 * (1.0 - progress);
    
    // Calculate beam core thickness (adjusted for multiple beams)
    float core_thickness_adjusted = beam_core_width / (min(i + beam_difference, 1.0) + (1.0 - beam_difference));
    float core_threshold = 1.0 - core_thickness_adjusted * progress;
    vec4 beam_core = clamp(smoothstep(core_threshold, core_threshold + 0.01 * progress, 1.0 - diff_clouds), vec4(0.0), vec4(1.0));
    
    // Calculate beam outline
    float outline_threshold = 1.0 - beam_outline_width * progress;
    vec4 beam_outline = clamp(smoothstep(outline_threshold, outline_threshold + 0.02, 1.0 - diff_clouds), 0.0, 1.0);
    beam_outline = clamp(beam_outline - beam_core, 0.0, 1.0);
    
    // Apply end fade to both core and outline
    beam_core *= end_falloff;
    beam_outline *= end_falloff;
    
    // Merge the beam core and outline
    return (beam_core * (color + vec4(glow, glow, glow, 0.0))) + 
           (beam_outline * (outline_color + vec4(outline_glow, outline_glow, outline_glow, 0.0)));
}

void fragment() {
    vec4 beam = vec4(0.0);
    
    for (int i = 0; i < beams; i++){
        beam = max(beam, bolt(UV, TIME, float(i)));
    }
    
    COLOR = beam;
}