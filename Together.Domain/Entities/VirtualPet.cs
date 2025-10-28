using Together.Domain.Enums;

namespace Together.Domain.Entities;

public class VirtualPet
{
    public Guid Id { get; private set; }
    public Guid ConnectionId { get; private set; }
    public string? Name { get; private set; }
    public int Level { get; private set; }
    public int ExperiencePoints { get; private set; }
    public string? AppearanceOptions { get; private set; }
    public PetState State { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public CoupleConnection Connection { get; private set; } = null!;

    private VirtualPet() { }

    public VirtualPet(Guid connectionId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        ConnectionId = connectionId;
        Name = name;
        Level = 1;
        ExperiencePoints = 0;
        State = PetState.Happy;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddExperience(int points)
    {
        if (points < 0)
            throw new ArgumentException("Experience points cannot be negative", nameof(points));

        ExperiencePoints += points;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        int requiredXp = Level * 100;
        while (ExperiencePoints >= requiredXp)
        {
            Level++;
            ExperiencePoints -= requiredXp;
            requiredXp = Level * 100;
        }
    }

    public void UpdateState(PetState newState)
    {
        State = newState;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));

        Name = newName;
    }

    public void UpdateAppearance(string appearanceOptions)
    {
        AppearanceOptions = appearanceOptions;
    }
}
