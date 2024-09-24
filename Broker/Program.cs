using MQTTnet;
using MQTTnet.Server;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class Program
{
    static async Task Main(string[] args)
    {
        // Настройки сервера MQTT для локальной работы (localhost)
        var optionsBuilder = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointBoundIPAddress(IPAddress.Any) // Используем 0.0.0.0 для доступа извне
            .WithDefaultEndpointPort(1883); // Указываем порт для MQTT-брокера

        // Создаем и конфигурируем MQTT-сервер
        var mqttServer = new MqttFactory().CreateMqttServer(optionsBuilder.Build());

        // Подписываемся на события подключения клиентов
        mqttServer.ClientConnectedAsync += e =>
        {
            Console.WriteLine($"Клиент подключен: {e.ClientId}");
            return Task.CompletedTask;
        };

        // Подписываемся на события отключения клиентов
        mqttServer.ClientDisconnectedAsync += e =>
        {
            Console.WriteLine($"Клиент отключен: {e.ClientId}");
            return Task.CompletedTask;
        };

        // Перехватываем публикацию сообщений от клиентов
        mqttServer.InterceptingPublishAsync += e =>
        {
            // Преобразуем байты сообщения в 16-ричный формат
            var hexString = BitConverter.ToString(e.ApplicationMessage.Payload).Replace("-", " ");
            Console.WriteLine($"Сообщение получено: Топик = {e.ApplicationMessage.Topic}, Сообщение (HEX) = {hexString}");
            return Task.CompletedTask;
        };

        // Запуск MQTT-сервера
        await mqttServer.StartAsync();

        // Выводим информацию о сетевых интерфейсах и IP-адресах
        Console.WriteLine("MQTT брокер запущен на следующих IP-адресах:");

        var hostName = Dns.GetHostName();
        var addresses = Dns.GetHostAddresses(hostName)
                            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        foreach (var ip in addresses)
        {
            Console.WriteLine($"- {ip}:1883");
        }

        Console.WriteLine("Сервер работает.");

        // Задержка для того, чтобы сервер оставался активным
        await Task.Delay(-1);

        // Остановка сервера (этот код никогда не выполнится, так как await Task.Delay(-1) не завершится)
        await mqttServer.StopAsync();
    }
}

