shader_type canvas_item;
render_mode blend_add, unshaded; // Remove this if you want to use HDR glow instead (use "Glow" and "Outline Glow" sliders)

uniform int beams = 2; // How many beams the energy field should have

uniform float energy = 3.0; // How much the beams will travel up and down
uniform int roughness : hint_range(1, 10) = 3; // How compact the noise texture will be
uniform int frequency = 3; // Amount of "ripples" in the beams

uniform float speed = 1.0; // Animation speed
uniform float thickness : hint_range(0.0, 0.1) = 0.0045; // Thickness of the main beam
uniform float outline_thickness : hint_range(0.0, 0.1) = 0.023; //Thickness of the outline color
uniform float beam_difference : hint_range(0.0, 1.0) = 0.0; // The thickness difference between the main beam and the other, if there are more than one beam. The closer to 1 the smaller the thickness difference.

uniform vec4 color : source_color = vec4(0.91, 1.0, 1.0, 1.0);
uniform vec4 outline_color : source_color = vec4(0.5, 1.0, 0.96, 1.0);

uniform float progress : hint_range(0.0, 1.0) = 1.0;

uniform float y_offset : hint_range (-0.5, 0.5) = 0.0; // Position of the beam
uniform float fixed_edge_size : hint_range(0.0, 0.5) = 0.05; // How close to the edge should the beam be still before the animatino starts
uniform vec2 noise_scale = vec2(1.0); // If the object (for example the ColorRect or Sprite node) is compressed use this to compensate for the noise texture being compressed.

// NEW UNIFORMS
uniform float beam_length : hint_range(0.0, 1.0) = 1.0; // Controls how much of the ColorRect the beam covers (0 = no beam, 1 = full width)
uniform float fade_distance : hint_range(0.0, 0.5) = 0; // Distance over which the beam fades in/out at start and end
uniform float corner_roundness : hint_range(0.0, 1.0) = 0.5; // How rounded the beam corners are (0 = sharp, 1 = very rounded)

float random(vec2 uv) {
   return fract(sin(dot(uv.xy,
       vec2(12.9898,78.233))) *
           43758.5453123);
}

vec2 randomVec2(vec2 uv){
   uv = vec2( dot(uv, vec2(127.1,311.7) ),
              dot(uv, vec2(269.5,183.3) ) );
   return -1.0 + 2.0 * fract(sin(uv) * 43758.5453123);
}

float noise(vec2 uv) {
   vec2 uv_index = floor(uv);
   vec2 uv_fract = fract(uv);
   vec2 blur = smoothstep(0.0, 1.0, uv_fract);
   return mix( mix( dot( randomVec2(uv_index + vec2(0.0,0.0) ), uv_fract - vec2(0.0,0.0) ),
                    dot( randomVec2(uv_index + vec2(1.0,0.0) ), uv_fract - vec2(1.0,0.0) ), blur.x),
               mix( dot( randomVec2(uv_index + vec2(0.0,1.0) ), uv_fract - vec2(0.0,1.0) ),
                    dot( randomVec2(uv_index + vec2(1.0,1.0) ), uv_fract - vec2(1.0,1.0) ), blur.x), blur.y) + 0.5;
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

// NEW FUNCTION: Calculate fade and rounding mask (left-aligned beam)
float calculateBeamMask(vec2 uv) {
    // Calculate beam boundaries - beam starts at 0 and extends by beam_length
    float beam_start = 0.0;
    float beam_end = beam_length;
    
    // If beam_length is 0, return 0 (no beam)
    if (beam_length <= 0.0) {
        return 0.0;
    }
    
    // Check if we're outside the beam area
    if (uv.x < beam_start || uv.x > beam_end) {
        return 0.0;
    }
    
    // Calculate fade mask for start and end
    float fade_mask = 1.0;
    
    // Fade in at the start
    if (uv.x < beam_start + fade_distance) {
        fade_mask *= smoothstep(beam_start, beam_start + fade_distance, uv.x);
    }
    
    // Fade out at the end
    if (uv.x > beam_end - fade_distance) {
        fade_mask *= smoothstep(beam_end, beam_end - fade_distance, uv.x);
    }
    
    // Add corner rounding by considering distance from center in Y direction
    float y_center = 0.5 + y_offset;
    float y_distance = abs(uv.y - y_center);
    
    // Create rounded corners effect
    float corner_fade = 1.0;
    if (corner_roundness > 0.0) {
        // Calculate distance from corners
        float corner_x_dist = min(uv.x - beam_start, beam_end - uv.x);
        float normalized_corner_dist = corner_x_dist / (fade_distance + 0.001); // Avoid division by zero
        
        // Apply rounding based on Y distance and corner proximity
        float rounding_factor = corner_roundness * (1.0 - clamp(normalized_corner_dist, 0.0, 1.0));
        corner_fade = smoothstep(rounding_factor * 0.3, 0.0, y_distance - 0.1);
        corner_fade = max(corner_fade, 1.0 - rounding_factor);
    }
    
    return fade_mask * corner_fade;
}

vec4 bolt(vec2 uv, float time, float i)
{
   // Setup the beam locking to the edges.
   float falloff = smoothstep(0.0, fixed_edge_size, uv.x) * smoothstep(0.0, fixed_edge_size, 1.0 - uv.x);

   // Use Fractal Brownian Motion to create a "cloud texture" and use Difference blend mode to make the beam
   vec4 clouds = vec4(fbm((uv + vec2(i) ) * noise_scale, time * speed)) * falloff;
   vec4 diff_clouds = difference(clouds, vec4(uv.y - 0.5 + y_offset + (uv.y * falloff * 0.02 * energy * progress)));

   // Create a new noise to mask the beams on low "progress" values. To make a "turn-off" effect more visually interesting.
   vec4 clouds2 = vec4(fbm((uv * 2.0) * noise_scale, time * 1.)) * 5.0;
   diff_clouds += smoothstep(0.0, 0.8, clouds2) * 0.1 * (1.-progress);

   // Set thickness of the beams. First beam is the Thickness size and all following beams are sized with beam_difference
   float thickness2 = 1. - ( thickness / (min(i + beam_difference, 1.0) + (1.0-beam_difference))) * progress ;
   vec4 beam = clamp(smoothstep(thickness2, thickness2 + 0.005 * progress, 1.0 - diff_clouds), vec4(0.0), vec4(1.0));

   //Set the beam outlines
   vec4 beam_outline;
   float outline = thickness2 - (outline_thickness * progress);
   beam_outline = clamp(smoothstep(outline, outline + 0.04, 1.0 - diff_clouds), 0.0, 1.0);
   beam_outline = clamp(beam_outline - beam, 0.0, 1.0);

   // Apply beam mask for length control and fading/rounding
   float beam_mask = calculateBeamMask(uv);

   // Merge the beam and the outline and return to the fragment function
   return (beam * color + beam_outline * outline_color) * beam_mask;
}

void fragment()
{   
   vec4 beam = vec4(0.0);

   for (int i = 0; i < beams; i++){
      beam = max(beam, bolt(UV, TIME, float(i)));
   }

   COLOR = vec4(beam.rgb, progress);
}