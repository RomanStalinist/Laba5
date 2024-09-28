using Grpc.Net.Client;
using Server.Protos;

// http://localhost:5237
// https://localhost:7097
using var channel = GrpcChannel.ForAddress("http://localhost:5000");
var client = new PetService.PetServiceClient(channel);

Console.WriteLine("""
                /*
                 *----------------------------------------------------
                 * Примеры выполнения:
                 *----------------------------------------------------
                 * /get 1
                 * /create Завертин,Вислоухий шотланд,Кошка,2022
                 *----------------------------------------------------
                 */

                """);

static bool IsRequestValid(string request)
{
    try
    {
        var parts = request.Split(' ');
        if (parts.Length == 1 && request is "/quit" or "/clear") return true;
        if (parts.Length != 2) throw new Exception("Запрос должен содержать команду и параметр");

        var command = parts[0].Trim();
        var parameter = parts[1].Trim();
        string[] supportedCommands = ["/get", "/create"];
        if (!supportedCommands.Contains(command)) throw new NotSupportedException($"{command} не поддерживается");

        switch (command)
        {
            case "/get":
                if (!uint.TryParse(parameter, out var id)) throw new InvalidCastException($"{parameter} не является положительным числом");
                break;

            case "/create":
                var createParts = parameter.Split(',');
                if (createParts.Length != 5)
                    throw new Exception("Команда /create требует 5 параметров: код,имя,порода,тип,год рождения");
                break;
        }

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex}\r\n");
        return false;
    }    
}

while (true)
{
    try
    {
        var input = Console.ReadLine()!;
        if (!IsRequestValid(input)) continue;
        
        if (input == "/quit")
            break;

        if (input == "/clear")
        {
            Console.Clear();
            continue;
        }

        if (input.StartsWith("/get"))
        {
            var parts = input.Split(' ');
            var parameter = parts[1];
            if (!uint.TryParse(parts[1], out var id)) throw new InvalidCastException($"{parts[1]} не является положительным числом");
            var response = await client.GetPetAsync(new GetPetRequest { Id = id });
            Console.WriteLine($"{response.Pet?.ToString() ?? "питомец не найден"}\r\n");
        }
        else if (input.StartsWith("/create"))
        {
            var parts = input.Split(' ');
            var parameters = parts[1].Split(',');

            if (!uint.TryParse(parameters[0], out var id))
                throw new InvalidCastException($"{parameters[0]} не возможно преобразовать в код");

            if (!uint.TryParse(parameters[4], out var yearOfBirth))
                throw new InvalidCastException($"{parameters[4]} не возможно преобразовать в год рождения");

            if ((await client.GetPetAsync(new GetPetRequest { Id = id })).Pet is not null)
                throw new ArgumentException($"Код {id} уже занят", null, null);

            Pet newPet = new()
            {
                Id = id,
                Name = parameters[1],
                Breed = parameters[2],
                Animal = parameters[3],
                YearOfBirth = yearOfBirth
            };

            // Создание питомца
            var request = new CreatePetRequest
            {
                Pet = newPet
            };

            var response = await client.PostPetAsync(request);
            Console.WriteLine($"Создан питомец: {newPet}\r\n");
        }
        else throw new NotSupportedException($"Действие {input} не поддерживается");        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{ex}\r\n");
    }
}

Console.ReadKey();
