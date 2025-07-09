public static class SoundUtils
{
    
    public static AudioStreamPlayer RandomizePitch(this AudioStreamPlayer audio, float minPitch, float maxPitch)
    {
        audio.PitchScale = (float)GD.RandRange(minPitch, maxPitch);
        return audio;
    }
    
    public static AudioStreamPlayer RandomizePitchOffset(this AudioStreamPlayer audio, float minMaxAmount)
    {
        audio.PitchScale = (float)GD.RandRange(audio.PitchScale - minMaxAmount, audio.PitchScale + minMaxAmount);
        return audio;
    }
    
    public static AudioStreamPlayer2D RandomizePitch(this AudioStreamPlayer2D audio, float minPitch, float maxPitch)
    {
        audio.PitchScale = (float)GD.RandRange(minPitch, maxPitch);
        return audio;
    }
    
    public static AudioStreamPlayer2D RandomizePitchOffset(this AudioStreamPlayer2D audio, float minMaxAmount)
    {
        audio.PitchScale = (float)GD.RandRange(audio.PitchScale - minMaxAmount, audio.PitchScale + minMaxAmount);
        return audio;
    }
    
}