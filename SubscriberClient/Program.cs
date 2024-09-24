using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Настройки клиента
        var options = new MqttClientOptionsBuilder()
            .WithClientId("PublisherClient") // Идентификатор клиента
            .WithTcpServer("localhost", 1883) // Адрес и порт брокера MQTT
            .WithCleanSession()
            .Build();

        // Создаем клиента MQTT
        var mqttClient = new MqttFactory().CreateMqttClient();

        // Подписываемся на события подключения
        mqttClient.ConnectedAsync += e =>
        {
            Console.WriteLine("Клиент ИЗДАТЕЛЬ успешно подключен к брокеру.");
            return Task.CompletedTask;
        };

        // Подписываемся на события отключения
        mqttClient.DisconnectedAsync += e =>
        {
            Console.WriteLine("Клиент ИЗДАТЕЛЬ отключен от брокера.");
            return Task.CompletedTask;
        };

        // Подключаемся к брокеру
        await mqttClient.ConnectAsync(options);

        string input = "";
        while (input != "qqq")
        {
            // Считываем сообщение от пользователя
            Console.Write("Введите сообщение для отправки (или 'qqq' для завершения): ");
            input = Console.ReadLine() ?? " ";

            // Если пользователь ввел 'qqq', выходим из цикла
            if (input == "qqq")
                break;

            byte[] payload;

            if (input == "hhh")
            {
                // Пример байтового массива для отправки
                payload = new byte[] { 0x01, 0x02, 0x03 };
                Console.WriteLine("Отправляются байты: 01 02 03");
            }
            else
            {
                // Преобразуем строку в байты
                payload = Encoding.UTF8.GetBytes(input);
            }

            // Создаем сообщение для публикации
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("test/topic") // Укажите топик, в который хотите публиковать сообщение
                .WithPayload(payload) // Сообщение в виде байтового массива
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce) // QoS уровень 2 (Exactly once)
                .WithRetainFlag(false)
                .Build();

            // Публикуем сообщение
            await mqttClient.PublishAsync(message);
            Console.WriteLine("Сообщение опубликовано.");
        }

        // Отключаем клиента
        await mqttClient.DisconnectAsync();
        Console.WriteLine("Клиент отключен.");
    }
}
