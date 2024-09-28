using Grpc.Core;
using Newtonsoft.Json;
using Server.Protos;

namespace Server.Services;

public class PetsService : PetService.PetServiceBase
{
    private const string jsonFilePath = "pets.json";

    // Метод для получения питомца
    public override async Task<GetPetResponse> GetPet(GetPetRequest request, ServerCallContext context)
    {
        if (!File.Exists(jsonFilePath)) return new GetPetResponse
        {
            Pet = null
        };

        var jsonData = await File.ReadAllTextAsync(jsonFilePath);
        var pets = JsonConvert.DeserializeObject<List<Pet>>(jsonData);
        var pet = pets?.FirstOrDefault(p => p.Id == request.Id);
        return new GetPetResponse
        {
            Pet = pet
        };
    }

    // Метод для добавления или обновления питомца
    public override async Task<Google.Protobuf.WellKnownTypes.Empty> PostPet(CreatePetRequest request, ServerCallContext context)
    {
        List<Pet> pets = [];

        if (File.Exists(jsonFilePath))
        {
            var jsonData = await File.ReadAllTextAsync(jsonFilePath);
            pets = JsonConvert.DeserializeObject<List<Pet>>(jsonData) ?? [];
        }

        // Обновление или добавление питомца
        var existingPet = pets?.FirstOrDefault(p => p.Id == request.Pet.Id);

        if (existingPet is null) pets?.Add(request.Pet);
        else
        {
            existingPet.Name = request.Pet.Name;
            existingPet.Breed = request.Pet.Breed;
            existingPet.Animal = request.Pet.Animal;
            existingPet.YearOfBirth = request.Pet.YearOfBirth;
        }

        await File.WriteAllTextAsync(jsonFilePath, JsonConvert.SerializeObject(pets));
        return new Google.Protobuf.WellKnownTypes.Empty();
    }
}
