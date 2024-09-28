namespace Server.Models;

public class Pet
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Animal { get; set; } = string.Empty;
    public uint YearOfBirth { get; set; }
    public override string ToString() => $"Id={Id}\r\nИмя: {Name}\r\nПорода: {Breed}\r\nЖивотное: {Animal}\r\nГод рождения: {YearOfBirth}";
}
