using Gallop;
using System;

[System.Serializable]
public class SerializableMorph
{
    public string Name = "";
    public float Value = 0;

    /// <summary> Reference to original morph for runtime. Not saved to file. </summary>
    [NonSerialized] public FacialMorph Morph;

    public SerializableMorph() { }

    public SerializableMorph(FacialMorph morph)
    {
        Name = morph.name;
        Value = morph.weight;

        Morph = morph;
    }

    public void ApplyTo(UmaContainerCharacter character, FacialMorph morph, bool applyImmediately = true)
    {
        if (applyImmediately)
        {
            character.FaceDrivenKeyTarget.ChangeMorphWeight(morph, Value);
        }
        else
        {
            morph.weight = Value;
        }
    }
}
